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

namespace VisionAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MSVisionAPIService _visioinAPIService;
        private readonly IHostingEnvironment _hostingEnv;

        public HomeController(ILogger<HomeController> logger
            ,MSVisionAPIService visionAPIService
            ,IHostingEnvironment hostingEnv)
        {
            _logger = logger;
            _visioinAPIService = visionAPIService;
            _hostingEnv = hostingEnv;
        }

        public IActionResult Index()
        {
            var model = _visioinAPIService
                .AnalyzeImageAsync("https://agazia.net/assets/img/hero-bg.jpg",
                 GetFeatureTypes()).Result;
            return View(model);
        }

        [HttpPost]
        public IActionResult LoadResult(ImageAnalysisVM model)
        {
            return PartialView("_Result", model);
        }

        [HttpPost]
        public IActionResult DeletePicture()
        {
            var msg = "Nichts zu löschen!";
            var filename = "pic.png";
            var webPath = _hostingEnv.WebRootPath;
            var path = Path.Combine("", webPath + @"\img\" + filename);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                msg = "Das Bild wurde gelöscht!";
            }
            return Json(msg);
        }

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
                            var filename = "pic.png";
                            var webPath = _hostingEnv.WebRootPath;
                            var path = Path.Combine("", webPath + @"\img\" + filename);
                            var url = string.Format("{0}://{1}/img/{2}", Request.Scheme, Request.Host, filename);
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
                                return Json(imageAnalysisVM);
                            }
                        }
                        else
                        {
                            imageAnalysisVM.Url = "Error: Please make sure to choose an image (.jpeg, .png, .gif) less than 5MB and try again!";
                        }
                    }
                }
                catch (Exception ex)
                {
                    imageAnalysisVM.Url = ex.ToString();
                }
                return Json(imageAnalysisVM);
            }
            else
            {
                imageAnalysisVM.Url = "Picture not found!";
                return Json(imageAnalysisVM);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public bool IsValidImg(IFormFile file)
        {
            var types = new[] { "image/png", "image/jpeg", "image/gif" };
            var megaByteSize = ConvertBytesToMegabytes(file.Length);

            foreach (var type in types)
            {
                if (type == file.ContentType & megaByteSize <= 5)
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
