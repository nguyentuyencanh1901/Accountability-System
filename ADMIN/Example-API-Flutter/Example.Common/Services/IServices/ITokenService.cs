using Grpc.Core;

namespace Example.Common.Services.IServices
{
    public interface ITokenService
    {
        Task<Metadata> GetHeadersForgRPC();
    }
}
