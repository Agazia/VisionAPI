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
using System.Globalization;

namespace VisionAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MSVisionAPIService _visioinAPIService;
        private readonly IWebHostEnvironment _hostingEnv;
        private readonly ITextToSpeech _textToSpeech;

        public HomeController(ILogger<HomeController> logger
            ,MSVisionAPIService visionAPIService
            ,IWebHostEnvironment hostingEnv
            ,ITextToSpeech textToSpeech
            )
        {
            _logger = logger;
            _visioinAPIService = visionAPIService;
            _hostingEnv = hostingEnv;
            _textToSpeech = textToSpeech;
        }

        public IActionResult Index()
        {
            return View(new CognetiveContainer()
            {
                ImageAnalysisVM = new ImageAnalysisVM() { Url = "https://agazia.net/assets/img/hero-bg.jpg" }
            });
        }

        [HttpPost]
        public IActionResult TextToSpeech(SpeechAnalyseVM model)
            => PartialView("_Speech", model);

        [HttpPost]
        public IActionResult PlaySpeech(SpeechAnalyseVM model)
            => PartialView("_PlaySpeech", model);

        [HttpPost]
        public async Task<IActionResult> UploadSpeech(string Id)
            => Json(await _textToSpeech.AnalyseSpeechAsync(Id, $"{Request.Scheme}://{Request.Host}/"));
        

        [HttpPost]
        public IActionResult LoadResult(CognetiveContainer model)
            => PartialView("_Result", model);

        [HttpPost]
        public IActionResult DeletePicture(string Id)
            => Json(DeletePicById(Id));

        [HttpPost]
        public async Task<IActionResult> AnalyzeImage()
            => Json(await _visioinAPIService.AnalyzeImageFileOrUrlAsync(Request.Form, $"{Request.Scheme}://{Request.Host}/"));

        [HttpPost]
        public async Task<IActionResult> AnalyzeImageAndSound()
        {
            var cognitiveContainer = new CognetiveContainer();

            try
            {
                cognitiveContainer.ImageAnalysisVM = await _visioinAPIService.AnalyzeImageFileOrUrlAsync(Request.Form, $"{Request.Scheme}://{Request.Host}/");
                if (cognitiveContainer.ImageAnalysisVM.Message == "Success")
                {
                    var content = cognitiveContainer.ImageAnalysisVM.ImageAnalysis.Description.Captions.FirstOrDefault().Text;
                    cognitiveContainer.SpeechAnalyseVM = await _textToSpeech.AnalyseSpeechAsync(content, $"{Request.Scheme}://{Request.Host}/");
                    if (cognitiveContainer.SpeechAnalyseVM.Message == "Success")
                    {
                        cognitiveContainer.Message = "Success";
                    }
                    else
                    {
                        cognitiveContainer.Message = cognitiveContainer.SpeechAnalyseVM.Message;
                    }
                }
                else
                {
                    cognitiveContainer.Message = cognitiveContainer.ImageAnalysisVM.Message;
                }
            }
            catch (Exception ex)
            {
                cognitiveContainer.Message = $"Error: {ex}";
            }
            
            return Json(cognitiveContainer);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DeleteAll()
            => Json(DeleteImgAndSound());

        public bool DeleteFilesInFolder(string path)
        {

            if (Directory.GetFiles(path).Length > 0)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (var file in di.GetFiles())
                {
                    try
                    {
                        var formatstring = "yyyyMMddHHmmss";
                        var filedate = DateTime.ParseExact(file.Name.Split('_').First(), formatstring, CultureInfo.InvariantCulture);
                        if (filedate < DateTime.Now.AddDays(-7))
                        {
                            file.Delete();
                        }
                    }
                    catch (Exception e)
                    {
                        e.ToString();
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public bool DeleteImgAndSound()
        {
            var webPath = _hostingEnv.WebRootPath;
            var path = Path.Combine("", webPath + @"\sound\");
            DeleteFilesInFolder(path);
            path = Path.Combine("", webPath + @"\img\");
            DeleteFilesInFolder(path);
            return true;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
