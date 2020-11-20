using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisionAPI.Models;

namespace VisionAPI.Services
{
    public class MSVisionAPIService
    {
        private VisionAPICredentials _credentials;
        private ComputerVisionClient _client;

        public MSVisionAPIService(IOptions<VisionAPICredentials> credentials)
        {
            _credentials = credentials.Value;
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

        public async Task<ImageAnalysis> AnalyzeImageInStreamAsync(Stream imageUrl, List<VisualFeatureTypes?> features)
            => await _client.AnalyzeImageInStreamAsync(imageUrl, features);

        public async Task<ImageAnalysis> AnalyzeImageAsync(string url, List<VisualFeatureTypes?> features)
            => await _client.AnalyzeImageAsync(url, features);
    }
}
