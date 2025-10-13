#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Unity.InferenceEngine;

namespace XrAiAccelerator
{

    [CustomEditor(typeof(YoloModelConverter))]
    public class YoloModelConverterEditor : Editor
    {
        private YoloModelConverter _targetClass;
        private float _iouThreshold;
        private float _scoreThreshold;
        private string _filePath;

        public void OnEnable()
        {
            _targetClass = (YoloModelConverter)target;
            _iouThreshold = serializedObject.FindProperty("_iouThreshold").floatValue;
            _scoreThreshold = serializedObject.FindProperty("_scoreThreshold").floatValue;
            _filePath = serializedObject.FindProperty("_filePath").stringValue;
        }

        public override void OnInspectorGUI()
        {
            _ = DrawDefaultInspector();

            if (GUILayout.Button("Generate YOLOv11 Sentis model with Non-Max-Supression layer"))
            {
                OnEnable(); // Get the latest values from the serialized object
                ConvertModel(); // convert the ONNX model to sentis
            }
        }

        private void ConvertModel()
        {
            // We create a new model that processes the output of the original model
            Model model = ModelLoader.Load(_targetClass._onnxModel);
            FunctionalGraph graph = new();
            FunctionalTensor input = graph.AddInput(model, 0);

            // The Non-Max-Suppression (NMS) algorithm requires co-ordinates to originate fomr the corner
            // This it the algorithm used to select the best boxes when there are multiple overlapping boxes
            float[] CENTERS_TO_CORNERS_MAP = new[] {
                1,      0,      1,      0,
                0,      1,      0,      1,
            -0.5f,   0,      0.5f,   0,
                0,     -0.5f,   0,      0.5f
            };

            FunctionalTensor CENTERS_TO_CORNERS_TENSOR = Functional.Constant(new TensorShape(4, 4), CENTERS_TO_CORNERS_MAP);

            FunctionalTensor[] originalOutputs = Functional.Forward(model, input);  //shape(1,N,85)

            // YOLO11-seg output0 is a tensor of shape (1, 116, 8400)
            //  1 is the batch size
            //  116 = 4 (x,y,w,h) + 80 (class scores) + 32 (mask weights)
            //  8400 is the number of boxes (always the same)
            // YOLO11-seg output1 is a tensor of shape (1, 32, 160, 160)
            //  1 is the batch size
            //  32 contains 32 mask weights
            //  160 contains the height values
            //  160 contains the width values
            FunctionalTensor boxOutput = originalOutputs[0];
            FunctionalTensor maskWeightsOutput = originalOutputs[1];

            // Pull out the box coordinates, class scores and mask weights from output0
            FunctionalTensor allBoxCoords = boxOutput[0, ..4, ..].Transpose(0, 1);
            FunctionalTensor allScores = boxOutput[0, 4..84, ..].Transpose(0, 1);
            FunctionalTensor allMasks = boxOutput[0, 84.., ..].Transpose(0, 1);

            // Find the best score and associated label ids for each box, and ignore the others - basically
            // for a given box what object is it most likely to be
            FunctionalTensor scores = Functional.ReduceMax(allScores, 1);
            FunctionalTensor labelIds = Functional.ArgMax(allScores, 1);

            // Translate the box coordinates to corners - for the NMS calculation
            FunctionalTensor allBoxesCorners = Functional.MatMul(allBoxCoords, CENTERS_TO_CORNERS_TENSOR);

            // For overlapping boxes, find the indices of the boxes to keep using Non-Max Suppression
            FunctionalTensor indices = Functional.NMS(allBoxesCorners, scores, _iouThreshold, _scoreThreshold);

            // Stretch the indices to match the shapes of the other tensors
            FunctionalTensor indices4 = indices.Unsqueeze(-1).BroadcastTo(new[] { 4 });
            FunctionalTensor indices32 = indices.Unsqueeze(-1).BroadcastTo(new[] { 32 });

            FunctionalTensor selectedLabelIds = Functional.Gather(labelIds, 0, indices);
            FunctionalTensor selectedMasks = Functional.Gather(allMasks, 0, indices32);
            FunctionalTensor selectedBoxCoords = Functional.Gather(allBoxCoords, 0, indices4);

            // First reshape maskWeightsOutput [1,32,160,160] to have shape [32, 160*160]
            FunctionalTensor reshapedMaskWeights = Functional.Reshape(maskWeightsOutput, new[] { 1, 32, 160 * 160 })[0];

            // Now do matrix multiplication with selectedMasks [N, 32] and reshapedMaskWeights [32, 160*160]
            // This should give a result with shape [N, 160*160]
            FunctionalTensor maskWeights = Functional.MatMul(selectedMasks, reshapedMaskWeights);

            // Apply sigmoid and reshape back to [N, 160, 160]
            maskWeights = Functional.Sigmoid(maskWeights);

            // The -1 in the reshape parameters is a placeholder that means "infer this dimension"
            maskWeights = Functional.Reshape(maskWeights, new[] { -1, 160, 160 });

            // Compile and save the model
            Model modelFinal = graph.Compile(selectedBoxCoords, selectedLabelIds, maskWeights);
            ModelQuantizer.QuantizeWeights(QuantizationType.Uint8, ref modelFinal);
            ModelWriter.Save(_filePath, modelFinal);

            // Refresh assets
            AssetDatabase.Refresh();
        }
    }
}
#endif
