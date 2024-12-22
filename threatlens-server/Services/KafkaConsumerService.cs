using Confluent.Kafka;
using System.Diagnostics;
using System.Text.Json;
using threatlens_server.Common;
using threatlens_server.Models;

namespace threatlens_server.Services
{
    public class KafkaConsumerService
    {
        private readonly ConsumerConfig _config;
        private readonly string _topic;
        private readonly MlModelService _mlModelService;

        public KafkaConsumerService(KafkaConsumerConfig config, MlModelService mlModelService)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = config.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _topic = config.Topic;
            _mlModelService = mlModelService;
        }

        public void ConsumeMessages(CancellationToken cancellationToken)
        {
            using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();

            consumer.Subscribe(_topic);

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(cancellationToken);
                        Debug.WriteLine($"Message: {consumeResult.Message.Value}");

                        var inputData = JsonSerializer.Deserialize<ModelInput>(consumeResult.Message.Value);

                        if (inputData == null)
                        {
                            Debug.WriteLine("Received invalid input data");
                            continue;
                        }

                        var prediction = _mlModelService.Predict(inputData);

                        if (prediction.Prediction)
                        {
                            Debug.WriteLine($"Anomaly detected! SrcIp: {inputData.SrcIp}, Score: {prediction.Score}");
                        }
                        else
                        {
                            Debug.WriteLine($"Normal traffic. SrcIp: {inputData.SrcIp}, Score: {prediction.Score}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Consume error: {e.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Closing consumer...");
            }
            finally
            {
                consumer.Close();
            }
        }

    }
}
