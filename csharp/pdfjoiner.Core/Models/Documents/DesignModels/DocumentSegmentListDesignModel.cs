using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdfjoiner.Core.Models
{
    public class DocumentSegmentListDesignModel : BaseDesignModel
    {
        public static DocumentSegmentListDesignModel Instance => new DocumentSegmentListDesignModel();

        private ObservableCollection<DocumentSegmentModel> _DocumentSegments;
        public ObservableCollection<DocumentSegmentModel> DocumentSegments
        {
            get => _DocumentSegments;
            set => SetProperty(ref _DocumentSegments, value);
        }

        public DocumentSegmentListDesignModel()
        {
            _DocumentSegments = new ObservableCollection<DocumentSegmentModel>();
            DocumentSegments.Add(new DocumentSegmentDesignModel());
        }
    }
}
