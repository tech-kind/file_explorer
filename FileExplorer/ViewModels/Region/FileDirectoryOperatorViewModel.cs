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
        private readonly IAsyncPublisher<string, string> _genericStringPublisher;
        private readonly IAsyncSubscriber<string, bool> _selectObjectSubscriber;

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
            _selectObjectSubscriber = serviceProvider.GetRequiredService<IAsyncSubscriber<string, bool>>();
            _selectObjectSubscriber.Subscribe(HomeViewIsSelectedObject, GetSelectedObject);
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
        private ValueTask GetSelectedObject(bool isSelected, CancellationToken token)
        {
            IsSelected = isSelected;
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// ファイル/ディレクトリのカット
        /// </summary>
        private async void Cut()
        {
            await _genericStringPublisher.PublishAsync(HomeViewCutFileDirectory, "");
        }

        /// <summary>
        /// ファイル/ディレクトリのコピー
        /// </summary>
        private async void Copy()
        {
            await _genericStringPublisher.PublishAsync(HomeViewCopyFileDirectory, "");
        }

        /// <summary>
        /// ファイル/ディレクトリのペースト
        /// </summary>
        private async void Paste()
        {
            await _genericStringPublisher.PublishAsync(HomeViewPasteFileDirectory, "");
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
            await _genericStringPublisher.PublishAsync(HomeViewRemoveFileDirectory, "");
        }
    }
}
