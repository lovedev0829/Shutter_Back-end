using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class CreateJobContact
    {
        [Required(ErrorMessage = "ContactTypeId is required.")]
        public Guid? ContactTypeId { get; set; }
        [Required(ErrorMessage = "Jobid is required.")]
        public Guid? Jobid { get; set; }
        [Required(ErrorMessage = "ContactId is required.")]
        public Guid? ContactId { get; set; }
    }
}
