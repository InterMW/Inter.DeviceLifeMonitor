using Infrastructure.RepositoryCore;
using MelbergFramework.Infrastructure.Redis;
using Microsoft.Extensions.Options;

namespace Infrastructure.Redis;

public class DeviceLastHeardRepository : RedisRepository<DeviceLastHeardContext>, IDeviceLastHeardRepository
{
    public DeviceLastHeardRepository(DeviceLastHeardContext context) : base(context) { }

    public Task<bool> ExistingRecord(string serialNumber, DateTime time) =>
        DB.SetContainsAsync(HistoryKey(time),serialNumber);

    public async Task WriteDownRecord(string serialNumber) 
    {
        var key = HistoryKey(DateTime.UtcNow);
        await DB.SetAddAsync(key,serialNumber);
        await DB.KeyExpireAsync(key,new TimeSpan(0,20,0));
    }

    private static string HistoryKey(DateTime time) => $"device-life-monitor-{time.Year}-{time.Month}-{time.Day}-{time.Hour}-{time.Minute - time.Minute % 5}";
}

public class DeviceLastHeardContext : RedisContext
{
    public DeviceLastHeardContext(
            IOptions<RedisConnectionOptions<DeviceLastHeardContext>> options,
            IConnector connector) : base(options.Value, connector) { }
}
