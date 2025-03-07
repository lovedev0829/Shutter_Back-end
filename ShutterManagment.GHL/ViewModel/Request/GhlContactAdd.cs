using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Request
{
    public class GhlContactAdd
    {
        public string? Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Notes { get; set; }
        public string ContactType { get; set; }
        public string Website { get; set; }
        public string Referral { get; set; }
        public string? GHLContactId { get; set; }
    }
}
