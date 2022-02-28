﻿using Prism.Mvvm;
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
            set { 
                SetProperty(ref _modeIndex, value);
                ChangeMode();
            }
        }

        public SettingViewModel()
        {
            ModeIndex = 1;
        }

        private void ChangeMode()
        {
            if (ModeIndex == 0)
            {
                WPFUI.Theme.Manager.Switch(WPFUI.Theme.Style.Light, false);
            }
            else
            {
                WPFUI.Theme.Manager.Switch(WPFUI.Theme.Style.Dark, false);
            }
        }
    }
}
