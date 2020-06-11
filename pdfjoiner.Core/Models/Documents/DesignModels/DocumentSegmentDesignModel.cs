using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdfjoiner.Core.Models
{
    public class DocumentSegmentDesignModel : DocumentSegmentModel
    {
        public static DocumentSegmentDesignModel Instance => new DocumentSegmentDesignModel();
        public DocumentSegmentDesignModel() : base(new DocumentDesignModel(), 0, 0)
        {

        }
    }
}
