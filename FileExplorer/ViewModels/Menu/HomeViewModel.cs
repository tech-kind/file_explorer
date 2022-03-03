using FileExplorer.Events;
using FileExplorer.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WPFUI.Common;

namespace FileExplorer.ViewModels.Menu
{
    public class HomeViewModel : BindableBase
    {
        private IEventAggregator _ea;
        private DispatcherTimer _timer;

        private DirectoryInfo? _currentPath;

        private Stack<DirectoryInfo> _directoryUndo = new Stack<DirectoryInfo>();
        private Stack<DirectoryInfo> _directoryRedo = new Stack<DirectoryInfo>();

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
                SetCurrentDirectoryContents();
            }
        }

        private ObservableCollection<FileDirectoryContent> _fileDirectoryCollection
            = new ObservableCollection<FileDirectoryContent>();

        /// <summary>
        /// 現在のディレクトリに位置するファイル/ディレクトリの一覧
        /// </summary>
        public ObservableCollection<FileDirectoryContent> FileDirectoryCollection
        {
            get { return _fileDirectoryCollection; }
            set { SetProperty(ref _fileDirectoryCollection, value); }
        }

        private List<object> _fileDirectorySelectedCollection
            = new List<object>();

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
        /// 親ディレクトリへの移動コマンド
        /// </summary>
        public DelegateCommand ChangeParentDirectoryCommand { get; private set; }

        /// <summary>
        /// ファイル/ディレクトリ名クリック時コマンド
        /// </summary>
        public DelegateCommand<DataGridBeginningEditEventArgs> FileDirectoryNameClickCommand { get; private set; }

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

        public HomeViewModel(IEventAggregator ea)
        {
            _ea = ea;
            ChangeChildDirectoryCommand = new DelegateCommand(ChangeToChildDirectory);
            ChangeParentDirectoryCommand = new DelegateCommand(ChangeToParentDirectory);
            FileDirectoryNameClickCommand = new DelegateCommand<DataGridBeginningEditEventArgs>(ConfirmBeginEdit);
            UndoCommand = new DelegateCommand(Undo).ObservesCanExecute(() => CanUndo);
            RedoCommand = new DelegateCommand(Redo).ObservesCanExecute(() => CanRedo);

            CurrentPath = new DirectoryInfo(@"C:\projects\Example\interprocess_sample").FullName;

            _timer = new();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _timer.Tick += new EventHandler(EnableBeginEdit);
        }

        /// <summary>
        /// 現在のディレクトリ内のファイル/ディレクトリの取得
        /// </summary>
        private void SetCurrentDirectoryContents()
        {
            if (CurrentPath == null) return;

            FileDirectoryCollection.Clear();

            // ディレクトリの取得
            var directories = Directory.GetDirectories(CurrentPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var d in directories)
            {
                DirectoryInfo info = new DirectoryInfo(d);
                FileDirectoryContent content = new()
                {
                    IsSelected = false,
                    Icon = Icon.Folder20,
                    Name = info.Name,
                    Type = "フォルダー",
                    UpdateTime = info.LastWriteTime.ToString("yyyy/MM/dd HH:mm")
                };
                FileDirectoryCollection.Add(content);
            }

            // ファイルの取得
            var files = Directory.GetFiles(CurrentPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var f in files)
            {
                FileInfo info = new FileInfo(f);
                FileDirectoryContent content = new()
                {
                    IsSelected = false,
                    Icon = Icon.Document20,
                    Name = info.Name,
                    Type = "ファイル",
                    UpdateTime = info.LastWriteTime.ToString("yyyy/MM/dd HH:mm")
                };
                FileDirectoryCollection.Add(content);
            }
        }

        /// <summary>
        /// 選択状態の更新
        /// </summary>
        private void IsSelectedUpdate()
        {
            foreach (var file in FileDirectoryCollection)
            {
                file.IsSelected = false;
            }

            foreach (var selectFile in FileDirectorySelectedCollection)
            {
                var castSelect = selectFile as FileDirectoryContent;
                if (castSelect == null)
                {
                    continue;
                }
                var file = FileDirectoryCollection.FirstOrDefault(x => x.Name == castSelect.Name);
                if (file != null)
                {
                    file.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// 子ディレクトリの移動
        /// </summary>
        private void ChangeToChildDirectory()
        {
            if (CurrentPath == null) return;
            if (FileDirectorySelectedCollection.Count != 1) return;

            var select = FileDirectorySelectedCollection[0] as FileDirectoryContent;
            if (select == null || select.Name == null) return;
            if (select.Type == "ファイル") return;
            var path = Path.Combine(CurrentPath, select.Name);
            CurrentPath = path;
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
        /// 編集モードに移行してよいか検証
        /// </summary>
        /// <param name="e"></param>
        private void ConfirmBeginEdit(DataGridBeginningEditEventArgs e)
        {
            var editingEventArgs = e.EditingEventArgs as MouseButtonEventArgs;

            if (editingEventArgs == null) return;

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
        private void EnableBeginEdit(object? sender, EventArgs e)
        {
            _ea.GetEvent<BeginEditEvent>().Publish();

            _timer.Stop();
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
    }
}
