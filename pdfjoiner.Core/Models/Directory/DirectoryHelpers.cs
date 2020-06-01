using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace pdfjoiner.Core.Models
{
    public static class DirectoryHelpers
    {
        public static List<DirectoryItem> GetLogicalDrives()
        {
            return Directory.GetLogicalDrives().Select(drive => new DirectoryItem { FullPath = drive, Type = DirectoryItemType.Drive }).ToList();
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

        public static bool HasPdfFileExtension(string fullpath)
        {
            if (string.IsNullOrEmpty(fullpath))
                return false;

            var lastIndex = fullpath.LastIndexOf('.');

            if (lastIndex <= 0)
                return false;

            if (fullpath.Substring(lastIndex) != ".pdf")
                return false;

            return true;
        }

        public static List<DirectoryItem> GetFolderContents(string fullpath)
        {
            var items = new List<DirectoryItem>();

            //Add directories
            try
            {
                var dirs = Directory.GetDirectories(fullpath);
                if (dirs.Length > 0)
                    items.AddRange(dirs.Select(dir => new DirectoryItem { FullPath = dir, Type = DirectoryItemType.Folder }));
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
                    items.AddRange(files.Select(file => new DirectoryItem { FullPath = file, Type = DirectoryItemType.File }));
            }
            catch (System.Exception e)
            {
                // Ignore the excepts that can be thrown as we will just not add anything to the list for files
            }

            return items;
        }
    }
}
