using pdfjoiner.Core.Models;
using System;
using System.Threading.Tasks;

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

        //TODO: DocumentJoinModel version of the ctor
        #endregion

        public string GenerateDocument(string joinString)
        {
            //Generate a document
            return "TODO";
        }

        /// <summary>
        /// Async method to generate the document.
        /// </summary>
        /// <param name="joinString">The string which defines how the new document should be structured.</param>
        public Task<string> GenerateDocumentAsync(string joinString)
        {
            return Task.FromResult<string>(GenerateDocument(joinString));
        }

    }
}
