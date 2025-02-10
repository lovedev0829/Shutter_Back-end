using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateNotification
    {
        public Guid? UserId { get; set; }

        public string? Subject { get; set; }

        public string? Body { get; set; }

        public bool? IsReaded { get; set; } = false;

        public DateTime? CreatedDate { get; set; }

        public  string? Url { get; set; }
        public  string? Type { get; set; }
        public  Guid? TypeId { get; set; }
    }
}
