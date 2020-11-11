using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HttpMultipartParser;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Http.Extensions;
using HeyRed.Mime;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Base62;

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

        [HttpDelete("{filename}")]
        public async Task<ActionResult> DeleteImage(string filename)
        {
            _logger.LogInformation($"recived DELETE request for filename {filename}");

            if (await _blob.GetBlobClient(filename).DeleteIfExistsAsync())
                return Ok();
            else
                return NotFound();
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<string>> PostImage()
        {
            // multipart/form-data 타입의 POST request 처리 -> 이미지 업로드
            // TODO 저장한 이미지의 파일명 리턴
            List<string> fileNameList = new List<string>();
            _logger.LogInformation("received POST request with multipart/form-data body");
            MultipartFormDataParser parser;
            try
            {
                parser = await MultipartFormDataParser.ParseAsync(Request.Body);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(e);
            }
            
            foreach (var file in parser.Files)
            {
                _logger.LogInformation($"formdata found - ({file.ContentDisposition}|{file.Name}|{file.FileName}|{file.ContentType})");

                Stream data = file.Data;
                if (data.Length == 0)
                    continue;

                // 파일명 12자리 숫자+소문자 문자열로 설정
                // 디렉토리는 파일명의 첫 2자리를 이름으로 갖는 폴더 안에 저장
                string fileName;
                string filePath = string.Empty;

                BlobClient blobClient;
                bool dupe;
                do
                {
                    fileName = GenerateFileName();
                    blobClient = _blob.GetBlobClient(fileName);
                    dupe = await blobClient.ExistsAsync();
                } while (dupe);

                _logger.LogInformation($"image saved - {filePath}");
                await blobClient.UploadAsync(data,new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } });
                fileNameList.Add(fileName);
            }

            return string.Join(";",fileNameList.ToArray());

            // TODO 이미지 저장 후 관련 포스팅 항목의 images 컬럼 업데이트까지 진행하고 싶음
        }

        // 16자리 랜덤파일이름 생성기
        private static string GenerateFileName()
        {
            var t = BitConverter.GetBytes(DateTime.UtcNow.Ticks).ToBase62() + "_";

            var base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            var len = 15 - t.Length;

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
