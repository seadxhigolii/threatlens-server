using Confluent.Kafka;

namespace threatlens_server.Common
{
    public static class KafkaConfig
    {
        public const string Topic = "network-packets";
        public static readonly ProducerConfig ProducerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092"
        };

        public static readonly ConsumerConfig ConsumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092",
            GroupId = "threatlens-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
    }
}
