using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VisionAPI.Helper
{
    public static class ValidateFile
    {
        public static bool IsValidImg(IFormFile file)
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
        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}
