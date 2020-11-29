using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VisionAPI.Models
{
    public class CognetiveContainer
    {
        public ImageAnalysisVM ImageAnalysisVM { get; set; }
        public SpeechAnalyseVM SpeechAnalyseVM { get; set; }
        public string Message { get; set; } = new string("Success");
    }
}
