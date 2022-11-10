using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdfjoiner.Core.Models
{
    public class DocumentDesignModel : DocumentModel
    {
        public static DocumentDesignModel Instance => new DocumentDesignModel();
        public DocumentDesignModel() : base (null)
        {
            FullPath = "C:\\TestDir\\Test.file";
            Name = "Test.file";
        }
    }
}
