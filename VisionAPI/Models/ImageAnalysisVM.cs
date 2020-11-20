using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VisionAPI.Models
{
    public class ImageAnalysisVM
    {
        public ImageAnalysis ImageAnalysis { get; set; }
        public string Url { get; set; } = new string("https://agazia.net/assets/img/hero-bg.jpg");
    }
}
