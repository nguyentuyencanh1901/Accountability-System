using Example.Common.Models.AppSetting;
using Confluent.Kafka;

namespace Example.Common.Utilities.Helper
{
    public sealed class KafkaConfigHelper
    {
        private static readonly Lazy<KafkaConfigHelper> lazy = new Lazy<KafkaConfigHelper>(() => new KafkaConfigHelper());
        public static KafkaConfigHelper Instance { get { return lazy.Value; } }

        private KafkaConfigHelper() { }

        public ProducerConfig CreateProducerConfig(KafkaSettingsModel kafkaSetting, string sslCaLocation)
        {
            return new ProducerConfig()
            {
                BootstrapServers = kafkaSetting.Bootstrapservers,
                SecurityProtocol = (SecurityProtocol)kafkaSetting.SecurityProtocol,
                SaslMechanism = (SaslMechanism)kafkaSetting.SaslMechanism,
                SaslUsername = kafkaSetting.SaslUsername,
                SaslPassword = kafkaSetting.SaslPassword,
                SslCaLocation = sslCaLocation // Đường dẫn tới tệp chứng chỉ CA (nếu sử dụng SSL)
            };
        }

        public ConsumerConfig CreateConsumerConfig(KafkaSettingsModel kafkaSetting, string groupId, string sslCaLocation)
        {
            return new ConsumerConfig
            {
                GroupId = $"gid_{groupId}",
                // AutoOffsetReset.Earliest: Lấy all data với groupId join lần đầu
                // với groupId đã join trước đó thì sẽ lấy data dựa theo offset (đảm bảo không bị trùng lặp data, không bị mất data cũ khi consumer join lại)
                AutoOffsetReset = AutoOffsetReset.Earliest,
                //AutoOffsetReset = AutoOffsetReset.Latest,

                BootstrapServers = kafkaSetting.Bootstrapservers,
                SecurityProtocol = (SecurityProtocol)kafkaSetting.SecurityProtocol,
                SaslMechanism = (SaslMechanism)kafkaSetting.SaslMechanism,
                SaslUsername = kafkaSetting.SaslUsername,
                SaslPassword = kafkaSetting.SaslPassword,
                SslCaLocation = sslCaLocation // Đường dẫn tới tệp chứng chỉ CA (nếu sử dụng SSL)
            };
        }
    }
}
