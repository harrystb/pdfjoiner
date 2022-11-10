using System.Collections.Generic;

namespace pdfjoiner.Core.Models
{
    /// <summary>
    /// A model which contains the information for a document generation
    /// </summary>
    public class DocumentJoinModel
    {
        #region Constructor
        public DocumentJoinModel(DocumentListModel documentList, string joinString)
        {
            DocumentList = documentList;
            JoinString = joinString;
        }
        #endregion

        #region Properties
        public DocumentListModel DocumentList { get; set; }
        public string JoinString { get; set; }

        public List<DocumentSegmentModel> DocumentSegments { get; set; }
        #endregion

        #region Attributes
        #endregion

        #region Public Methods
        #endregion


        #region Private Methods
        #endregion


    }
}
