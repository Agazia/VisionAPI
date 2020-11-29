using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VisionAPI.Models
{
    public class SpeechAnalyseVM
    {
        public string Filename { get; set; }
        public string Content { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
    }
}
