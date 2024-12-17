using Confluent.Kafka;
using System.Text.Json;
using threatlens_server.Common;

namespace threatlens_server.Features
{

    public class NetworkPacketConsumer
    {
        private readonly MlModelLoader _modelLoader = new();

        public async Task ConsumePacketsAsync()
        {
            using var consumer = new ConsumerBuilder<string, string>(KafkaConfig.ConsumerConfig).Build();
            consumer.Subscribe(KafkaConfig.Topic);

            try
            {
                while (true)
                {
                    var consumeResult = consumer.Consume();
                    var packet = JsonSerializer.Deserialize<Models.InputData>(consumeResult.Message.Value);

                    if (_modelLoader.Predict(packet))
                    {
                        Console.WriteLine("Anomaly detected! Alert!");
                    }
                    else
                    {
                        Console.WriteLine("Packet is normal.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
        }
    }
}
