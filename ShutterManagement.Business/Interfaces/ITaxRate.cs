using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Interfaces
{
    public interface ITaxRate : IGenericRepository<TblTaxRate>
    {
        public Guid TaxRateId { get; set; }

        public Guid? Region { get; set; }

        public decimal? Rate { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

       
    }
}
