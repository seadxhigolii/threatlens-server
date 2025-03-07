using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using threatlens_server.Models;
using System.Diagnostics;

namespace threatlens_server.Services
{
    public class MlModelService
    {
        private readonly InferenceSession _session;
        private readonly LabelEncoder _labelEncoder;
        private readonly string _inputName;
        private readonly string _outputName;

        public MlModelService(string modelPath)
        {
            _session = new InferenceSession(modelPath);
            _labelEncoder = new LabelEncoder();

            _inputName = _session.InputMetadata.Keys.First();
            _outputName = _session.OutputMetadata.Keys.First();

            Debug.WriteLine($"Using Input Name: {_inputName}");
            Debug.WriteLine($"Using Output Name: {_outputName}");
        }

        public ModelOutput Predict(ModelInput input)
        {
            try
            {
                int srcIpEncoded = _labelEncoder.Encode(input.SrcIp);
                int dstIpEncoded = _labelEncoder.Encode(input.DstIp);
                int protoEncoded = _labelEncoder.Encode(input.Proto);

                var inputTensor = new DenseTensor<float>(new float[]
                {
                    srcIpEncoded, dstIpEncoded, input.Sport, input.Dsport,
                    protoEncoded, input.Sbytes, input.Dbytes, input.Sttl
                }, new[] { 1, 8 });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)
                };

                using var results = _session.Run(inputs);

                if (results == null || !results.Any())
                    throw new InvalidOperationException("No outputs returned from the ONNX model.");

                var probabilitiesResult = results.FirstOrDefault(r => r.Name == "probabilities");
                if (probabilitiesResult == null)
                    throw new InvalidOperationException("'probabilities' output not found.");

                var probabilities = probabilitiesResult.AsEnumerable<float>().ToArray();
                if (probabilities == null || probabilities.Length == 0)
                    throw new InvalidOperationException("Model returned an empty probabilities array.");

                float score = probabilities[1];
                bool prediction = score >= 0.5f;

                return new ModelOutput
                {
                    Prediction = prediction,
                    Score = score
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Prediction failed: {ex.Message}");
                throw;
            }
        }
    }
}
