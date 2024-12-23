using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.DAL.Context;
using ShutterManagement.Interfaces;

namespace ShutterManagement.Business.Interfaces
{
    public interface IOrderWindowProductComponentService : IGenericRepository<TblOrderWindowProductComponent>
    {
    }
}
