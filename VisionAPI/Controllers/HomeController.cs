using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Threading.Tasks;
using VisionAPI.Models;
using VisionAPI.Services;
using System.IO;
using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace VisionAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MSVisionAPIService _visioinAPIService;
        private readonly IWebHostEnvironment _hostingEnv;

        public HomeController(ILogger<HomeController> logger
            ,MSVisionAPIService visionAPIService
            ,IWebHostEnvironment hostingEnv)
        {
            _logger = logger;
            _visioinAPIService = visionAPIService;
            _hostingEnv = hostingEnv;
        }

        public IActionResult Index()
        {
            var model = new ImageAnalysisVM();
            try
            {
                var response = _visioinAPIService.AnalyzeImageAsync(model.Url, GetFeatureTypes()).Result;
                model.ImageAnalysis = response;
                model.Filename = Guid.NewGuid().ToString();
                model.Message = "Success";
            }
            catch (Exception)
            {
                model.Message = $"Error: An error occured while analyzing the image";
                throw;
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult LoadResult(ImageAnalysisVM model)
        {
            return PartialView("_Result", model);
        }

        [HttpPost]
        public IActionResult DeletePicture(string Id)
            => Json(DeletePicById(Id));

        [HttpPost]
        public async Task<IActionResult> UploadLocal()
        {
            var imageAnalysisVM = new ImageAnalysisVM();

            if (Request.Form.Files.Count > 0)
            {
                try
                {
                    var files = Request.Form.Files;

                    foreach (var file in files)
                    {
                        if (IsValidImg(file))
                        {
                            //Upload Local
                            var filename = Guid.NewGuid().ToString();
                            var webPath = _hostingEnv.WebRootPath;
                            var path = Path.Combine("", webPath + @"\img\" + filename + ".png");
                            var url = $"{Request.Scheme}://{Request.Host}/img/{filename}.png";
                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            using (var imageStream = new FileStream(path, FileMode.Open))
                            {
                                ImageAnalysis res = _visioinAPIService
                                    .AnalyzeImageInStreamAsync(imageStream, GetFeatureTypes()).Result;
                                imageAnalysisVM.ImageAnalysis = res;
                                imageAnalysisVM.Url = url;
                                imageAnalysisVM.Filename = filename;
                                imageAnalysisVM.Message = "Success";
                                return Json(imageAnalysisVM);
                            }
                        }
                        else
                        {
                            imageAnalysisVM.Message = "Error: Please make sure to choose an image (.jpeg, .png, .gif) less than or equal to 4 MB and try again!";
                        }
                    }
                }
                catch (Exception)
                {
                    imageAnalysisVM.Message = $"Error: An error occured while analyzing the image.";
                }
                return Json(imageAnalysisVM);
            }
            else
            {
                try
                {
                    var picUrl = Request.Form.FirstOrDefault(x => x.Key == "picurl").Value;
                    var response = _visioinAPIService.AnalyzeImageAsync(picUrl, GetFeatureTypes()).Result;
                    imageAnalysisVM.ImageAnalysis = response;
                    imageAnalysisVM.Url = picUrl;
                    imageAnalysisVM.Filename = Guid.NewGuid().ToString();
                    imageAnalysisVM.Message = "Success";

                    return Json(imageAnalysisVM);
                }
                catch (Exception)
                {
                    imageAnalysisVM.Message = $"Error: The given URL is not valid!";
                }
                return Json(imageAnalysisVM);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public bool DeleteFilesInFolder()
        {
            var webPath = _hostingEnv.WebRootPath;
            var path = Path.Combine("", webPath + @"\img\");

            if (Directory.GetFiles(path).Length > 0)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (var file in di.GetFiles())
                {
                    file.Delete();
                }
                return true;
            }

            return false;
        }

        public bool DeletePicById(string Id)
        {
            var webPath = _hostingEnv.WebRootPath;
            var path = Path.Combine("", webPath + @"\img\" + Id + ".png");

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                return true;
            }
            return false;
        }

        public bool IsValidImg(IFormFile file)
        {
            var types = new[] { "image/png", "image/jpeg", "image/gif" };
            var megaByteSize = ConvertBytesToMegabytes(file.Length);

            foreach (var type in types)
            {
                if (type == file.ContentType & megaByteSize <= 4)
                    return true;
            }

            return false;
        }
        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public List<VisualFeatureTypes?> GetFeatureTypes()
            => new List<VisualFeatureTypes?>()
            {
                VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                VisualFeatureTypes.Objects
            };

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
