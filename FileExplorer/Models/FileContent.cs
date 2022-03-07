using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class FileContent : FileDirectoryContent
    {
        /// <summary>
        /// コピー
        /// </summary>
        /// <param name="destDir"></param>
        public override void Copy(string destDir)
        {
            if (Name == null || FullName == null) return;

            var copyFile = Path.Combine(destDir, Name);
            var info = new FileInfo(FullName);
            var fileName = Path.GetFileNameWithoutExtension(Name);
            if (File.Exists(copyFile))
            {
                copyFile = Path.Combine(destDir, $"{fileName} - コピー{info.Extension}");
                int count = 2;
                while(File.Exists(copyFile))
                {
                    copyFile = Path.Combine(destDir, $"{fileName} - コピー({count++}){info.Extension}");
                }
            }
            File.Copy(FullName, copyFile);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public override void Remove()
        {
            if (FullName == null) return;
            File.Delete(FullName);
        }
    }
}
