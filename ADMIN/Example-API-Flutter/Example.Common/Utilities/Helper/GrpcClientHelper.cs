using Example.Common.Const;
using Grpc.Core;
using Grpc.Net.Client;

namespace Example.Common.Utilities.Helper
{
    public sealed class GrpcClientHelper
    {
        private static readonly Lazy<GrpcClientHelper> lazy = new Lazy<GrpcClientHelper>(() => new GrpcClientHelper());
        public static GrpcClientHelper Instance { get { return lazy.Value; } }

        private GrpcClientHelper()
        {
        }

        public T CreateClient<T>()
        {
            var channel = GrpcChannel.ForAddress(StaticVariable.GrpcCenter);
            return (T)Activator.CreateInstance(typeof(T), new object[] { channel });
        }

        private GrpcChannel CreateAuthenticatedChannel()
        {
            var credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
            {
                //metadata.Add("Authorization", $"Bearer {token}");
            });

            var channel = GrpcChannel.ForAddress(StaticVariable.GrpcCenter, new GrpcChannelOptions
            {
                Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
            });
            return channel;
        }
    }
}
