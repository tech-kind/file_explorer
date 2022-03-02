﻿using FileExplorer.Events;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileExplorer.Views.Menu
{
    /// <summary>
    /// HomeView.xaml の相互作用ロジック
    /// </summary>
    public partial class HomeView : UserControl
    {
        private IEventAggregator _ea;

        public HomeView(IEventAggregator ea)
        {
            InitializeComponent();

            _ea = ea;
            _ea.GetEvent<BeginEditEvent>().Subscribe(BeginEdit);
        }

        private void BeginEdit()
        {
            FileDirectoryDataGrid.BeginEdit();
        }
    }
}
