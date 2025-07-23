using System.Collections.Generic;
using UnityEngine;
using Unity.InferenceEngine;

namespace XrAiYolo
{
    public class YoloBoxer
    {
        public static List<XrAiBoundingBox> ToBoundBoxes(string[] labels, Tensor<float> output, Tensor<int> labelIds, float imageWidth, float imageHeight)
        {
            List<XrAiBoundingBox> boundingBoxes = new();

            var scaleX = imageWidth / 640;
            var scaleY = imageHeight / 640;

            // var halfWidth = imageWidth / 2;
            // var halfHeight = imageHeight / 2;

            int boxesFound = output.shape[0];
            if (boxesFound <= 0) return boundingBoxes;

            var maxBoxes = Mathf.Min(boxesFound, 200);

            for (var n = 0; n < maxBoxes; n++)
            {
                float yoloCenterX = output[n, 0];
                float yoloCenterY = output[n, 1];
                // float yoloWidth = output[n, 2];
                // float yoloHeight = output[n, 3];

                float centerX = yoloCenterX * scaleX;
                float centerY = yoloCenterY * scaleY;

                var classname = labels[labelIds[n]].Replace(" ", "_");

                var box = new XrAiBoundingBox
                {
                    CenterX = centerX,
                    CenterY = centerY,
                    ClassName = classname,
                    Width = output[n, 2] * scaleX,
                    Height = output[n, 3] * scaleY,
                };

                boundingBoxes.Add(box);
            }

            return boundingBoxes;
        }
    }
}