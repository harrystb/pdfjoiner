using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace pdfjoiner.DesktopClient.UserControls
{
    public static class FileExplorerHelpers
    {
        public static List<FileExplorerItem> GetLogicalDrives()
        {
            return Directory.GetLogicalDrives().Select(drive => new FileExplorerItem { FullPath = drive, Type = FileExplorerItemType.Drive }).ToList();
        }

        public static string GetFileFolderName(string fullpath)
        {
            // Guard against null/empty
            if (string.IsNullOrEmpty(fullpath))
                return string.Empty;

            // Normalise linux/windows paths
            var normalisedPath = fullpath.Replace('/', '\\');

            // Find the last folder seperator
            var lastIndex = normalisedPath.LastIndexOf('\\');

            // Guard against seperator at begining or not present
            if (lastIndex <= 0)
                return fullpath;

            return fullpath.Substring(lastIndex + 1);

        }

        public static List<FileExplorerItem> GetFolderContents(string fullpath)
        {
            var items = new List<FileExplorerItem>();

            //Add directories
            try
            {
                var dirs = Directory.GetDirectories(fullpath);
                if (dirs.Length > 0)
                    items.AddRange(dirs.Select(dir => new FileExplorerItem { FullPath = dir, Type = FileExplorerItemType.Folder }));
            }
            catch (System.Exception e)
            {
                // Ignore the excepts that can be thrown as we will just not add anything to the list for folders
            }
            //Add Files
            try
            {
                var files = Directory.GetFiles(fullpath);
                if (files.Length > 0)
                    items.AddRange(files.Select(file => new FileExplorerItem { FullPath = file, Type = FileExplorerItemType.File }));
            }
            catch (System.Exception e)
            {
                // Ignore the excepts that can be thrown as we will just not add anything to the list for files
            }

            return items;
        }
    }
}
