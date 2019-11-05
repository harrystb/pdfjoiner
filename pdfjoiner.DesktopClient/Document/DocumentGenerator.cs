
using System;
using System.Collections.Generic;

namespace pdfjoiner
{
    public class DocumentGenerator
    {       
        #region Private Attributes

        private IDictionary<string, DocumentItem> _DocumentItems;
        private bool GenerationRunning = false;
        private System.Diagnostics.Process GenerationProcess;

        #endregion

        #region Constructor
        public DocumentGenerator()
        {
            
        }
        #endregion

        #region Methods
        /// <summary>
        /// Generates the PDF.
        /// </summary>
        /// <param name="GenerationString"></param>
        /// <returns>A empty string if no errors occured, otherwise the error description.</returns>
        public string Generate(string GenerationString)
        {
            // Check if it is already running
            if (GenerationRunning)
                return "Already running.";

            // Check the validation string.
            string validationErrors = ValidateGenerationString(GenerationString);
            if (validationErrors.Length > 0)
                return "Error - Invalid generation string: " + validationErrors;

            //Create the generation folder and the tex file
            string generationFolder = AppDomain.CurrentDomain.BaseDirectory + "PDFJoinerTemp\\";
            //TODO -> Create folder and start process

            //Generate the document.
            GenerationProcess = new System.Diagnostics.Process();


        
            return string.Empty;
        }

        public void ShowTerminal()
        {

        }
        #endregion

        #region Helpers
        /// <summary>
        /// Validated the generation string provided.
        /// </summary>
        /// <param name="GenerationString"></param>
        /// <returns>An empty string if successful, otherwise return a string containing the invalid page definitions.</returns>
        private string ValidateGenerationString(string GenerationString)
        {
            //If the string is empty, then just return 0
            if (GenerationString.Length == 0)
                return 0;
            // Keep track of the current errors
            var errors = new List<string>(); 
            // Go through each page definition and and check it while keeping track of the current index
            foreach (string pageDef in GenerationString.Split(','))
            {
                //Empty page def
                if (pageDef.Length == 0)
                {
                    errors.Add(pageDef);
                    continue;
                }
                //Extract the document ID
                var docId = pageDef[0].ToString();
                //Document ID is not in list of documents
                if (!(_DocumentItems.ContainsKey(docId)))
                {
                    errors.Add(pageDef);
                    continue;
                }

                //Extract the numbers by splitting on either side of a "-". A single page string will leave length as 1.
                var docPages = pageDef.Substring(1).Split('-');

                //if length is more than 2, then there are too many "-" in the string
                if (docPages.Length > 2)
                {
                    errors.Add(pageDef);
                    continue;
                }

                //Check to make sure first page string provided is all digits, and also the second page string if the '-' is present.
                foreach (string pageString in docPages)
                {
                    if (!CheckNumber(pageString))
                    {
                        errors.Add(pageDef);
                        break;
                    }
                }
            }
            //return errors if any were found
            return string.Join(',', errors);
        }

        /// <summary>
        /// Check if the string is only numbers.
        /// </summary>
        /// <param name="s">A string to check.</param>
        /// <returns>True if the string only contains numbers</returns>
        private bool CheckNumber(string s)
        {
            bool isNumber = true;
            foreach (char c in s)
            {
                if (!char.IsDigit(c))
                    isNumber = false;
            }
            return isNumber;
        }

        #endregion

    }
}
