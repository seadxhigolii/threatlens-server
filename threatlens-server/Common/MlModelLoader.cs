using Microsoft.ML;

namespace threatlens_server.Common
{
    public class MlModelLoader
    {
        private readonly PredictionEngine<Models.InputData, PredictionOutput> _predictionEngine;

        public MlModelLoader()
        {
            var mlContext = new MLContext();
            var modelPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Threatlens\\AI Model\\dynamic_trained_model(2).zip";

            var loadedModel = mlContext.Model.Load(modelPath, out var inputSchema);
            _predictionEngine = mlContext.Model.CreatePredictionEngine<Models.InputData, PredictionOutput>(loadedModel);
        }

        public bool Predict(Models.InputData input)
        {
            var result = _predictionEngine.Predict(input);
            return result.LabelResult;
        }
    }

    public class PredictionOutput
    {
        public bool Prediction { get; set; }
        public float Probability { get; set; }
        public bool LabelResult { get; set; }
    }
}
