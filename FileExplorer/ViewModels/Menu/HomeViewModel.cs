using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPFUI.Common;

namespace FileExplorer.ViewModels.Menu
{
    public class HomeViewModel : BindableBase
    {
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

        public DelegateCommand<DataGridBeginningEditEventArgs> BeginningEditCommand { get; private set; }

        public HomeViewModel()
        {
            ChangeChildDirectoryCommand = new DelegateCommand(ChangeToChildDirectory);
            ChangeParentDirectoryCommand = new DelegateCommand(ChangeToParentDirectory);
            BeginningEditCommand = new DelegateCommand<DataGridBeginningEditEventArgs>(BeginningEdit);

            CurrentPath = new DirectoryInfo(@"C:\projects\Example\interprocess_sample").FullName;

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

        private void BeginningEdit(DataGridBeginningEditEventArgs e)
        {
            var editingEventArgs = e.EditingEventArgs as MouseButtonEventArgs;
            
            if (editingEventArgs == null) return;

            if (editingEventArgs.ClickCount == 1)
            {
                e.Cancel = false;
            }
            if (editingEventArgs.ClickCount > 1)
            {
                e.Cancel = true;
                ChangeToChildDirectory();
            }
        }
    }

    public class FileDirectoryContent : BindableBase
    {
        private bool _isSelected;
        /// <summary>
        /// 選択状態
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private Icon _icon;
        /// <summary>
        /// アイコン
        /// </summary>
        public Icon Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        private string? _name;
        /// <summary>
        /// ファイルorディレクトリ名
        /// </summary>
        public string? Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string? _type;
        /// <summary>
        /// 種別
        /// </summary>
        public string? Type
        {
            get { return _type; }
            set { SetProperty(ref _type, value); }
        }

        private string? _updateTime;
        /// <summary>
        /// 更新日時
        /// </summary>
        public string? UpdateTime
        {
            get { return _updateTime; }
            set { SetProperty(ref _updateTime, value); }
        }
    }
}
