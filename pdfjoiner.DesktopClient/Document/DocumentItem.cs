﻿#nullable enable
using System;
using System.IO;
namespace pdfjoiner
{
    /// <summary>
    /// Information about a document item such as path, filename and number of pages.
    /// </summary>
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
            Filename = getFilenameFromPath(Path);
            NumberOfPages = getNumberOfPagesFromFile(Path)??"Unknown";
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

        /// <summary>
        /// Checks if a file exists, then attempts to find the number of pages if it is a PDF file.
        /// </summary>
        /// <param name="path">The fullpath to the file.</param>
        /// <returns>If the file does not exist or is empty or not a PDF then null is returned otherwise a string containing the number of pages if found, otherwise an empty string.</returns>
        private string? getNumberOfPagesFromFile(string path)
        {
            //If the file does not exist then return an empty string
            if (!File.Exists(path))
                return null;
            //Open the file for reading one line at a time
            var sr = new StreamReader(path);
            //Read the first line.
            string? line = sr.ReadLine();
            //Check if the file is a PDF file, return an null if it is not.
            if ((line != null && line.Contains("PDF")))
                return null;
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
                } else
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
                if (currentElement.Contains(">>")) {
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
            return numberOfPages;
        }
        /// <summary>
        /// Checks if there is the total page count element in the pdf element string
        /// </summary>
        /// <param name="element">The element to search</param>
        /// <returns>Page count if found, else null</returns>
        private string? CheckForPagesInElement(string element)
        {
            //If the element does not contain page count - return null
            if (!element.Contains("/Pages"))
                return null;

            //extract the Pages portion of the string
            string pageSection = element.Substring(element.IndexOf("/Pages") + 6);
            //Check if the /Count is after /Pages if not, them this element still does not contain total
            if (!pageSection.Contains("/Count"))
                return null;

            //Get the "Count" Element
            string countSection = pageSection.Substring(pageSection.IndexOf("/Count") + 6);

            //get the page count
            string pageCount = countSection.Substring(0, countSection.IndexOf("/Kid")).Trim();

            return pageCount;
        }

        #endregion

    }
}
