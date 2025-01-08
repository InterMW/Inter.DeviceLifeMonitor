using Device.Common;
using Device.GrpcClient;
using MelbergFramework.Application;

namespace Application;
internal class Program
{
    private static async Task Main(string[] args)
    {
        await MelbergHost
            .CreateHost<AppRegistrator>()
            .DevelopmentPasswordReplacement("Rabbit:ClientDeclarations:Connections:0:Password", "rabbit_pass")
            .Build()
            .RunAsync();
    }
}
