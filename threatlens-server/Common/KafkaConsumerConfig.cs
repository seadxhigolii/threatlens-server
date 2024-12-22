using Confluent.Kafka;

namespace threatlens_server.Common
{
    public class KafkaConsumerConfig
    {
        public string BootstrapServers { get; set; } = "localhost:29092";
        public string GroupId { get; set; } = "network-packets-consumer-group";
        public string Topic { get; set; } = "network-packets";
    }
}
