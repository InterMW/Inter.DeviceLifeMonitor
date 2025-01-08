namespace Infrastructure.RepositoryCore;

public interface IDeviceLastHeardRepository
{
    Task<bool> ExistingRecord(string serialNumber, DateTime time);
    Task WriteDownRecord(string serialNumber);
}
