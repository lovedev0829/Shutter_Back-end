using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.ViewModels.Request
{
    public class UserDetails
    {
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? FirstName { get; set; }
        [DefaultValue("")]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? MiddleName { get; set; }
        [DefaultValue("")]
        [Required]
        [MaxLength(100)]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = AppConstant.AlphabaticalError)]
        public string? LastName { get; set; }
        [DefaultValue("")]
        [Required, MaxLength(100)]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address.")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [DefaultValue("")]
        [Required]
        [MaxLength(10)]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]

        public string? Phone { get; set; }
        [DefaultValue("")]
        [Required]
        [MinLength(8), MaxLength(12)]
        public string? Password { get; set; }

        [DefaultValue("")]
        [Required]
        public Guid? RoleId { get; set; }
        [DefaultValue("")]
        public Guid? JobTitle { get; set; }
    }
}
