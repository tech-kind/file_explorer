using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WPFUI.Common;

namespace FileExplorer.Models
{
    public class FileDirectoryContent : INotifyPropertyChanged
    {
        private bool _isSelected;
        /// <summary>
        /// 選択状態
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        private Icon _icon;
        /// <summary>
        /// アイコン
        /// </summary>
        public Icon Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        private string? _name;
        /// <summary>
        /// ファイルorディレクトリ名
        /// </summary>
        public string? Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string? _type;
        /// <summary>
        /// 種別
        /// </summary>
        public string? Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        private string? _updateTime;
        /// <summary>
        /// 更新日時
        /// </summary>
        public string? UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
