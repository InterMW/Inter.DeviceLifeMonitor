using DomainService;
using MelbergFramework.Infrastructure.Rabbit.Consumers;
using MelbergFramework.Infrastructure.Rabbit.Messages;
using MelbergFramework.Infrastructure.Rabbit.Translator;
using Newtonsoft.Json;

namespace Application.Processor;

public class HeartbeatProcessor(
        ILifeMonitorDomainService service,
        IJsonToObjectTranslator<HeartbeatMessage> translator) : IStandardConsumer
{
    public async Task ConsumeMessageAsync(Message message, CancellationToken ct)
    {
        var serialNumber = translator.Translate(message).SerialNumber;
        Console.WriteLine($"Found something {serialNumber}");
        await service.ListenToDevice(serialNumber);
    }
}

public class HeartbeatMessage : StandardMessage
{
    [JsonProperty("SerialNumber")]
    public string SerialNumber {get; set;} = "";
    public override string GetRoutingKey() => "";
}
