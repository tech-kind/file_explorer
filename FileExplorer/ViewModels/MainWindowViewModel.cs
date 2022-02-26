using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        private bool _homeChecked;
        /// <summary>
        /// ホーム画面表示
        /// </summary>
        public bool HomeChecked
        {
            get { return _homeChecked; }
            set
            {
                if (value) Navigate("HomeView");
                SetProperty(ref _homeChecked, value);
            }
        }

        private bool _recentChecked;

        /// <summary>
        /// 最近画面表示
        /// </summary>
        public bool RecentChecked
        {
            get { return _recentChecked; }
            set
            {
                if (value) Navigate("RecentView");
                SetProperty(ref _recentChecked, value);
            }
        }

        private bool _favoriteChecked;
        /// <summary>
        /// お気に入り画面表示
        /// </summary>
        public bool FavoriteChecked
        {
            get { return _favoriteChecked; }
            set
            {
                if (value) Navigate("FavoriteView");
                SetProperty(ref _favoriteChecked, value);
            }
        }

        private bool _settingChecked;
        /// <summary>
        /// 設定画面表示
        /// </summary>
        public bool SettingChecked
        {
            get { return _settingChecked; }
            set
            {
                if (value) Navigate("SettingView");
                SetProperty(ref _settingChecked, value);
            }
        }

        public DelegateCommand LoadedCommand { get; private set; }

        public MainWindowViewModel(IRegionManager regionManger)
        {
            _regionManager = regionManger;

            LoadedCommand = new DelegateCommand(() =>
            {
                // メイン画面ロード後に、ホーム画面を表示するようにする
                HomeChecked = true;
            });
        }

        /// <summary>
        /// ナビゲーション
        /// </summary>
        /// <param name="navigatePath"></param>
        private void Navigate(string navigatePath)
        {
            // navigatePathで指定されたViewをContentRegionに表示する
            _regionManager.RequestNavigate("ContentRegion", navigatePath);
        }
    }
}
