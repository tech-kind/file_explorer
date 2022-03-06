using FileExplorer.Models;
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
    public class FileDirectoryOperatorViewModel : BindableBase
    {
        private List<object>? _selectedObject;
        private readonly IAsyncPublisher<string, string> _genericStringPublisher;
        private readonly IAsyncSubscriber<string, List<object>> _selectObjectSubscriber;

        private bool _isSelected = false;

        /// <summary>
        /// Cutコマンド
        /// </summary>
        public DelegateCommand CutCommand { get; private set; }

        /// <summary>
        /// Copyコマンド
        /// </summary>
        public DelegateCommand CopyCommand { get; private set; }

        /// <summary>
        /// Pasteコマンド
        /// </summary>
        public DelegateCommand PasteCommand { get; private set; }

        /// <summary>
        /// Renameコマンド
        /// </summary>
        public DelegateCommand RenameCommand { get; private set; }

        /// <summary>
        /// Removeコマンド
        /// </summary>
        public DelegateCommand RemoveCommand { get; private set; }

        /// <summary>
        /// ファイル/ディレクトリが選択されているかどうか
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public FileDirectoryOperatorViewModel(IServiceProvider serviceProvider)
        {
            _selectObjectSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, List<object>>>();
            _selectObjectSubscriber.Subscribe(HomeViewSelectedChangeObject, GetSelectedObject);
            _genericStringPublisher = serviceProvider.GetRequiredService<IAsyncPublisher<string, string>>();
            CutCommand = new DelegateCommand(Cut).ObservesCanExecute(() => IsSelected);
            CopyCommand = new DelegateCommand(Copy).ObservesCanExecute(() => IsSelected);
            PasteCommand = new DelegateCommand(Paste);
            RenameCommand = new DelegateCommand(Rename).ObservesCanExecute(() => IsSelected);
            RemoveCommand = new DelegateCommand(Remove).ObservesCanExecute(() => IsSelected);
        }

        /// <summary>
        /// 選択されているファイル/ディレクトリの取得
        /// </summary>
        /// <param name="selectObject"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private ValueTask GetSelectedObject(List<object> selectObject, CancellationToken token)
        {
            _selectedObject = selectObject;
            IsSelected = _selectedObject.Count > 0;
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// ファイル/ディレクトリのカット
        /// </summary>
        private void Cut()
        {

        }

        /// <summary>
        /// ファイル/ディレクトリのコピー
        /// </summary>
        private void Copy()
        {

        }

        /// <summary>
        /// ファイル/ディレクトリのペースト
        /// </summary>
        private void Paste()
        {

        }

        /// <summary>
        /// 名称変更
        /// </summary>
        private async void Rename()
        {
            // 編集モードへ移行
            await _genericStringPublisher.PublishAsync(HomeViewBeginEdit, "");
        }

        /// <summary>
        /// ファイル/ディレクトリの削除
        /// </summary>
        private async void Remove()
        {
            if (_selectedObject == null || _selectedObject.Count == 0) return;
            foreach (var item in _selectedObject)
            {
                if (item is not FileDirectoryContent content) continue;

                if (content.FullName == null) continue;

                if (content.Type == "フォルダー")
                {
                    Directory.Delete(content.FullName, true);
                }
                else
                {
                    File.Delete(content.FullName);
                }
            }
            await _genericStringPublisher.PublishAsync(HomeViewRefreshCurrentDirectory, "");
        }
    }
}
