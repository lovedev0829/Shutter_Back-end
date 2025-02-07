using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Response
{
     public class GetAllNotification
    {
        public Guid? Id { get; set; }
        public string? Body { get; set; }
        public string? Subject { get; set; }

        public bool? IsReaded { get; set; }

        public  DateTime? CreatedDate { get; set; }

        public Guid? UserId { get; set; }
            
    }
}
