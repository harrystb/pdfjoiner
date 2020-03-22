using pdfjoiner.Core.Generator;
using pdfjoiner.Core.Models;
using System;
using System.Linq;

namespace pdfjoiner.console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Parse the arguments
            //TESTING: Arg1 = PDF A, Arg2 = PDF B, Arg3 = Join String
            if (args.Length != 4)
            {
                Console.WriteLine("Incorrect number of arguments provided.");
            }
            string pdfPath1 = args[1];
            string pdfPath2 = args[2];
            string joinString = args[3];

            //Set up the generator
            var documentList = new DocumentListModel();
            string pdf1Ref = documentList.AddDocument(pdfPath1);
            string pdf2Ref = documentList.AddDocument(pdfPath1);
            joinString = joinString.Split(",").Aggregate("", (a, b) =>
            {
                if (a.Length > 0)
                {
                    a += ",";
                }
                if (b[0] == 'A')
                {
                    return a + b.Replace("A", pdf1Ref);
                }
                else if (b[0] == 'B')
                {
                    return a + b.Replace("B", pdf2Ref);
                }
                else
                {
                    return a;
                }
            });
            var pdfGenerator = new PdfGenerator(documentList);

        }

        private void PrintUsage()
        {
            Console.WriteLine("pdfjoiner <PathToPDFA> <PathToPDFB> <JoinString>");
        }
    }
}
