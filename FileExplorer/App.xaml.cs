using Prism.Unity;
using Prism.Ioc;
using System.Windows;
using FileExplorer.Views;
using FileExplorer.Views.Menu;

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
            // ナビゲーションによって表示されるViewをコンテナに登録
            containerRegistry.RegisterForNavigation<HomeView>();
            containerRegistry.RegisterForNavigation<RecentView>();
            containerRegistry.RegisterForNavigation<FavoriteView>();
            containerRegistry.RegisterForNavigation<SettingView>();
        }

    }
}
