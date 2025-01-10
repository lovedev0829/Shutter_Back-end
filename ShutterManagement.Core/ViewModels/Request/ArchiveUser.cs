using ShutterManagement.Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class ArchiveUser
    {

        //[Required(ErrorMessage = "ContactId is required.")]
        public Guid? ContactId { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public bool Status { get; set; }

        public ArchiveType ArchiveType { get; set; }

        //[Required(ErrorMessage = "LastOrderDate is required.")]
        //[DataType(DataType.Date)]
        //[Range(typeof(DateTime), "01/01/2000", "12/31/2099", ErrorMessage = "LastOrderDate must be between 01/01/2000 and 12/31/2099.")]
        //public DateTime LastOrderDate { get; set; }
    }
}
