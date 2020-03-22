namespace pdfjoiner.Core.Models
{
    public class DocumentPageModel
    {
        #region Constructors
        /// <summary>
        /// A model containing a page range
        /// </summary>
        /// <param name="document">The document model</param>
        /// <param name="startPage">The start of the range</param>
        /// <param name="endPage">The end of the range</param>
        public DocumentPageModel(DocumentModel document, int startPage, int endPage)
        {
            //Guard against invalid pages provided
            if (startPage < 1 || startPage > document.NumPages)
                throw new System.ArgumentException($"Start page is not within the pages of the document, 1 to {document.NumPages}.", "startPage");
            if (endPage > document.NumPages || endPage < startPage)
                throw new System.ArgumentException($"Start page is not larger than the start page or is greater than the number of pages in the document, {startPage} to {document.NumPages}.", "endPage");
            Document = document;
            StartPage = startPage;
            EndPage = endPage;
        }

        /// <summary>
        /// A model containing a single page
        /// </summary>
        /// <param name="document">The document model</param>
        /// <param name="page">The page to include</param>
        public DocumentPageModel(DocumentModel document, int page)
        {
            //Guard against invalid page provided
            if (page < 1 || page > document.NumPages)
                throw new System.ArgumentException($"The page is not within the pages of the document, 1 to {document.NumPages}.", "page");
            Document = document;
            StartPage = page;
            EndPage = page;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The document model
        /// </summary>
        public DocumentModel Document { get; set; }


        /// <summary>
        /// The start of the page range
        /// </summary>
        public int StartPage { get; private set; }

        /// <summary>
        /// The end of the page range
        /// </summary>
        public int EndPage { get; private set; }

        #endregion

        #region Methods
        /// <summary>
        /// Set the page range
        /// </summary>
        /// <param name="startPage">The page for the start of the range</param>
        /// <param name="endPage">The page for the end of the range</param>
        public void SetPageRange(int startPage, int endPage)
        {
            //Guard against invalid page provided
            if (startPage < 1 || startPage > Document.NumPages)
                throw new System.ArgumentException($"Start page is not within the pages of the document, 1 to {Document.NumPages}.", "startPage");
            if (endPage > Document.NumPages || endPage < startPage)
                throw new System.ArgumentException($"Start page is not larger than the start page or is greater than the number of pages in the document, {startPage} to {Document.NumPages}.", "endPage");
            StartPage = startPage;
            EndPage = endPage;
        }

        /// <summary>
        /// Set the end of the page range
        /// </summary>
        /// <param name="endPage">The page for the end of the range</param>
        public void SetEndPage(int endPage)
        {
            //Guard against invalid page provided
            if (endPage > Document.NumPages || endPage < StartPage)
                throw new System.ArgumentException($"Start page is not larger than the start page or is greater than the number of pages in the document, {startPage} to {Document.NumPages}.", "endPage");
            EndPage = endPage;
        }

        /// <summary>
        /// Set the start of the page range
        /// </summary>
        /// <param name="startPage">The page for the start of the range</param>
        public void SetStartPage(int startPage)
        {
            //Guard against invalid page provided
            if (startPage < 1 || startPage > EndPage)
                throw new System.ArgumentException($"Start page is not within the pages of the document, 1 to {EndPage}.", "startPage");
            StartPage = startPage;
        }

        /// <summary>
        /// Set the page range to a single page
        /// </summary>
        /// <param name="page">The page</param>
        public void SetPage(int page)
        { 
            //Guard against invalid page provided
            if (page < 1 || page > Document.NumPages)
                throw new System.ArgumentException($"The page is not within the pages of the document, 1 to {Document.NumPages}.", "page");
            StartPage = page;
            EndPage = page;
        }
        #endregion
    }
}
