#nullable enable
using System;
using System.IO;
namespace pdfjoiner
{
    /// <summary>
    /// Information about a document item such as path, filename and number of pages.
    /// </summary>
    public class DocumentItem : IEquatable<DocumentItem>
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
        public string NumberOfPages { get; set; }

        #endregion
        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="Path">Full path of the document to be added</param>
        public DocumentItem(string path)
        {
            Path = path;
            Filename = GetFilenameFromPath(Path);
            NumberOfPages = GetNumberOfPagesFromFile(Path) ?? "Unknown";
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Checks Path and Filename of both DocumentItems to determine if they are equal. 
        /// Ignores Number of pages.
        /// </summary>
        /// <param name="other">The DocumentItem to compare against</param>
        /// <returns>True if Path and Filename are the same.</returns>
        public bool Equals(DocumentItem other)
        {
            if ((this.Path == other.Path) && (this.Filename == other.Filename))
                return true;
            return false;
        }

        #endregion

        #region Helper Functions

        private string GetFilenameFromPath(string path)
        {
            if (!File.Exists(path))
                return string.Empty;
            string normalisedPath = path.Replace('/', '\\');
            int lastIndex = normalisedPath.LastIndexOf('\\');
            return path.Substring(lastIndex + 1, path.Length - lastIndex - 1);
        }

        /// <summary>
        /// Checks if a file exists, then attempts to find the number of pages if it is a PDF file.
        /// </summary>
        /// <param name="path">The fullpath to the file.</param>
        /// <returns>If the file does not exist or is empty or not a PDF then null is returned otherwise a string containing the number of pages if found, otherwise an empty string.</returns>
        private string? GetNumberOfPagesFromFile(string path)
        {
            //If the file does not exist then return an empty string
            if (!File.Exists(path))
                return null;
            //Open the file for reading one line at a time
            var sr = new StreamReader(path);
            //Read the first line.
            string? line = sr.ReadLine();
            //Check if the file is a PDF file, return an null if it is not.
            if ((line != null && !(line.Contains("PDF"))))
            {
                sr.Dispose();
                return null;
            }
            //String buffer to hold the current element properties <</.../.../..>>
            string currentElement = string.Empty;
            //Bool to know whether we are in a element or not
            bool inElement = false;
            //string for holding the page number
            string? numberOfPages = null;
            while (line != null)
            {
                //Look for <</Type/Pages/Count X...>> - Note there may be extra spaces or newlines.
                if (inElement)
                {
                    //we are already in an element, so keep adding to the buffer
                    currentElement += line;
                }
                else
                {
                    //not yet in a buffer, only start adding if we find a start of the element
                    if (line.Contains("<<"))
                    {
                        //Found the start  of an element. add to buffer
                        currentElement = line.Substring(line.IndexOf("<<"));
                        inElement = true;
                    }
                }

                //check if there is a end of element symbol
                if (currentElement.Contains(">>"))
                {
                    numberOfPages = CheckForPagesInElement(currentElement);
                    //if we have part of the next element, then leave that intact
                    if (currentElement.LastIndexOf("<<") > currentElement.LastIndexOf(">>"))
                        currentElement = currentElement.Substring(currentElement.LastIndexOf("<<"));
                    else
                        currentElement = string.Empty;
                }
                //if we have the number of pages, then stop searching
                if (numberOfPages != null)
                    break;
                //read the next line.
                line = sr.ReadLine();
            }
            sr.Dispose();
            return numberOfPages;
        }
        /// <summary>
        /// Checks if there is the total page count element in the pdf element string
        /// </summary>
        /// <param name="element">The element to search</param>
        /// <returns>Page count if found, else null</returns>
        private string? CheckForPagesInElement(string element)
        {
            //If the element does not contain page count or if it is not the top level count - return null
            if (!element.Contains("/Pages") || element.Contains("/Parent"))
                return null;

            //Make sure count is present
            if (!element.Contains("/Count"))
                return null;

            int startIndex = element.IndexOf("/Count") + 7;
            int length = element.IndexOf('/', startIndex) - startIndex;
            if (length <= 0)
            {
                //There is no other words - look for end of element
                length = element.IndexOf('>', startIndex) - startIndex;
            }
            string count = element.Substring(startIndex, length);
            return count;
        }

        #endregion

    }
}
