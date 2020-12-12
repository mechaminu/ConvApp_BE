using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Base62;
using HttpMultipartParser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {

        private readonly ILogger _logger;
        public BlobContainerClient _blob;

        public ImagesController(ILogger<ImagesController> logger, BlobServiceClient blob)
        {
            _logger = logger;
            _blob = blob.GetBlobContainerClient("images");
        }

        /// <summary>
        ///     파일명에 해당하는 이미지를 Azure blob container에서 삭제
        /// </summary>
        /// <param name="filename">파일명</param>
        /// <returns>성공시 Ok, 실패시 NotFound</returns>
        [HttpDelete("{filename}")]
        public async Task<ActionResult> DeleteImage(string filename)
        {
            _logger.LogInformation($"[DELETE] Deleting {filename}");

            if (await _blob.GetBlobClient(filename).DeleteIfExistsAsync())
                return Ok();
            else
                return NotFound();
        }

        /// <summary>
        ///     multipart/form-data POST request로 받은 이미지의 업로드
        /// </summary>
        /// <returns>파일명 목록 (delimited with ;)</returns>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> PostImage()
        {
            // 
            // TODO 저장한 이미지의 파일명 리턴
            _logger.LogInformation("[POST] Uploading images");
            MultipartFormDataParser parser;
            try
            {
                parser = await MultipartFormDataParser.ParseAsync(Request.Body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uploading images failed");
                return BadRequest(ex);
            }

            List<string> fileNameList = new List<string>();
            try
            {
                foreach (var file in parser.Files)
                {
                    Stream data = file.Data;
                    if (data.Length == 0)
                        continue;

                    var fileName = await PostImageStream(data, file.ContentType);

                    fileNameList.Add(fileName);
                }

                return string.Join(";", fileNameList.ToArray());
            }
            catch
            {
                try
                {
                    foreach (var fileName in fileNameList)
                    {
                        await DeleteImage(fileName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed image deletion attempt. leftover:{string.Join(";", fileNameList.ToArray())}");
                }

                return BadRequest();
            }
        }

        /// <summary>
        ///     이미지를 스트림으로 받아 Azure blob container에 저장
        /// </summary>
        /// <param name="data">이미지 stream 데이터</param>
        /// <param name="type">이미지 MIME 형식</param>
        /// <returns>blob container에 저장된 이미지 파일명</returns>
        public async Task<string> PostImageStream(Stream data, string type) {
            try
            {
                string fileName;    
                BlobClient blobClient;
                bool dupe;

                do
                {
                    fileName = GenerateFileName();
                    blobClient = _blob.GetBlobClient(fileName);
                    dupe = await blobClient.ExistsAsync();
                } while (dupe);

                await blobClient.UploadAsync(data, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = type } });

                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uploading an image failed");
                throw;
            }
        }

        /// <summary>
        ///     시간 및 난수 기반 랜덤 Base62 파일명을 생성
        /// </summary>
        /// <returns>16자리 Base62 파일명</returns>
        private static string GenerateFileName()
        {
            
            var t = BitConverter.GetBytes(DateTime.UtcNow.Ticks).ToBase62() + "_";

            var base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var len = 16 - t.Length;

            char[] res = new char[len];
            var random = new Random();
            for (int i = 0; i < len; i++)
            {
                int idx = random.Next(0, base62Chars.Length - 1);
                res[i] = base62Chars[idx];
            }

            return t + string.Join("", res);
        }
    }
}
