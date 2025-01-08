using Device.Common;
using Device.Domain;
using Device.GrpcClient;
using Infrastructure.RepositoryCore;
using Microsoft.Extensions.Logging;

namespace DomainService;

public interface ILifeMonitorDomainService {
    Task ListenToDevice(string serialNumber);
    Task VerifyAndUpdate(DateTime now);
}

public class LifeMonitorDomainService : ILifeMonitorDomainService
{
    private readonly IDeviceGrpcClient _deviceClient;
    private readonly IDeviceLastHeardRepository _lastHeardRepository;
    private readonly ILogger<LifeMonitorDomainService> _logger;

    public LifeMonitorDomainService(
            IDeviceGrpcClient deviceClient,
            IDeviceLastHeardRepository lastHeardRepository,
            ILogger<LifeMonitorDomainService> logger
            )
    {
        _deviceClient = deviceClient;
        _lastHeardRepository = lastHeardRepository;
        _logger = logger;
    }

    public Task ListenToDevice(string serialNumber) => 
        _lastHeardRepository.WriteDownRecord(serialNumber);

    public async Task VerifyAndUpdate(DateTime now)
    {
        await Task.WhenAll(
            _deviceClient
            .GetDevicesAsync(CancellationToken.None)
            .Select(_ => UpdateDevice(_,now))
            .ToEnumerable());
    }

    private async Task UpdateDevice(DeviceModel device, DateTime now)
    {
        var wasSeen = await _lastHeardRepository.ExistingRecord(device.SerialNumber, now);
        if(device.IsOnline == wasSeen)
        {
            _logger.LogInformation(
                "Didn't update {_serialNumber} because it was already {_alive}."
                , device.SerialNumber, wasSeen);
            return; 
        }

        _logger.LogInformation(
            "Updated {_serialNumber} to be {_alive}."
            , device.SerialNumber, wasSeen);

        await _deviceClient.SetDeviceLifeState(device.SerialNumber, wasSeen);
    }

    public async Task MarkAsCurrentlyAwake(string serialNumber)
    {
        DeviceModel device;
        try
        {
            device = await _deviceClient.GetDeviceAsync(serialNumber);

            if(device == null)
            {
                await _deviceClient.CreateDeviceAsync(serialNumber);
                device = await _deviceClient.GetDeviceAsync(serialNumber);
            }

            await _deviceClient.SetDeviceLifeState(serialNumber, true);
        }
        catch (DeviceSerialNumberInvalidException)
        {
            _logger.LogError("Device {_serialNumber} has invalid serialNumber", serialNumber);
            return;
        }
        catch (DeviceCannotBeCreatedException)
        {
            _logger.LogError("Device {_serialNumber} could not be created", serialNumber);
        }
    }

    public async Task UpdateStates()
    {
        await foreach(var device in _deviceClient.GetDevicesAsync(CancellationToken.None))
        {
            var deviceIsOnline = device.IsOnline;

            if(!deviceIsOnline)
            {
                return;
            }

            var heardFromRecently = false;//await _lastHeardRepository.ExistingRecord(device.SerialNumber);

            if(!deviceIsOnline && !heardFromRecently)
            {
                try
                {
                    await _deviceClient.SetDeviceLifeState(device.SerialNumber, false);
                }
                catch (DeviceSerialNumberInvalidException)
                {
                    _logger.LogError("Something impossible happened, an invalid serial number {_serial} has been entered.", device.SerialNumber);
                }
            }
        }
    }
}
