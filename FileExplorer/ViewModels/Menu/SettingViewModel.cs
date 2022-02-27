using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.ViewModels.Menu
{
    public class SettingViewModel : BindableBase
    {
        private int _modeIndex;

        public int ModeIndex
        {
            get { return _modeIndex; }
            set { SetProperty(ref _modeIndex, value); }
        }

        public SettingViewModel()
        {
            _modeIndex = 0;
        }
    }
}
