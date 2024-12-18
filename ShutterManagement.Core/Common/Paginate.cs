using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.Core.Common
{
    public static class Paginate
    {
        public static List<T> Pagination<T>(IEnumerable<T> filteredRecords, Pagination filter)
        {
            return filteredRecords
                .Skip((filter.PageNo - 1) * filter.RecordPerPage)
                .Take(filter.RecordPerPage)
                .ToList();
        }

    }
}
