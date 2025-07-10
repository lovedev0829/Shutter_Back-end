using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels
{
    public class GetAllBlobDocument
    {
        public Guid? DocumentId { get; set; }
        public string? DocumentTitle { get; set; }
        public string? BlobUrl { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

    }
}
