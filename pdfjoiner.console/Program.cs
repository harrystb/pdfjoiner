using pdfjoiner.Core.Generator;
using pdfjoiner.Core.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace pdfjoiner.console
{
    static class Program
    {
        static void Main(string[] args)
        {
            // Parse the arguments
            //TESTING: Arg1 = PDF A, Arg2 = PDF B, Arg3 = Join String, Arg4 = Output document
            if (args.Length != 4)
            {
                Console.WriteLine("Incorrect number of arguments provided.");
                Console.WriteLine("Usage: pdfjoiner <pdf1> <pdf2> <joinString> <outputPdf>");
                Environment.Exit(1);
            }
            string pdfPath1 = args[0];
            string pdfPath2 = args[1];
            string joinString = args[2];
            string outputPath = args[3];
            if (!Path.HasExtension(outputPath))
            {
                outputPath += ".pdf";
                Console.WriteLine($"Extension not provided in the output file path. Changed to '{outputPath}'");
            }
            try
            {
                var documentSegments = joinString.Split(",").Aggregate(new List<DocumentSegmentModel>(), (segmentList, nextSegment) =>
                {
                    string pdfPath = string.Empty;
                    int startIndex = 0;
                    int endIndex = 0;
                    if (nextSegment[0] == 'A')
                    {
                        pdfPath = pdfPath1;
                        
                    }
                    else if (nextSegment[0] == 'B')
                    {
                        pdfPath = pdfPath2;
                    }
                    else
                    {
                        throw new System.ArgumentException($"A letter other than A or B was provided to the joinString in the segment '{nextSegment}'.");
                    }
                    var document = new DocumentModel(pdfPath);
                    var pageRangeString = nextSegment.Substring(1);
                    if (pageRangeString == string.Empty)
                    {
                        startIndex = 0;
                        endIndex = document.LastPageIndex;
                    }
                    else 
                    {
                        var pageRanges = pageRangeString.Split("-");
                        if (pageRanges[0] == "")
                        {
                            startIndex = 0;
                        }
                        else 
                        {
                            if (!int.TryParse(pageRanges[0], out startIndex))
                                throw new System.ArgumentException($"An invalid page number '{pageRanges[0]}' was provided in the segment '{nextSegment}'");
                            if (startIndex < 0)
                                throw new System.ArgumentException($"The start index of the range '{pageRangeString}' in the segment '{nextSegment}' is less than 0.");
                            if (startIndex > document.LastPageIndex)
                                throw new System.ArgumentException($"The start index of the range '{pageRangeString}' in the segment '{nextSegment}' is greater than the maximum for the document '{document.LastPageIndex}'");
                        }
                        if (pageRanges.Length == 1)
                        {
                            endIndex = startIndex;
                        }
                        else if (pageRanges[1] == "")
                        {
                            endIndex = document.LastPageIndex;
                        }
                        else
                        {
                            if (!int.TryParse(pageRanges[1], out endIndex))
                                throw new System.ArgumentException($"An invalid page number '{pageRanges[1]}' was provided in the segment '{nextSegment}'");
                            if (endIndex > document.LastPageIndex)
                                throw new System.ArgumentException($"The end index of the range '{pageRangeString}' in the segment '{nextSegment}' is greater than the maximum for the document '{document.LastPageIndex}'");
                            if (endIndex < startIndex)
                                throw new System.ArgumentException($"The end index of the range '{pageRangeString}' in the segment '{nextSegment}' is less than the start index.");
                        }

                    }

                    segmentList.Add(new DocumentSegmentModel(document, startIndex, endIndex));
                    return segmentList;
                });

                var pdfGenerator = new PdfGenerator(documentSegments);
                pdfGenerator.GenerateDocumentToFile(outputPath);
                Console.WriteLine($"Document Generated. See {outputPath}");
            }
            catch (System.IO.FileNotFoundException e)
            {
                Console.WriteLine($"{e.Message}");
            }
            catch (System.ArgumentException e)
            {
                Console.WriteLine($"{e.Message}");
            }
        }
    }
}
