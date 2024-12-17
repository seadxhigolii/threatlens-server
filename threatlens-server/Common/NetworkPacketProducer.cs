using Confluent.Kafka;
using System.Text.Json;

namespace threatlens_server.Common
{
    public class NetworkPacketProducer
    {
        public static async Task ProducePacketAsync(object packet)
        {
            using var producer = new ProducerBuilder<string, string>(KafkaConfig.ProducerConfig).Build();

            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(packet)
            };

            await producer.ProduceAsync(KafkaConfig.Topic, message);
        }
    }
}
