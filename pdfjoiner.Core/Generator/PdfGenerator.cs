﻿using pdfjoiner.Core.Models;
using System;

namespace pdfjoiner.Core.Generator
{
    public class PdfGenerator
    {
        #region Properties

        #endregion

        #region Attributes
        private DocumentListModel documentList;
        /// <summary>
        /// The list of pdf documents.
        /// </summary>
        public DocumentListModel DocumentList
        {
            get { return documentList; }
            set { documentList = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor which initiates empty properties.
        /// </summary>
        public PdfGenerator()
        {
            //initiate a new list
            documentList = new DocumentListModel();
        }

        /// <summary>
        /// Constructor which takes in a predined document list.
        /// </summary>
        /// <param name="list">The document list to be included.</param>
        public PdfGenerator(DocumentListModel list)
        {
            //Guard statement
            if (list == null)
                throw new ArgumentNullException("list");

            //store the provided list locally
            documentList = list;
        }
        #endregion
    }
}