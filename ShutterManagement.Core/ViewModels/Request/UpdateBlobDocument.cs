using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateBlobDocument : CreateBlobDocument
    {
        [Required]
        public Guid? DocumentId { get; set; }
        [Required]
        public string? BlobUrl { get; set; }
    }
}
