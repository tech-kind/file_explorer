using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer.Models
{
    public class DirectoryContent : FileDirectoryContent
    {
        /// <summary>
        ///  コピー
        /// </summary>
        /// <param name="destDir"></param>
        public override void Copy(string destDir)
        {
            if (Name == null || FullName == null) return;

            var copyDir = Path.Combine(destDir, Name);
            if (Directory.Exists(copyDir))
            {
                copyDir = Path.Combine(destDir, $"{Name} - コピー");
                int count = 2;
                while (Directory.Exists(copyDir))
                {
                    copyDir = Path.Combine(destDir, $"{Name} - コピー({count++})");
                }
            }
            DirectoryCopy(FullName, copyDir, true);
        }

        /// <summary>
        /// 削除
        /// </summary>
        public override void Remove()
        {
            if (FullName == null) return;
            Directory.Delete(FullName, true);
        }

        /// <summary>
        /// ディレクトリのコピー
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
    }
}
