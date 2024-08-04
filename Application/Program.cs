using Device.Common;
using Device.GrpcClient;
internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        DeviceGrpcDependencyModule.RegisterClient(builder.Services);
        var app = builder.Build();

        try
        {

            
            var j = app.Services.GetService<IDeviceGrpcClient>();
            await j.CreateDeviceAsync("abcdefabcdef");
            await foreach( var dev in j.GetDevicesAsync(CancellationToken.None))
            {
                Console.WriteLine(dev.SerialNumber);
            }
            await j.CreateDeviceAsync("a");
        }
        catch (DeviceCannotBeCreatedException ex)
        {
            
            throw ex;
        }
        //app.MapGet("/", () => "Hello World!");

        //app.Run();
    }
}
