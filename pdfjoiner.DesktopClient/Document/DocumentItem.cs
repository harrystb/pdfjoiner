using System.IO;
namespace pdfjoiner
{
    public class DocumentItem
    {

        #region Public Attributes
        /// <summary>
        /// The full path of the document
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// The filename of the document.
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// The number of pages inside the document
        /// </summary>
        public int NumberOfPages { get; set; }

        #endregion
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Path">Full path of the document to be added</param>
        public DocumentItem(string Path)
        {
            this.Path = Path;
            this.Filename = getFilenameFromPath(Path);
        }
        #endregion

        #region Helper Functions

        private string getFilenameFromPath(string path)
        {
            if (!File.Exists(path))
                return string.Empty;
            string normalisedPath = path.Replace('/', '\\');
            int  lastIndex = normalisedPath.LastIndexOf('\\');
            return path.Substring(lastIndex + 1, path.Length - lastIndex - 1);
        }

           private int getNumberOfPagesFromFile(string path)
        {
            if (!File.Exists(path))
                return 0;
            //TODO Finish this function. Add in more comments.
            throw new System.NotImplementedException();

        }
        #endregion

    }
}
