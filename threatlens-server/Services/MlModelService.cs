using Microsoft.ML;
using threatlens_server.Models;

namespace threatlens_server.Services
{
    public class MlModelService
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly PredictionEngine<ModelInput, ModelOutput> _predictionEngine;

        public MlModelService(string modelPath)
        {
            _mlContext = new MLContext();
            _model = _mlContext.Model.Load(modelPath, out _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(_model);
        }

        public ModelOutput Predict(ModelInput input)
        {
            return _predictionEngine.Predict(input);
        }
    }
}
