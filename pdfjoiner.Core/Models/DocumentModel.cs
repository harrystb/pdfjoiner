﻿using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System.IO;

namespace pdfjoiner.Core.Models
{
    public class DocumentModel
    {
        #region Constructor
        /// <summary>
        /// Stores the information about a pdf document at the path provided.
        /// </summary>
        /// <param name="path">The full or relative path of the document</param>
        public DocumentModel(string path)
        {
            FullPath = Path.GetFullPath(path);
            Name = Path.GetFileNameWithoutExtension(path);
            Pdf = PdfReader.Open(FullPath, PdfDocumentOpenMode.Import);
        }
        #endregion

        #region Private Variables
        /// <summary>
        /// The Pdf imported from a file
        /// </summary>
        private readonly PdfDocument Pdf;
        #endregion

        #region Properties
        /// <summary>
        /// Full path to of the document.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// The name of the document.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of pages in a document
        /// </summary>
        public int NumPages
        {
            get
            {
                if (Pdf == null)
                    return 0;
                return Pdf.PageCount;
            }
        }
        #endregion
    }
}
