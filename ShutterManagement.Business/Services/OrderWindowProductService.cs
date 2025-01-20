using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.Business.Services
{
    public class OrderWindowProductService : BaseRepository<ShutterDBContext, TblOrderWindowProduct>, IOrderWindowProductService
    {
    }
}
