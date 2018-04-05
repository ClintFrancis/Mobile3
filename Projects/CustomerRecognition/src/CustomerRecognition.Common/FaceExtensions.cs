using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerRecognition.Common
{
    public static class FaceExtensions
    {
        public static double Size(this FaceRectangle rect)
        {
            return rect.Width * rect.Height;
        }
    }
}
