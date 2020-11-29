using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisionAPI.Models;
using VisionAPI.Helper;

namespace VisionAPI.Services
{
    public class MSVisionAPIService
    {
        private VisionAPICredentials _credentials;
        private ComputerVisionClient _client;
        private readonly IWebHostEnvironment _hostingEnv;


        public MSVisionAPIService(IOptions<VisionAPICredentials> credentials, IWebHostEnvironment hostingEnv)
        {
            _credentials = credentials.Value;
            _hostingEnv = hostingEnv;
            _client = Authenticate();
        }

        private ComputerVisionClient Authenticate()
        {
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(_credentials.SubscriptionKey))
            {
                Endpoint = _credentials.EndPoint
            };
            return _client;
        }

        public async Task<ImageAnalysis> AnalyzeImageInStreamAsync(Stream imageUrl)
            => await _client.AnalyzeImageInStreamAsync(imageUrl, GetFeatureTypes());

        public async Task<ImageAnalysis> AnalyzeImageAsync(string url)
            => await _client.AnalyzeImageAsync(url, GetFeatureTypes());

        public async Task<ImageAnalysisVM> AnalyzeUrlImage(string url)
            => new ImageAnalysisVM()
            {
                ImageAnalysis = await AnalyzeImageAsync(url),
                Filename = Guid.NewGuid().ToString(),
                Url = url,
                Message = "Success"
            };

        public async Task<ImageAnalysisVM> AnalyzeImageFileAsync(IFormFile file, string baseUrl)
        {
            var imageAnalysisVM = new ImageAnalysisVM()
            {
                Filename = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{Guid.NewGuid()}"
            };

            var webPath = _hostingEnv.WebRootPath;
            var path = Path.Combine("", webPath + @"\img\" + imageAnalysisVM.Filename + ".png");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            using (var imageStream = new FileStream(path, FileMode.Open))
            {
                imageAnalysisVM.ImageAnalysis = await AnalyzeImageInStreamAsync(imageStream);
                imageAnalysisVM.Url = $"{baseUrl}img/{imageAnalysisVM.Filename}.png";
                imageAnalysisVM.Message = "Success";
            }

            return imageAnalysisVM;
        }

        public async Task<ImageAnalysisVM> AnalyzeImageFileOrUrlAsync(IFormCollection form, string urlBase)
        {
            var imageAnalysisVM = new ImageAnalysisVM();

            if (form.Files.Count > 0)
            {
                try
                {
                    foreach (var file in form.Files)
                    {
                        if (ValidateFile.IsValidImg(file))
                        {
                            imageAnalysisVM = await AnalyzeImageFileAsync(file, urlBase);
                            imageAnalysisVM.Message = "Success";
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
                return imageAnalysisVM;
            }
            else
            {
                try
                {
                    var picUrl = form.FirstOrDefault(x => x.Key == "picurl").Value;

                    imageAnalysisVM = await AnalyzeUrlImage(picUrl);
                    imageAnalysisVM.Message = "Success";
                }
                catch (Exception)
                {
                    imageAnalysisVM.Message = $"Error: The given URL is not valid!";
                }
                return imageAnalysisVM;
            }
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
    }
}
