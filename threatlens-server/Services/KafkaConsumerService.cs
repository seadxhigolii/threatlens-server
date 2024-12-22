using Confluent.Kafka;
using System.Diagnostics;
using threatlens_server.Common;

namespace threatlens_server.Services
{
    public class KafkaConsumerService
    {
        private readonly ConsumerConfig _config;
        private readonly string _topic;

        public KafkaConsumerService(KafkaConsumerConfig config)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers,
                GroupId = config.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _topic = config.Topic;
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
                        Debug.WriteLine($"Message: {consumeResult.Message.Value}, Partition: {consumeResult.Partition}, Offset: {consumeResult.Offset}");
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
