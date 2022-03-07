using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using static FileExplorer.Utils.TopicName;

namespace FileExplorer.Views.Menu
{
    /// <summary>
    /// HomeView.xaml の相互作用ロジック
    /// </summary>
    public partial class HomeView : UserControl
    {
        private readonly IAsyncSubscriber<string, string> _beginEditSubscriber;
        private readonly IAsyncSubscriber<string, bool> _focusableSubscriber;

        public HomeView(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _beginEditSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, string>>();
            _beginEditSubscriber.Subscribe(HomeViewBeginEdit, BeginEdit);
            _focusableSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, bool>>();
            _focusableSubscriber.Subscribe(HomeViewDataGridFocusable, DataGridFocusable);
        }

        private ValueTask BeginEdit(string message, CancellationToken token)
        {
            FileDirectoryDataGrid.BeginEdit();
            return ValueTask.CompletedTask;
        }

        private ValueTask DataGridFocusable(bool focus, CancellationToken token)
        {
            if (focus)
            {
                FileDirectoryDataGrid.Focus();
                FileDirectoryDataGrid.SelectedIndex = 0;
            }
            return ValueTask.CompletedTask;
        }
    }
}
