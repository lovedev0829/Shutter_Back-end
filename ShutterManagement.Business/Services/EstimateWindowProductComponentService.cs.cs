using ShutterManagement.Business.Interfaces;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Business.Services
{
    public class EstimateWindowProductComponentService : BaseRepository<BtprojecShuttermanagementDevContext, TblEstimateWindowProductComponent>, IEstimateWindowProductComponentService
    {
    }
}
