using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UpdateJobContact : CreateJobContact
    {
        [Required(ErrorMessage = "JobContactId is required.")]
        public Guid? JobContactId { get; set; }
    }
}
