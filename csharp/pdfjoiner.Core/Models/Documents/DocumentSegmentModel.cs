using System.Reflection.Metadata.Ecma335;

namespace pdfjoiner.Core.Models
{
    public class DocumentSegmentModel
    {
        #region Constructors
        /// <summary>
        /// A model containing a page range
        /// </summary>
        /// <param name="document">The document model</param>
        /// <param name="startPageIndex">The start of the range</param>
        /// <param name="endPageIndex">The end of the range</param>
        public DocumentSegmentModel(DocumentModel document, int startPageIndex, int endPageIndex)
        {
            if (document == null)
                throw new System.ArgumentNullException(nameof(document));
            Document = document;
            SetPageRange(startPageIndex, endPageIndex);
        }

        /// <summary>
        /// A model containing a single page
        /// </summary>
        /// <param name="document">The document model</param>
        /// <param name="pageIndex">The page to include</param>
        public DocumentSegmentModel(DocumentModel document, int pageIndex)
        {
            Document = document ?? throw new System.ArgumentNullException(nameof(document));
            SetPage(pageIndex);
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
        public int StartPageIndex { get; private set; }

        /// <summary>
        /// The page number for the first page in the range
        /// </summary>
        public int StartPageNumber
        {
            get { return StartPageIndex + 1; }
        }

        /// <summary>
        /// The end of the page range
        /// </summary>
        public int EndPageIndex { get; private set; }
        /// <summary>
        /// The page number for the last page in the range
        /// </summary>
        public int EndPageNumber
        {
            get { return EndPageIndex + 1; }
        }

        /// <summary>
        /// Retreive the name of the document
        /// </summary>
        public string DocumentName { get => Document?.Name; }
        #endregion

        #region Methods
        /// <summary>
        /// Set the page range
        /// </summary>
        /// <param name="startPageIndex">The page for the start of the range</param>
        /// <param name="endPageIndex">The page for the end of the range</param>
        public void SetPageRange(int startPageIndex, int endPageIndex)
        {
            //Guard against invalid page provided
            if (startPageIndex < 0 || startPageIndex > Document.LastPageIndex)
                throw new System.ArgumentException($"The index is not within the bounds of 0 to the length of the document, 0 to {Document.LastPageIndex}.", nameof(startPageIndex));
            if (endPageIndex > Document.LastPageIndex || endPageIndex < startPageIndex)
                throw new System.ArgumentException($"The index is not within the bounds of the StartPageIndex to length of the document, {StartPageIndex} to {Document.LastPageIndex}.", nameof(endPageIndex));
            StartPageIndex = startPageIndex;
            EndPageIndex = endPageIndex;
        }

        /// <summary>
        /// Set the end of the page range
        /// </summary>
        /// <param name="endPageIndex">The page for the end of the range</param>
        public void SetEndPage(int endPageIndex)
        {
            //Guard against invalid page provided
            if (endPageIndex > Document.LastPageIndex || endPageIndex < StartPageIndex)
                throw new System.ArgumentException($"The index is not within the bounds of the StartPageIndex to length of the document, {StartPageIndex} to {Document.LastPageIndex}.", nameof(endPageIndex));
            EndPageIndex = endPageIndex;
        }

        /// <summary>
        /// Set the start of the page range
        /// </summary>
        /// <param name="startPageIndex">The page for the start of the range</param>
        public void SetStartPage(int startPageIndex)
        {
            //Guard against invalid page provided
            if (startPageIndex < 0 || startPageIndex > EndPageIndex)
                throw new System.ArgumentException($"The index is not within the bounds of 0 to EndPageIndex, 0 to {EndPageIndex}.", nameof(startPageIndex));
            StartPageIndex = startPageIndex;
        }

        /// <summary>
        /// Set the page range to a single page
        /// </summary>
        /// <param name="pageIndex">The page</param>
        public void SetPage(int pageIndex)
        { 
            //Guard against invalid page provided
            if (pageIndex < 0 || pageIndex > Document.LastPageIndex)
                throw new System.ArgumentException($"The index is not within bounds of the document, 0 to {Document.LastPageIndex}.", nameof(pageIndex));
            StartPageIndex = pageIndex;
            EndPageIndex = pageIndex;
        }
        #endregion
    }
}
