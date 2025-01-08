using Application.Processor;
using Device.GrpcClient;
using DomainService;
using Infrastructure.Redis;
using Infrastructure.RepositoryCore;
using MelbergFramework.Application;
using MelbergFramework.Infrastructure.Rabbit;
using MelbergFramework.Infrastructure.Redis;

namespace Application;

public class AppRegistrator : Registrator
{
    public override void RegisterServices(IServiceCollection services)
    {
        RabbitModule.RegisterMicroConsumer<
            ClockProcessor,
            ClockMessage>(services, false);
        RabbitModule.RegisterMicroConsumer<
            HeartbeatProcessor,
            HeartbeatMessage>(services, false);
        DeviceGrpcDependencyModule.RegisterClient(services);
        RedisDependencyModule.LoadRedisRepository< IDeviceLastHeardRepository,DeviceLastHeardRepository,DeviceLastHeardContext>(services);
        services
            .AddTransient<ILifeMonitorDomainService,LifeMonitorDomainService>();
    }
}
