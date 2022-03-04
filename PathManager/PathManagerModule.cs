using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prism.Ioc;
using Prism.Modularity;

namespace PathManager
{
    public class PathManagerModule : IModule
    {
        public async void OnInitialized(IContainerProvider containerProvider)
        {
            IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddMessagePipe();
                services.AddHostedService<PathManager>();
            })
            .Build();

            await host.RunAsync();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}