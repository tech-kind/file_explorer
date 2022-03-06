using FileExplorer.Models;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WPFUI.Common;
using static FileExplorer.Utils.TopicName;

namespace FileExplorer.ViewModels.Menu
{
    public class HomeViewModel : BindableBase
    {
        private readonly DispatcherTimer _timer;
        private readonly IAsyncPublisher<string, string> _genericStringPublisher;
        private readonly IAsyncPublisher<string, List<object>> _selectedObjectPublisher;
        private readonly IAsyncSubscriber<string, string> _currentDirectorySubscriber;

        private ObservableCollection<FileDirectoryContent> _fileDirectoryCollection
            = new();

        /// <summary>
        /// 現在のディレクトリに位置するファイル/ディレクトリの一覧
        /// </summary>
        public ObservableCollection<FileDirectoryContent> FileDirectoryCollection
        {
            get { return _fileDirectoryCollection; }
            set { SetProperty(ref _fileDirectoryCollection, value); }
        }

        private List<object> _fileDirectorySelectedCollection
            = new();

        /// <summary>
        /// 選択されているファイル/ディレクトリの一覧
        /// </summary>
        public List<object> FileDirectorySelectedCollection
        {
            get { return _fileDirectorySelectedCollection; }
            set
            {
                SetProperty(ref _fileDirectorySelectedCollection, value);
                IsSelectedUpdate();
            }
        }

        /// <summary>
        /// 子ディレクトリへの移動コマンド
        /// </summary>
        public DelegateCommand ChangeChildDirectoryCommand { get; private set; }

        /// <summary>
        /// ファイル/ディレクトリ名クリック時コマンド
        /// </summary>
        public DelegateCommand<DataGridBeginningEditEventArgs> FileDirectoryNameClickCommand { get; private set; }

        public HomeViewModel(IServiceProvider serviceProvider)
        {
            _genericStringPublisher = serviceProvider.GetRequiredService<IAsyncPublisher<string, string>>();
            _currentDirectorySubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, string>>();
            _currentDirectorySubscriber.Subscribe(HomeViewCurrentDirectory, SetCurrentDirectoryContents);
            _selectedObjectPublisher = serviceProvider.GetRequiredService<IAsyncPublisher<string, List<object>>>();
            ChangeChildDirectoryCommand = new DelegateCommand(ChangeToChildDirectory);
            FileDirectoryNameClickCommand = new DelegateCommand<DataGridBeginningEditEventArgs>(ConfirmBeginEdit);

            _timer = new();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _timer.Tick += new EventHandler(EnableBeginEdit);
        }

        /// <summary>
        /// 現在のディレクトリ内のファイル/ディレクトリの取得
        /// </summary>
        private ValueTask SetCurrentDirectoryContents(string path, CancellationToken token)
        {
            FileDirectoryCollection.Clear();

            // ディレクトリの取得
            var directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var d in directories)
            {
                DirectoryInfo info = new(d);
                FileDirectoryContent content = new()
                {
                    IsSelected = false,
                    Icon = Icon.Folder20,
                    Name = info.Name,
                    Type = "フォルダー",
                    UpdateTime = info.LastWriteTime.ToString("yyyy/MM/dd HH:mm"),
                    FullName = info.FullName
                };
                FileDirectoryCollection.Add(content);
            }

            // ファイルの取得
            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var f in files)
            {
                FileInfo info = new(f);
                FileDirectoryContent content = new()
                {
                    IsSelected = false,
                    Icon = Icon.Document20,
                    Name = info.Name,
                    Type = "ファイル",
                    UpdateTime = info.LastWriteTime.ToString("yyyy/MM/dd HH:mm"),
                    FullName= info.FullName
                };
                FileDirectoryCollection.Add(content);
            }

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 選択状態の更新
        /// </summary>
        private async void IsSelectedUpdate()
        {
            foreach (var file in FileDirectoryCollection)
            {
                file.IsSelected = false;
            }

            foreach (var selectFile in FileDirectorySelectedCollection)
            {
                if (selectFile is not FileDirectoryContent castSelect)
                {
                    continue;
                }
                var file = FileDirectoryCollection.FirstOrDefault(x => x.Name == castSelect.Name);
                if (file != null)
                {
                    file.IsSelected = true;
                }
            }
            await _selectedObjectPublisher.PublishAsync(HomeViewSelectedChangeObject, _fileDirectorySelectedCollection);
        }

        /// <summary>
        /// 子ディレクトリの移動
        /// </summary>
        private async void ChangeToChildDirectory()
        {
            if (FileDirectorySelectedCollection.Count != 1) return;

            if (FileDirectorySelectedCollection[0] is not FileDirectoryContent select || select.Name == null) return;
            if (select.Type == "ファイル") return;
            await _genericStringPublisher.PublishAsync(HomeViewChangeChildDirectory, select.Name);
        }

        /// <summary>
        /// 編集モードに移行してよいか検証
        /// </summary>
        /// <param name="e"></param>
        private void ConfirmBeginEdit(DataGridBeginningEditEventArgs e)
        {
            if (e.EditingEventArgs is not MouseButtonEventArgs editingEventArgs) return;

            // クリック1回だけの場合は、一旦キャンセルして
            // 一定時間内にダブルクリックされないか確認する
            if (editingEventArgs.ClickCount == 1)
            {
                _timer.Start();
                e.Cancel = true;
            }
            // ダブルクリック時は子ディレクトリへの移動を優先
            // 編集モードへは移行しない
            else if (editingEventArgs.ClickCount > 1)
            {
                _timer.Stop();
                ChangeToChildDirectory();
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 編集モードへ移行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EnableBeginEdit(object? sender, EventArgs e)
        {
            await _genericStringPublisher.PublishAsync(HomeViewBeginEdit, "");

            _timer.Stop();
        }
    }
}
