using Microsoft.AspNetCore.Hosting;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisionAPI.Models;

namespace VisionAPI.Services
{
    public class TextToSpeechAPIService : ITextToSpeech
    {
        public TextToSpeechAPICredentials Options { get; }
        private readonly IWebHostEnvironment _hostingEnv;

        public TextToSpeechAPIService(IOptions<TextToSpeechAPICredentials> optionsAccessor, IWebHostEnvironment hostingEnv)
        {
            Options = optionsAccessor.Value;
            _hostingEnv = hostingEnv;
        }
        public async Task TranslateText(string content, string path)
        {
            var config = SpeechConfig.FromSubscription(Options.SubscriptionKey, Options.Region);

            using var audioConfig = AudioConfig.FromWavFileOutput(path);
            using var synthesizer = new SpeechSynthesizer(config, audioConfig);
            await synthesizer.SpeakTextAsync(content);
        }

        public async Task<SpeechAnalyseVM> AnalyseSpeechAsync(string content, string baseUrl)
        {
            var speechModel = new SpeechAnalyseVM() { Filename = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{Guid.NewGuid()}" };

            try
            {
                var webPath = _hostingEnv.WebRootPath;
                var path = Path.Combine("", webPath + @"\sound\" + speechModel.Filename + ".wav");

                await TranslateText(content, path);
                speechModel.Url = $"{baseUrl}sound/{speechModel.Filename}.wav";
                speechModel.Content = content;
                speechModel.Message = "Success";
            }
            catch (Exception)
            {
                speechModel.Message = $"Error: An error occured while analyzing the text.";
            }
            return speechModel;
        }
    }

    public interface ITextToSpeech
    {
        Task TranslateText(string content, string path);
        Task<SpeechAnalyseVM> AnalyseSpeechAsync(string content, string baseUrl);
    }
}
