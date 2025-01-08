using DomainService;
using MelbergFramework.Infrastructure.Rabbit.Consumers;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Rabbit.Translator;

namespace Application.Processor;

public class ClockProcessor(
        ILifeMonitorDomainService service,
        IJsonToObjectTranslator<ClockMessage> translator,
        ILogger<ClockProcessor> logger
        ) : IStandardConsumer
{
    public async Task ConsumeMessageAsync(Message message, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddMinutes(-5);
        if(now.Minute % 5 == 0)
        {
            logger.LogInformation("Taking rolecall");
            await service.VerifyAndUpdate(now);
        }
    }
}

public class ClockMessage : StandardMessage
{
    public override string GetRoutingKey() => throw new NotImplementedException();
}
