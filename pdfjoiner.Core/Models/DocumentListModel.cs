using PdfSharp.Pdf;
using System;
using System.Collections.Generic;

namespace pdfjoiner.Core.Models
{
    /// <summary>
    /// Container for the list of documents
    /// </summary>
    public class DocumentListModel
    {
        #region Properties
        private Dictionary<string, DocumentModel> documentList;
        public Dictionary<string, DocumentModel> DocumentList
        {
            get
            {
                return documentList;
            }

            private set
            {
                documentList = value;
            }
        }


        #endregion

        #region Attributes

        /// <summary>
        /// Stores the key for the next document that gets added.
        /// </summary>
        string nextKey = "A";

        #endregion

        #region Public Methods
        /// <summary>
        /// Add a document into the document list.
        /// </summary>
        /// <param name="document">The document to be added.</param>
        /// <returns>The key reference for the document.</returns>
        public string AddDocument(DocumentModel document)
        {
            //store the key for the document to be returned later
            string key = nextKey;
            // add the document to the list
            documentList.Add(key, document);
            // update the next key attribute
            UpdateNextKey();
            //return the key of the document that was added
            return key;

        }

        /// <summary>
        /// Add a document to the list from the full path provided.
        /// </summary>
        /// <param name="fullpath">Full path to the document.</param>
        /// <returns>The key reference for the document.</returns>
        public string AddDocument(string fullpath)
        {
            //create the document model object
            DocumentModel document = new DocumentModel(fullpath);
            //call the other method with the new document model
            return AddDocument(document);
        }
        #endregion


        #region Private Methods

        /// <summary>
        /// Update the nextKey variable to the next available character in the sequence.
        /// A, B, ..., Z, AA, ..., AZ, ..., ZZ
        /// </summary>
        private void UpdateNextKey()
        {
            //start searching for the next key at the lowest possible key
            string key = "A";
            //run through possible keys starting from the bottom until a free key is found
            while (documentList.ContainsKey(key))
            {
                key = GetPossibleNextKey(key);
            }
            //update the attribute
            nextKey = key;
        }

        /// <summary>
        /// Returns the next possible key after the provided key. 
        /// Key range examples, A,...,Z,AA,...,AZ,...,ZZ,AAA,....
        /// </summary>
        /// <param name="key">The key to be incremented.</param>
        /// <returns>The incremented key.</returns>
        private string GetPossibleNextKey(string key)
        {
            //Guard against a null string argument
            if (key == null)
                throw new ArgumentNullException("key", "key provided is null, empty or whitespace");

            //if it is an empty string or just whitespace - return the default start "A"
            if (string.IsNullOrWhiteSpace(key))
                return "A";

            //get the last character
            char lastChar = key[key.Length - 1];

            //increment the last character if it is not "Z" and return the new key
            if ( lastChar != 'Z')
            {
                lastChar++;
                return key.Substring(0, key.Length - 1) + lastChar;
            }

            //last letter is Z - need to increment the rest of the string, then set the last char to "A"
            // for example Z -> GetPossibleNextKey("") + "A" -> "AA"
            return GetPossibleNextKey(key.Substring(0, key.Length - 1)) + "A";
        }

        #endregion


    }
}