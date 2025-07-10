using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateBlobDocument
    {
        public string? DocumentTitle { get; set; }
        public string? BlobUrl { get; set; }
        public IFormFile? BlobDocument { get; set; }
    }
}
