using Example.Common.Models;
using Example.Common.Services.IServices;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Common.Services
{
    /// <summary>
    /// Dịch vụ gửi message lên Kafka: tạo producer tạm theo cấu hình, produce JSON kèm headers, flush và đóng.
    /// Không giữ producer lâu dài — mỗi lần gọi ProducerAsync tạo producer mới (phù hợp tần suất thấp).
    /// </summary>
    public class KafkaProducerService : IKafkaProducerService
    {
        private readonly ILogger<KafkaProducerService> _logger;

        public KafkaProducerService(ILogger<KafkaProducerService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gửi một message JSON lên topic: build producer từ configuration, produce, flush tối đa 10s.
        /// Trả ResponseData thành công kèm payload đã gửi; lỗi được log và bọc trong Message.
        /// </summary>
        public async Task<ResponseData<object>> ProducerAsync(string topicName, string jsonData, Headers headers, ProducerConfig configuration)
        {
            try
            {
                using (var producer = new ProducerBuilder<Null, string>(configuration).Build())
                {
                    await producer.ProduceAsync(topicName, new Message<Null, string> { Value = jsonData, Headers = headers });
                    producer.Flush(TimeSpan.FromSeconds(10));
                    return new ResponseData<object>(true, jsonData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ResponseData<object>(ex.Message);
            }
        }
    }
}
