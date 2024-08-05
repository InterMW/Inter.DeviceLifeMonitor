namespace Infrastructure.RepositoryCore;

public interface IDeviceLastHeardRepository
{
    Task<bool> ExistingRecord(string serialNumber);
}
