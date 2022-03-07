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
        private string? _currentDirectory;
        private readonly DispatcherTimer _timer;
        private readonly IAsyncPublisher<string, string> _genericStringPublisher;
        private readonly IAsyncPublisher<string, bool> _genericBoolPublisher;
        private readonly IAsyncSubscriber<string, string> _genericStringSubscriber;

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

        private List<FileDirectoryContent> _fileDirectorySelectedCollection
            = new();

        private List<FileDirectoryContent> _fileDirectoryCopyCollection
            = new();

        /// <summary>
        /// 選択されているファイル/ディレクトリの一覧
        /// </summary>
        public List<object> FileDirectorySelectedCollection
        {
            get { return _fileDirectorySelectedCollection.ConvertAll(x => x as object); }
            set
            {
                SetProperty(ref _fileDirectorySelectedCollection, value.ConvertAll(x => (FileDirectoryContent)x));
                IsSelectedUpdate();
            }
        }

        /// <summary>
        /// 子ディレクトリへの移動コマンド
        /// </summary>
        public DelegateCommand ChangeChildDirectoryCommand { get; private set; }

        public DelegateCommand RemoveCommand { get; private set; }

        public DelegateCommand CopyCommand { get; private set; }

        public DelegateCommand PasteCommand { get; private set; }

        /// <summary>
        /// ファイル/ディレクトリ名クリック時コマンド
        /// </summary>
        public DelegateCommand<DataGridBeginningEditEventArgs> FileDirectoryNameClickCommand { get; private set; }

        public HomeViewModel(IServiceProvider serviceProvider)
        {
            _genericStringPublisher = serviceProvider.GetRequiredService<IAsyncPublisher<string, string>>();
            _genericStringSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, string>>();
            _genericStringSubscriber.Subscribe(HomeViewCurrentDirectory, SetCurrentDirectoryContents);
            _genericStringSubscriber.Subscribe(HomeViewCopyFileDirectory, CopyFileDirectory);
            _genericStringSubscriber.Subscribe(HomeViewPasteFileDirectory, PasteFileDirectory);
            _genericStringSubscriber.Subscribe(HomeViewRemoveFileDirectory, RemoveFileDirectory);
            _genericBoolPublisher = serviceProvider.GetRequiredService<IAsyncPublisher<string, bool>>();
            ChangeChildDirectoryCommand = new DelegateCommand(ChangeToChildDirectory);
            RemoveCommand = new DelegateCommand(RemoveFromCommand);
            CopyCommand = new DelegateCommand(CopyFromCommand);
            PasteCommand = new DelegateCommand(PasteFromCommand);
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
            _currentDirectory = path;
            FileDirectoryCollection.Clear();

            // ディレクトリの取得
            var directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var d in directories)
            {
                DirectoryInfo info = new(d);
                FileDirectoryContent content = new DirectoryContent()
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
                FileDirectoryContent content = new FileContent()
                {
                    IsSelected = false,
                    Icon = Icon.Document20,
                    Name = info.Name,
                    Type = "ファイル",
                    UpdateTime = info.LastWriteTime.ToString("yyyy/MM/dd HH:mm"),
                    FullName = info.FullName
                };
                FileDirectoryCollection.Add(content);
            }
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// コマンドからのコピー
        /// </summary>
        private async void CopyFromCommand()
        {
            await CopyFileDirectory("", new CancellationToken());
        }

        /// <summary>
        /// 選択中のファイル/ディレクトリのコピー
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ValueTask CopyFileDirectory(string message, CancellationToken token)
        {
            _fileDirectoryCopyCollection = new List<FileDirectoryContent>(_fileDirectorySelectedCollection);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// コマンドからのペースト
        /// </summary>
        private async void PasteFromCommand()
        {
            await PasteFileDirectory("", new CancellationToken());
        }

        /// <summary>
        /// 選択中のファイル/ディレクトリのペースト
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ValueTask PasteFileDirectory(string message, CancellationToken token)
        {
            if (_currentDirectory == null) return ValueTask.FromCanceled(token);

            foreach (var copyFile in _fileDirectoryCopyCollection)
            {
                copyFile.Copy(_currentDirectory);
            }

            SetCurrentDirectoryContents(_currentDirectory, token);

            return _genericBoolPublisher.PublishAsync(HomeViewDataGridFocusable, true);
        }

        /// <summary>
        /// コマンドからの削除
        /// </summary>
        private async void RemoveFromCommand()
        {
            await RemoveFileDirectory("", new CancellationToken());
        }

        /// <summary>
        /// 選択中のファイル/ディレクトリの削除
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ValueTask RemoveFileDirectory(string message, CancellationToken token)
        {
            if (_currentDirectory == null) return ValueTask.FromCanceled(token);

            foreach (var selectFile in _fileDirectorySelectedCollection)
            {
                selectFile.Remove();
            }
            return SetCurrentDirectoryContents(_currentDirectory, token);
        }

        /// <summary>
        /// 選択状態の更新
        /// </summary>
        private async void IsSelectedUpdate()
        {
            foreach (var file in _fileDirectoryCollection)
            {
                file.IsSelected = false;
            }

            foreach (var selectFile in _fileDirectorySelectedCollection)
            {
                var file = FileDirectoryCollection.FirstOrDefault(x => x.Name == selectFile.Name);
                if (file != null)
                {
                    file.IsSelected = true;
                }
            }
            await _genericBoolPublisher.PublishAsync(HomeViewIsSelectedObject, _fileDirectorySelectedCollection.Count > 0);
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
