using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ObservableCollection<FileDirectoryContent> FileDirectoryCollection
        {
            get { return _fileDirectoryCollection; }
            set { SetProperty(ref _fileDirectoryCollection, value); }
        }

        private List<object> _fileDirectorySelectedCollection
            = new List<object>();

        public List<object> FileDirectorySelectedCollection
        {
            get { return _fileDirectorySelectedCollection; }
            set
            {
                SetProperty(ref _fileDirectorySelectedCollection, value);
                IsSelectedUpdate();
            }
        }

        public HomeViewModel()
        {
            CurrentPath = new DirectoryInfo(@"C:\projects\Example\interprocess_sample").FullName;
        }

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
                    Name = info.Name,
                    Type = "Directry",
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
                    Name = info.Name,
                    Type = "File",
                    UpdateTime = info.LastWriteTime.ToString("yyyy/MM/dd HH:mm")
                };
                FileDirectoryCollection.Add(content);
            }
        }

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
