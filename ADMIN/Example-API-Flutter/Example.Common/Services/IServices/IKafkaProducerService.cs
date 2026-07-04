using Example.Common.Models;
using Confluent.Kafka;

namespace Example.Common.Services.IServices
{
    public interface IKafkaProducerService
    {
        Task<ResponseData<object>> ProducerAsync(string topicName, string jsonData, Headers headers, ProducerConfig configuration);
    }
}
