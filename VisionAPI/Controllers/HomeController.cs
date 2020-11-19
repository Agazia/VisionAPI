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
            return View();
        }

        [HttpPost]
        public IActionResult LoadResult(ImageAnalysisVM model)
        {
            return PartialView("_Result", model);
        }

        [HttpPost]
        public IActionResult DeletePicture()
        {
            var filename = "pic.png";
            var webPath = _hostingEnv.WebRootPath;
            var path = Path.Combine("", webPath + @"\img\" + filename);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> UploadLocal()
        {
            if (Request.Form.Files.Count > 0)
            {
                var returnobj = "";

                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                    VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                    VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                    VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Objects
                };

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
                                    .AnalyzeImageInStreamAsync(imageStream, features).Result;

                                var imageAnalysisVM = new ImageAnalysisVM()
                                {
                                    ImageAnalysis = res,
                                    Url = url
                                };
                                return Json(imageAnalysisVM);
                            }
                        }
                        else
                        {
                            returnobj = "Filetype not supported. Please make sure to choose an image (.jpeg, .png, .gif) less than 5MB and try again!";
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Json("Error occurered. Error details: " + ex.Message);
                }
                return Json(returnobj);
            }
            else
            {
                return Json("No files selected.");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public bool IsValidImg(IFormFile file)
        {
            var types = new[] { "image/png", "image/jpeg", "image/gif" };

            foreach (var type in types)
            {
                if (type == file.ContentType & ConvertBytesToMegabytes(file.Length) < 6)
                    return true;
            }

            return false;
        }
        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
