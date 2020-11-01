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

namespace ConvAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        public static string StorageDirectory = Path.Combine(Environment.CurrentDirectory,"convappimages");
        private readonly ILogger _logger;

        public ImagesController(ILogger<ImagesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{filename}")]
        public IActionResult GetImage(string filename)
        {
            _logger.LogInformation($"recived GET request for filename {filename}");

            var path = Path.Combine(StorageDirectory, string.Join("", filename.Take(2)),filename);

            if (System.IO.File.Exists(path))
            {
                return Ok(new FileStream(path, FileMode.Open));
            }

            return NotFound();
                
        }

        [HttpDelete("{filename}")]
        public IActionResult DeleteImage(string filename)
        {
            _logger.LogInformation($"recived DELETE request for filename {filename}");

            var path = Path.Combine(StorageDirectory, string.Join("", filename.Take(2)), filename);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return Ok();
            }

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
                string fileName = string.Empty;
                string filePath = string.Empty;
                bool dupe = true;   // 중복 검증 flag. 루프 진입을 위해 초기값 true
                while (dupe)
                {
                    fileName = GenerateFileName() + "." + MimeTypesMap.GetExtension(file.ContentType);
                    filePath = Path.Combine(Directory.CreateDirectory(Path.Combine(StorageDirectory, string.Join("", fileName.ToCharArray().Take(2)))).FullName, fileName);
                    dupe = System.IO.File.Exists(filePath);
                }

                _logger.LogInformation($"image saved - {filePath}");

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await data.CopyToAsync(fileStream);
                    fileStream.Close();
                }

                fileNameList.Add(fileName);
            }

            return string.Join(",",fileNameList.ToArray());

            // TODO 이미지 저장 후 관련 포스팅 항목의 images 컬럼 업데이트까지 진행하고 싶음

        }

        // 12자리 랜덤파일이름 생성기
        private static string GenerateFileName()
        {
            var arr = "0123456789abcdefghijklmnopqrstuvwxyz".ToCharArray();

            char[] res = new char[12];
            var random = new Random();
            for (int i = 0; i < 12; i++)
            {
                int idx = random.Next(0, arr.Length - 1);
                res[i] = arr[idx];
            }

            return string.Join("", res);
        }
    }
}
