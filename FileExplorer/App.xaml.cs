using Prism.Unity;
using Prism.Ioc;
using System.Windows;
using FileExplorer.Views;
using FileExplorer.Views.Menu;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace FileExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {

        protected override Window CreateShell()
        {
            WPFUI.Theme.Watcher.Start();
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            ServiceCollection services = new ServiceCollection();

            // MessagePipe の標準サービスを登録する
            services.AddMessagePipe();

            services.AddTransient(typeof(IAsyncPublisher<,>), typeof(AsyncMessageBroker<,>));
            services.AddTransient(typeof(IAsyncSubscriber<,>), typeof(AsyncMessageBroker<,>));

            ServiceProvider provider = services.BuildServiceProvider();

            // path_manager
            {
                // var pathPublisher = provider.GetRequiredService<IRequestHandler<string, string>>();
                // var pathSubscriber = provider.GetRequiredService<IRequestHandler<string, string>>();
                // var path_manager = new PathManager.PathManager(pathPublisher, pathSubscriber);
                // path_manager.StartAsync(new System.Threading.CancellationToken());
            }

            containerRegistry.Register<IServiceProvider>(() => provider);

            // ナビゲーションによって表示されるViewをコンテナに登録
            containerRegistry.RegisterForNavigation<HomeView>();
            containerRegistry.RegisterForNavigation<RecentView>();
            containerRegistry.RegisterForNavigation<FavoriteView>();
            containerRegistry.RegisterForNavigation<SettingView>();
        }
    }
}
