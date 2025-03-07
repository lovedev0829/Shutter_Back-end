using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShutterManagement.DAL.Context;
using ShutterManagement.Services;

namespace ShutterManagement.GHL.Service
{
  
        public class CustomerRepository : BaseRepository<ShutterDBContext, Customer>
        {
            public CustomerRepository(ShutterDBContext dbContext) : base()
            {
            }

            // Override specific methods if needed for custom queries.
            public async Task<Customer> GetByEmailAsync(string email)
            {
                return await dbContext.Set<Customer>().AsNoTracking().FirstOrDefaultAsync(c => c.Email == email);
            }

            public async Task<List<Customer>> GetUnsyncedCustomersAsync()
            {
                return await dbContext.Set<Customer>().AsNoTracking().Where(c => c.SyncStatus == false && c.IsFromGHL == false).ToListAsync();
            }
        }
    }


