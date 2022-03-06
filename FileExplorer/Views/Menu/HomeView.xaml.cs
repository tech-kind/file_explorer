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

namespace FileExplorer.Views.Menu
{
    /// <summary>
    /// HomeView.xaml の相互作用ロジック
    /// </summary>
    public partial class HomeView : UserControl
    {
        private readonly IAsyncSubscriber<string, string> _beginEditSubscriber;

        public HomeView(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _beginEditSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, string>>();
            _beginEditSubscriber.Subscribe("/home_view/begin_edit", BeginEdit);
        }

        private ValueTask BeginEdit(string message, CancellationToken token)
        {
            FileDirectoryDataGrid.BeginEdit();
            return ValueTask.CompletedTask;
        }
    }
}
