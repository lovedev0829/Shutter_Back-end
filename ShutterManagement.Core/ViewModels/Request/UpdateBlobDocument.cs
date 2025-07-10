using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateBlobDocument : CreateBlobDocument
    {
        public Guid? DocumentId { get; set; }
    }
}
