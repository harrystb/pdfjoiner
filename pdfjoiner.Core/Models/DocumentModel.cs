using System;
using System.Collections.Generic;
using System.Text;

namespace pdfjoiner.Core.Models
{
    public class DocumentModel
    {

        private string path = string.Empty;
        /// <summary>
        /// Full path to of the document.
        /// </summary>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        private string name = string.Empty;

        public DocumentModel(string fullpath)
        {
            path = fullpath;
        }

        /// <summary>
        /// The name of the document.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


    }
}
