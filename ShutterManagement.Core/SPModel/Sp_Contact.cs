using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.SPModel
{
    public class Sp_Contact
    {
        public Guid ContactId { get; set; }
        public string ContactType { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
    }
}
