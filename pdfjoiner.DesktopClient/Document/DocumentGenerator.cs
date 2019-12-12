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
        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        #endregion

        #region Public Attributes
        private string _status = string.Empty;
        private string Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                StatusCallback?.Invoke(_status);
            }
        }
        #endregion

        #region Private Attributes

        private Dictionary<string, DocumentItem> DocumentItems = new Dictionary<string, DocumentItem>();
        private System.Diagnostics.Process? GenerationProcess = null;
        private bool GenerationWindowVisible = false;
        private bool GenerationTerminateFlag = false;
        private Thread? GenerationThread = null;
        private string GeneratedPDFPath = string.Empty;

        private delegate void StatusUpdateCallbackHandler(string status);
        private event StatusUpdateCallbackHandler? StatusCallback = null;

        #endregion

        #region Constructor
        public DocumentGenerator()
        {
            return;
        }
        #endregion

        #region Methods
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
                ShowWindow(GenerationProcess.MainWindowHandle.ToInt32(), SW_HIDE);
            } else
            //otherwise...show
            {
                ShowWindow(GenerationProcess.MainWindowHandle.ToInt32(), SW_SHOW);
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
                System.IO.File.Copy(DocumentItems[documentReference].Path, generationFolder + DocumentItems[documentReference].Filename);
            }
            
            //generate the document
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = generationFolder,
                FileName = "CMD.exe",
                Arguments = "/C ..\\MikTek\\texmfs\\install\\miktex\\bin\\x64\\pdflatex.exe pdfjoinertemp.tex",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };
            GenerationProcess = new System.Diagnostics.Process
            {
                StartInfo = startInfo
            };
            GenerationProcess.Start();
            GenerationWindowVisible = false;
            Status = "Generating.";
            GenerationThread = new Thread(ThreadProc);

            return string.Empty;
        }

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
                    System.IO.File.Copy(GeneratedPDFPath, outputPdfPath, true);
                }
            }
            else
            {
                //not found - failed to generate.
                Status = "Error - generation of PDF failed.";
                return;
            }
            Status = "Done.";
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
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpag]{#1}" + System.Environment.NewLine;
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
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpag]{#1}" + System.Environment.NewLine;
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
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpag]{#1}" + System.Environment.NewLine;
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
            latex += "\\newcommand{\\autoincludepdflastpage}[1]{" + System.Environment.NewLine;
            latex += "\\pdfximage{#1}" + System.Environment.NewLine;
            latex += "\\setcounter{mtpdfpage}{1}" + System.Environment.NewLine;
            latex += "\\loop" + System.Environment.NewLine;
            latex += "\\sbox{\\mtsavebox}{\\includegraphics[page=\\themtpdfpag]{#1}" + System.Environment.NewLine;
            latex += "\\ifdim\\ht\\mtsavebox<\\wd\\mtsavebox" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper,landscape]{#1}" + System.Environment.NewLine;
            latex += "\\else" + System.Environment.NewLine;
            latex += "\\includepdf[pages=\\themtpdfpage,fitpaper]{#1}" + System.Environment.NewLine;
            latex += "\\fi" + System.Environment.NewLine;
            latex += "\\stepcounter{mtpdfpage}" + System.Environment.NewLine;
            latex += "\\ifnum\\value{mtpdfpage}<\\numexpr #3+1\\relax" + System.Environment.NewLine;
            latex += "\\repeat" + System.Environment.NewLine;
            latex += "}" + System.Environment.NewLine;
            //start of the latex document
            latex += "\\begin{document}" + System.Environment.NewLine;
            //For each part of the generation string, add in the relevant includepdf line
            foreach(string generationCode in generationString.Split())
            {
                if (IsWholeDocument(generationCode))
                {
                    //Add the whole document generation command
                    latex += "\\autoincludepdf{";
                    latex += DocumentItems[generationCode.Substring(0,1)].Filename;
                    latex += "}\n";
                } else
                {
                    //Assume valid generation string as it has been checked already
                    //To get to this point the code must be A1-2, A-2 or A2-
                    var pages = generationCode.Substring(1).Split('-');
                    //If the page number before the - is blank, then it must be a up to page definition
                    if (pages[0].Length == 0)
                    {
                        latex += "\\autoincludepdflastpage{";
                        latex += DocumentItems[generationCode.Substring(0,1)].Filename;
                        latex += "}{";
                        latex += pages[1];
                        latex += "}\n";

                    }else if (pages[1].Length == 0)
                    {
                        latex += "\\autoincludepdfstartpage{";
                        latex += DocumentItems[generationCode.Substring(0,1)].Filename;
                        latex += "}{";
                        latex += pages[0];
                        latex += "}\n";
                    } else
                    {
                        latex += "\\autoincludepdfstartpage{";
                        latex += DocumentItems[generationCode.Substring(0,1)].Filename;
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
        /// Checks if a valid generation code is including the whole document or partial document.
        /// </summary>
        /// <param name="generationCode></param>
        /// <returns>True if generation code does not specify a page range, otherwise false.</returns>
        private bool IsWholeDocument(string generationCode)
        {
            //Assumes generation code is valid as it should have already been validated.
            //Whole Doc: A A-, Partial: A-2, A2-, A1-2
            //If length is 1 or 2, then it must be a whole document.
            if (generationCode.Length == 1 || generationCode.Length == 2) 
                return true;
            return false;
        }

        /// <summary>
        /// Returns a list of the unique document references in the generation string.
        /// </summary>
        /// <param name="generationString"> The generation string.</param>
        /// <returns></returns>
        List<string> GetGenerationDocuments(string generationString)
        {
            List<string> documentReferences = new List<string>();
            //For each generation code, add the document reference to the list if it doesn't already exist there.
            foreach (string generationCode in generationString.Split())
            {
                //Get the document reference from the string
                string documentReference = generationCode[0].ToString();
                //If 
                if (!documentReferences.Contains(documentReference))
                {
                    documentReferences.Add(documentReference);
                }
            }
            return documentReferences;
        }

        #endregion

    }
}
