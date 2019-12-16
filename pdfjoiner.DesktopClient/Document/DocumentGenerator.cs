#nullable enable
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace pdfjoiner
{
    public class DocumentGenerator
    {
        #region External DLL
        [DllImport("User32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_RESTORE = 9;
        #endregion

        #region Public Attributes
        private string _status = string.Empty;
        public string Status
        {
            get
            {
                return _status;
            }
            private set
            {
                _status = value;
                StatusCallback?.Invoke(_status);
            }
        }

        private Dictionary<string, DocumentItem> _documentItems = new Dictionary<string, DocumentItem>();
        public Dictionary<string, DocumentItem> DocumentItems
        { 
            get
            {
                return _documentItems;
            }

            private set
            {
                _documentItems = value;
            }
        }



        #endregion

        #region Private Attributes

        private System.Diagnostics.Process? GenerationProcess = null;
        private bool GenerationWindowVisible = false;
        private bool GenerationTerminateFlag = false;
        private Thread? GenerationThread = null;
        private string GeneratedPDFPath = string.Empty;
        private string NextKey = "A";
        private IntPtr GenerationProcessHWND = IntPtr.Zero;

        public delegate void StatusUpdateCallbackHandler(string status);
        private event StatusUpdateCallbackHandler? StatusCallback = null;

        #endregion

        #region Constructor
        public DocumentGenerator()
        {
            return;
        }
        #endregion

        #region Methods

        #region Document List Interaction

        /// <summary>
        /// Adds a document to the document list.
        /// </summary>
        /// <param name="path">Full path to the document.</param>
        /// <returns>Returns the allocated Key, or null if the document is already in the list.</returns>
        public string? AddDocumentToList(string path)
        {
            var newDoc = new DocumentItem(path);
            if (DocumentItems.ContainsValue(newDoc))
                //Document is already there
                return null;

            var key = GetNextKey();
            DocumentItems.Add(key, newDoc);
            Status = "Document successfully added.";
            return key;
        }

        /// <summary>
        /// Removes the given document from the Document List.
        /// </summary>
        /// <param name="key">The ID of the document to remove.</param>
        /// <returns>True if the item was removed successfully, false otherwise.</returns>
        public bool RemoveDocumentFromList(string key)
        {
            return DocumentItems.Remove(key);
        }

        /// <summary>
        /// Clears the Document List.
        /// </summary>
        public void ResetDocumentList()
        {
            DocumentItems = new Dictionary<string, DocumentItem>();
            NextKey = "A";
        }

        /// <summary>
        /// Returns the document item with the given ID.
        /// </summary>
        /// <param name="id">ID of the document to retrieve.</param>
        /// <returns>The Document item if found, otherwise null.</returns>
        public DocumentItem? GetDocument(string id)
        {
            if (!DocumentItems.ContainsKey(id))
                return null;
            return DocumentItems[id];
        }

        #endregion

        #region Callback registration

        /// <summary>
        /// Pass in the call back for when the status has been changed.
        /// </summary>
        /// <param name="callback"></param>
        public void SetStatusChangedCallback( StatusUpdateCallbackHandler callback)
        {
            StatusCallback = callback;
        }

        #endregion

        #region Generation Interaction
        /// <summary>
        /// Toggles the visibility of the command window running latex.
        /// </summary>
        public void ToggleProcessWindowVisibility()
        {
            //do nothing if it is not running
            if (GenerationProcess == null)
                return;
            //if visible then hide
            if (GenerationWindowVisible)
            {
                //ShowWindow(GenerationProcess.MainWindowHandle, SW_HIDE);
                ShowWindow(GenerationProcess.MainWindowHandle, SW_MINIMIZE);
                GenerationWindowVisible = false;
            } else
            //otherwise...show
            {
                ShowWindow(GenerationProcess.MainWindowHandle, SW_RESTORE);
                //ShowWindow(GenerationProcessHWND, SW_SHOW);
                GenerationWindowVisible = true;
            }
        }

        /// <summary>
        /// Terminates the Generation Process if it is running.
        /// </summary>
        /// <returns>False if there was an issue with stopping the process.</returns>
        public bool TerminateGeneration()
        {
            //If the generation process does not exist, return true
            if (GenerationProcess == null)
                return true;

            //If the terminate flag is already set, wait until the process gets cleaned up by the other thread
            if (GenerationTerminateFlag)
            {
                int count = 0;
                while (GenerationProcess != null || count < 10)
                {
                    count++;
                    Thread.Sleep(100); // wait 100ms and check again
                }
                if (GenerationProcess != null)
                    return false; //The process did not end within the 10 retrys
                return true; //the process did end
            }

            if (GenerationThread == null)
                return false; //Something has stopped the thread without stopping the process.

            //No other thread is currently terminating, so we will terminate it.
            GenerationTerminateFlag = true;
            GenerationThread.Join();
            GenerationThread = null;
            GenerationProcess.Dispose();
            GenerationProcess = null;
            //finished terminating, clear the flag
            GenerationTerminateFlag = false;
            return true;
        }

        /// <summary>
        /// Starts the generation the PDF. 
        /// The status attribute will update based on the success of the generations.
        /// </summary>
        /// <param name="GenerationString"></param>
        /// <returns>A empty string if no errors occured, otherwise the error description.</returns>
        public string Generate(string GenerationString)
        {
            // Check if it is already running
            if (GenerationProcess != null)
                return "Already running.";

            // Check the validation string.
            string validationErrors = ValidateGenerationString(GenerationString);
            if (validationErrors.Length > 0)
                return "Error - Invalid generation string: " + validationErrors;

            //Create the temporary folder for generation
            string generationFolder = AppDomain.CurrentDomain.BaseDirectory + "PDFJoinerTemp\\";
            System.IO.Directory.CreateDirectory(generationFolder);
            //Store the path for the latex and pdf file which will be created
            string latexFilePath = generationFolder + "pdfjoinertemp.tex";
            GeneratedPDFPath = generationFolder + "pdfjoinertemp.pdf";
            //Create the latex file
            System.IO.File.WriteAllText(latexFilePath, GetLatexCode(GenerationString));
            //Copy the relevant PDF files into the temp folder
            foreach (string documentReference in GetGenerationDocuments(GenerationString))
            {
                //Copy the relevant file
                System.IO.File.Copy(DocumentItems[documentReference].Path, generationFolder + DocumentItems[documentReference].Filename, true);
            }
            
            //generate the document
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = generationFolder,
                FileName = "CMD.exe",
                Arguments = "/C pdflatex.exe pdfjoinertemp.tex",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized,
                UseShellExecute = true
            };
            GenerationProcess = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };
            GenerationProcess.Start();
            //Wait until process has started before trying to record the handle
            //TODO : Somehow work out how to wait until it is initialised before trying to get the handle...
            //store the handle
            //GenerationProcessHWND = GenerationProcess.MainWindowHandle;
            GenerationWindowVisible = false;
            //ToggleProcessWindowVisibility();
            Status = "Generating.";
            GenerationThread = new Thread(ThreadProc);
            GenerationThread.Start();

            return string.Empty;
        }

        #endregion

        #endregion

        #region Helpers
        /// <summary>
        /// Monitors the generation process and terminate flags to control 
        /// </summary>
        private void ThreadProc()
        {
            //Check every second to see if the process is done, or terminate has been called
            while (!GenerationTerminateFlag && !(GenerationProcess == null) && !GenerationProcess.HasExited)
            {
                Thread.Sleep(1000);
            }
            //Check if terminate was pressed and the process is still running
            if (GenerationTerminateFlag && !(GenerationProcess == null) && !GenerationProcess.HasExited)
            {
                //terminate the process
                GenerationProcess.Kill();
                GenerationProcess.Dispose();
                GenerationProcess = null;
                Status = "Cancelled.";
                return;
            }

            //Check if the file was generated
            if (System.IO.File.Exists(GeneratedPDFPath))
            {
                //Get the save file location and copy it there
                SaveFileDialog saveFileDialog1 = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    Title = "Save the PDF File"
                };
                saveFileDialog1.ShowDialog();
                if (saveFileDialog1.FileName != "")
                {
                    string outputPdfPath = saveFileDialog1.FileName;
                    System.IO.File.Move(GeneratedPDFPath, outputPdfPath, true);
                }
            }
            else
            {
                //not found - failed to generate.
                Status = "Error - generation of PDF failed.";
                return;
            }
            GenerationProcess?.Dispose();
            GenerationProcess = null;
            Status = "Done.";
        }

        /// <summary>
        /// Validated the generation string provided.
        /// </summary>
        /// <param name="GenerationString"></param>
        /// <returns>An empty string if successful, otherwise return a string containing the invalid page definitions.</returns>
        private string ValidateGenerationString(string GenerationString)
        {
            //If the string is empty, then just return ""
            if (GenerationString.Length == 0)
                return "";
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
                if (!(DocumentItems.ContainsKey(docId)))
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
        /// <summary>
        /// Generates the latex code to perform the joining as per the generation string.
        /// </summary>
        /// <returns>A string with the latex code if generation is sucessful, otherwise it returns an empty string.</returns>
        private string GetLatexCode(string generationString)
        {
            //Variable to hold the code while we add to it
            var latex = String.Empty;
            //Add in the start of the latex code
            latex += "\\documentclass[a4]{report}" + System.Environment.NewLine;
            latex += "\\usepackage[space]{grffile}" + System.Environment.NewLine;
            latex += "\\usepackage{pdfpages}" + System.Environment.NewLine;
            latex += "\\newcounter{mtpdfpage}" + System.Environment.NewLine;
            latex += "\\newsavebox{\\mtsavebox}" + System.Environment.NewLine;
            //Add in latex command for all pages auto sized
            latex += "\\newcommand{\\autoincludepdf}[1]{" + System.Environment.NewLine;
            latex += "\\pdfximage{#1}" + System.Environment.NewLine;
            latex += "\\setcounter{mtpdfpage}{1}" + System.Environment.NewLine;
            latex += "\\loop" + System.Environment.NewLine;
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpage]{#1}}" + System.Environment.NewLine;
            latex += "\\ifdim\\ht\\mtsavebox<\\wd\\mtsavebox" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper,landscape]{#1}" + System.Environment.NewLine;
            latex += "\\else" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper]{#1}" + System.Environment.NewLine;
            latex += "\\fi" + System.Environment.NewLine;
            latex += "\\stepcounter{mtpdfpage}" + System.Environment.NewLine;
            latex += "\\ifnum\\value{mtpdfpage}<\\numexpr\\pdflastximagepages+1\\relax" + System.Environment.NewLine;
            latex += "\\repeat" + System.Environment.NewLine;
            latex += "}" + System.Environment.NewLine;
            //Add in latex command for defined page range auto sized
            latex += "\\newcommand{\\autoincludepdfpages}[3]{" + System.Environment.NewLine;
            latex += "\\pdfximage{#1}" + System.Environment.NewLine;
            latex += "\\setcounter{mtpdfpage}{#2}" + System.Environment.NewLine;
            latex += "\\loop" + System.Environment.NewLine;
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpage]{#1}}" + System.Environment.NewLine;
            latex += "\\ifdim\\ht\\mtsavebox<\\wd\\mtsavebox" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper,landscape]{#1}" + System.Environment.NewLine;
            latex += "\\else" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper]{#1}" + System.Environment.NewLine;
            latex += "\\fi" + System.Environment.NewLine;
            latex += "\\stepcounter{mtpdfpage}" + System.Environment.NewLine;
            latex += "\\ifnum\\value{mtpdfpage}<\\numexpr #3+1\\relax" + System.Environment.NewLine;
            latex += "\\repeat" + System.Environment.NewLine;
            latex += "}" + System.Environment.NewLine;
            //Add in latex command for all pages from given start page auto sized
            latex += "\\newcommand{\\autoincludepdfstartpage}[2]{" + System.Environment.NewLine;
            latex += "\\pdfximage{#1}" + System.Environment.NewLine;
            latex += "\\setcounter{mtpdfpage}{#2}" + System.Environment.NewLine;
            latex += "\\loop" + System.Environment.NewLine;
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpage]{#1}}" + System.Environment.NewLine;
            latex += "\\ifdim\\ht\\mtsavebox<\\wd\\mtsavebox" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper,landscape]{#1}" + System.Environment.NewLine;
            latex += "\\else" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper]{#1}" + System.Environment.NewLine;
            latex += "\\fi" + System.Environment.NewLine;
            latex += "\\stepcounter{mtpdfpage}" + System.Environment.NewLine;
            latex += "\\ifnum\\value{mtpdfpage}<\\numexpr\\pdflastximagepages+1\\relax" + System.Environment.NewLine;
            latex += "\\repeat" + System.Environment.NewLine;
            latex += "}" + System.Environment.NewLine;
            //Add in latex command for all pages up to given end page auto sized
            latex += "\\newcommand{\\autoincludepdflastpage}[2]{" + System.Environment.NewLine;
            latex += "\\pdfximage{#1}" + System.Environment.NewLine;
            latex += "\\setcounter{mtpdfpage}{1}" + System.Environment.NewLine;
            latex += "\\loop" + System.Environment.NewLine;
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpage]{#1}}" + System.Environment.NewLine;
            latex += "\\ifdim\\ht\\mtsavebox<\\wd\\mtsavebox" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper,landscape]{#1}" + System.Environment.NewLine;
            latex += "\\else" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper]{#1}" + System.Environment.NewLine;
            latex += "\\fi" + System.Environment.NewLine;
            latex += "\\stepcounter{mtpdfpage}" + System.Environment.NewLine;
            latex += "\\ifnum\\value{mtpdfpage}<\\numexpr #2+1\\relax" + System.Environment.NewLine;
            latex += "\\repeat" + System.Environment.NewLine;
            latex += "}" + System.Environment.NewLine;
            //start of the latex document
            latex += "\\begin{document}" + System.Environment.NewLine;
            //For each part of the generation string, add in the relevant includepdf line
            foreach(string generationCode in generationString.Split(','))
            {
                (string id, string pageText) = GetIdAndPages(generationCode);
                //check to make sure the generation code actually has something
                if (string.IsNullOrEmpty(id))
                    continue;
                //check what type of document include is needed
                if (IsWholeDocument(generationCode))
                {
                    //Add the whole document generation command
                    latex += "\\autoincludepdf{";
                    latex += DocumentItems[id].Filename;
                    latex += "}\n";
                } else
                {
                    //Assume valid generation string as it has been checked already
                    //To get to this point the code must be A1-2, A-2, A2- or A1, or A
                    var pages = pageText.Split('-');
                    if (pages.Length == 1)
                    {
                        //only adding a single page
                        latex += "\\autoincludepdfpages{";
                        latex += DocumentItems[id].Filename;
                        latex += "}{";
                        latex += pages[0];
                        latex += "}{";
                        latex += pages[0];
                        latex += "}\n";
                    } 
                    else if (pages[0].Length == 0)
                    {
                        //If the page number before the - is blank, then it must be a up to page definition
                        latex += "\\autoincludepdflastpage{";
                        latex += DocumentItems[id].Filename;
                        latex += "}{";
                        latex += pages[1];
                        latex += "}\n";

                    }
                    else if (pages[1].Length == 0)
                    {
                        latex += "\\autoincludepdfstartpage{";
                        latex += DocumentItems[id].Filename;
                        latex += "}{";
                        latex += pages[0];
                        latex += "}\n";
                    }
                    else
                    {
                        latex += "\\autoincludepdfpages{";
                        latex += DocumentItems[id].Filename;
                        latex += "}{";
                        latex += pages[0];
                        latex += "}{";
                        latex += pages[1];
                        latex += "}\n";
                    }
                }
                latex += System.Environment.NewLine;
            }

            //Add in the end of the latex code
            latex += "\\end{document}" + System.Environment.NewLine;
            return latex;

        }

        /// <summary>
        /// Gets the next key and updates the next key for the next time this function is called.
        /// </summary>
        /// <returns>The key as a string.</returns>
        private string GetNextKey()
        {
            //store the key to return
            var key = NextKey;

            //update NextKey
            NextKey = UpdateKeyPart(NextKey);

            return key;
        }

        private string UpdateKeyPart(string KeyFragment)
        {
            //get the last char
            char last = KeyFragment[^1];
            //we are at the end of the string
            if (!(last == 'Z'))
                return KeyFragment[0..^1] + (++last).ToString();
            //Need to update another character
            //Easy case - single letter
            if (KeyFragment.Length == 1)
                //change value to AA
                return "AA";

            return UpdateKeyPart(KeyFragment[0..^1]) + 'A';
        }

        /// <summary>
        /// Checks if a valid generation code is including the whole document or partial document.
        /// </summary>
        /// <param name="generationCode></param>
        /// <returns>True if generation code does not specify a page range, otherwise false.</returns>
        private bool IsWholeDocument(string generationCode)
        {
            //get the id and page portions of the code
            (_, string pages) = GetIdAndPages(generationCode);
            //Assumes generation code is valid as it should have already been validated.

            //Only a document ID given, must be A style, must be whole document
            if (string.IsNullOrEmpty(pages))
                return true;
            //pages is '-' only, must be whole document as well 
            if (pages.Length == 1 && pages[0] == '-') 
                return true;

            //Cannot be a whole document
            return false;
        }

        /// <summary>
        /// Returns a list of the unique document references in the generation string.
        /// </summary>
        /// <param name="generationString"> The generation string.</param>
        /// <returns></returns>
        private List<string> GetGenerationDocuments(string generationString)
        {
            List<string> documentReferences = new List<string>();
            //For each generation code, add the document reference to the list if it doesn't already exist there.
            foreach (string generationCode in generationString.Split(','))
            {
                //Get the document reference from the string
                (string documentReference, _) = GetIdAndPages(generationCode);
                //if id is empty, then ignore
                if (string.IsNullOrEmpty(documentReference))
                    continue;
                //if id is not already included, then add it to the list
                if (!documentReferences.Contains(documentReference))
                {
                    documentReferences.Add(documentReference);
                }
            }
            return documentReferences;
        }

        private (string, string) GetIdAndPages(string generationCode)
        {
            int i = 0;
            for (; i < generationCode.Length; i++)
            {
                if (char.IsDigit(generationCode[i]) || generationCode[i] == '-')
                    break;
            }
            string id = generationCode.Substring(0, i);
            string pages = generationCode.Substring(i);
            return (id, pages);
        }

        #endregion

    }
}
