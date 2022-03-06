using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FileExplorer.Utils.TopicName;

namespace FileExplorer.ViewModels.Region
{
    public class CurrentDirectoryViewModel : BindableBase
    {
        private readonly IAsyncPublisher<string, string> _currentDirectoryPublisher;
        private readonly IAsyncSubscriber<string, string> _genericStringSubscriber;
        private readonly Stack<DirectoryInfo> _directoryUndo = new();
        private readonly Stack<DirectoryInfo> _directoryRedo = new();

        private DirectoryInfo? _currentPath;

        /// <summary>
        /// 現在のディレクトリ
        /// </summary>
        public string? CurrentPath
        {
            get
            {
                if (_currentPath == null) return string.Empty;
                return _currentPath.FullName;
            }
            set
            {
                if (value == null) return;
                var path = new DirectoryInfo(value);
                if (_currentPath != null)
                {
                    // Redoの先頭と今のディレクトリが同じ場合は、
                    // Undoに加えない
                    if (_directoryRedo.Count == 0
                        || _directoryRedo.Peek().FullName != _currentPath.FullName)
                    {
                        _directoryUndo.Push(_currentPath);
                        CanUndo = true;
                    }
                }
                SetProperty(ref _currentPath, path);
                IsExistParent = path.Parent != null;
                CurrentDirectoryPublish();
            }
        }

        /// <summary>
        /// 更新コマンド
        /// </summary>
        public DelegateCommand RefreshCommand { get; private set; }

        /// <summary>
        /// 親ディレクトリへの移動コマンド
        /// </summary>
        public DelegateCommand ChangeParentDirectoryCommand { get; private set; }

        /// <summary>
        /// Undoコマンド
        /// </summary>
        public DelegateCommand UndoCommand { get; private set; }

        private bool _canUndo = false;

        /// <summary>
        /// Undo可能かどうか
        /// </summary>
        public bool CanUndo
        {
            get { return _canUndo; }
            set { SetProperty(ref _canUndo, value); }
        }

        /// <summary>
        /// Redoコマンド
        /// </summary>
        public DelegateCommand RedoCommand { get; private set; }

        private bool _canRedo = false;

        /// <summary>
        /// Redo可能かどうか
        /// </summary>
        public bool CanRedo
        {
            get { return _canRedo; }
            set { SetProperty(ref _canRedo, value); }
        }

        private bool _isExistParent;

        /// <summary>
        /// 親ディレクトリが存在するかどうか
        /// </summary>
        public bool IsExistParent
        {
            get { return _isExistParent; }
            set { SetProperty(ref _isExistParent, value); }
        }

        public CurrentDirectoryViewModel(IServiceProvider serviceProvider)
        {
            _currentDirectoryPublisher = serviceProvider.GetRequiredService<IAsyncPublisher<string, string>>();
            _genericStringSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, string>>();
            _genericStringSubscriber.Subscribe(HomeViewChangeChildDirectory, ChangeToChiledDirectory);
            _genericStringSubscriber.Subscribe(HomeViewRefreshCurrentDirectory, RefreshCurrentDirectory);
            UndoCommand = new DelegateCommand(Undo).ObservesCanExecute(() => CanUndo);
            RedoCommand = new DelegateCommand(Redo).ObservesCanExecute(() => CanRedo);
            ChangeParentDirectoryCommand = new DelegateCommand(ChangeToParentDirectory);
            RefreshCommand = new DelegateCommand(CurrentDirectoryPublish);

            CurrentPath = new DirectoryInfo(@"C:\projects\Example").FullName;
        }

        /// <summary>
        /// 親ディレクトリへの移動
        /// </summary>
        private void ChangeToParentDirectory()
        {
            if (CurrentPath == null) return;
            var info = new DirectoryInfo(CurrentPath);

            if (info.Parent == null) return;
            var path = info.Parent.FullName;
            CurrentPath = path;
        }

        /// <summary>
        /// Undo
        /// </summary>
        private void Undo()
        {
            var path = _directoryUndo.Pop();
            if (_currentPath != null)
            {
                _directoryRedo.Push(_currentPath);
            }

            CanUndo = (_directoryUndo.Count > 0);
            CanRedo = true;

            if (path == null) return;
            CurrentPath = path.FullName;
        }

        /// <summary>
        /// Redo
        /// </summary>
        private void Redo()
        {
            var path = _directoryRedo.Pop();

            CanRedo = (_directoryRedo.Count > 0);

            if (path == null) return;
            CurrentPath = path.FullName;
        }

        /// <summary>
        /// 現在のディレクトリの内容を更新
        /// </summary>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ValueTask RefreshCurrentDirectory(string message, CancellationToken token)
        {
            CurrentDirectoryPublish();
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 現在のディレクトリをHomeViewに通知
        /// </summary>
        private async void CurrentDirectoryPublish()
        {
            if (_currentPath == null) return;
            await _currentDirectoryPublisher.PublishAsync(HomeViewCurrentDirectory, _currentPath.FullName);
        }

        /// <summary>
        /// 子ディレクトへの移動を受信
        /// </summary>
        /// <param name="childName"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ValueTask ChangeToChiledDirectory(string childName, CancellationToken token)
        {
            if (_currentPath == null) return ValueTask.FromCanceled(token);

            var path = Path.Combine(_currentPath.FullName, childName);
            CurrentPath = path;
            return ValueTask.CompletedTask;
        }
    }
}
