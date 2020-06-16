using pdfjoiner.Core.Models;
using PdfSharpCore.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace pdfjoiner.Core.Generator
{
    public class PdfGenerator
    {
        #region Properties

        #endregion

        #region Attributes
        /// <summary>
        /// The list 
        /// </summary>
        private readonly IEnumerable<DocumentSegmentModel> DocumentSegments;

        private PdfDocument GeneratedPdf = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a PdfGenerator object from a list of document segments.
        /// </summary>
        public PdfGenerator(IEnumerable<DocumentSegmentModel> documentSegments)
        {
            //initiate a new list
            DocumentSegments = documentSegments;
        }

        /// <summary>
        /// Creates a PdfGenerator with a DocumentJoinModel
        /// </summary>
        public PdfGenerator(DocumentJoinModel documentJoinModel)
        {
            DocumentSegments = documentJoinModel.DocumentSegments;
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Generate the document
        /// </summary>
        public void GenerateDocument()
        {
            //Guard against repeated generations
            if (GeneratedPdf != null)
                throw new InvalidOperationException("Document has already been generated. GenerateDocument should not be called again.");
            GeneratedPdf = new PdfDocument();

            //Add all the necessary pages
            GeneratedPdf = DocumentSegments.Aggregate(GeneratedPdf, (GeneratedPdf, nextSegment) => 
            {
                for (int i = nextSegment.StartPageIndex; i <= nextSegment.EndPageIndex; i++)
                {
                    GeneratedPdf.AddPage(nextSegment.Document.GetPage(i), AnnotationCopyingType.DeepCopy);
                }
                return GeneratedPdf;
            });
            
        }

        /// <summary>
        /// Save the generated document to the provided path.
        /// </summary>
        /// <param name="path">The location to save the generated document</param>
        /// <returns></returns>
        public void SaveGeneratedDocument(string path)
        {
            GeneratedPdf.Save(path);
        }

        /// <summary>
        /// Generate and save the document to the provided path.
        /// </summary>
        /// <param name="path">The location to save the generated document</param>
        /// <returns></returns>
        public void GenerateDocumentToFile(string path)
        {
            //Generate a document
            GenerateDocument();
            SaveGeneratedDocument(path);
        }

        /// <summary>
        /// Generate the document
        /// </summary>
        public Task GenerateDocumentAsync()
        {
            return Task.Run(() => GenerateDocument());
        }

        /// <summary>
        /// Save the generated document to the provided path.
        /// </summary>
        /// <param name="path">The location to save the generated document</param>
        /// <returns></returns>
        public Task SaveGeneratedDocumentAsync(string path)
        {
            return Task.Run(() => SaveGeneratedDocument(path));
        }

        /// <summary>
        /// Generate and save the document to the provided path.
        /// </summary>
        /// <param name="path">The location to save the generated document</param>
        /// <returns></returns>
        public Task GenerateDocumentToFileAsync(string path)
        {
            return Task.Run(() => GenerateDocumentToFile(path));
        }
        #endregion
    }
}
