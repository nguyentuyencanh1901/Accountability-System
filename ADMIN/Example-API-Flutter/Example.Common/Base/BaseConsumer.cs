using Example.Common.Const;
using Example.Common.Utilities.Helper;
using Confluent.Kafka;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Example.Common.Base
{
    public class BaseConsumer : BackgroundService
    {
        protected readonly IWebHostEnvironment _hostingEnv;
        protected readonly ConsumerConfig _consumerConfig;

        public BaseConsumer(IWebHostEnvironment hostingEnv/*, string groupId*/)
        {
            _hostingEnv = hostingEnv;
            SecurityProtocol securityProtocol = _hostingEnv.EnvironmentName.Equals("Development") ? SecurityProtocol.SaslPlaintext : SecurityProtocol.SaslSsl;

            _consumerConfig = new ConsumerConfig
            {
                //GroupId = groupId,
                //GroupId = Guid.NewGuid().ToString(), // test thì để groupId random để lấy all data trong topic
                BootstrapServers = StaticVariable.KafkaSettingsModel.Bootstrapservers,
                // AutoOffsetReset.Earliest: Lấy all data với groupId join lần đầu
                // với groupId đã join trước đó thì sẽ lấy data dựa theo offset (đảm bảo không bị trùng lặp data, không bị mất data cũ khi consumer join lại)
                AutoOffsetReset = AutoOffsetReset.Earliest,
                //AutoOffsetReset = AutoOffsetReset.Latest,

                SecurityProtocol = securityProtocol,
                SaslMechanism = SaslMechanism.Plain,
                SaslUsername = StaticVariable.KafkaSettingsModel.SaslUsername,
                SaslPassword = StaticVariable.KafkaSettingsModel.SaslPassword,
                SslCaLocation = FileHelper.GetFileSSL(), // Đường dẫn tới tệp chứng chỉ CA (nếu sử dụng SSL)
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
