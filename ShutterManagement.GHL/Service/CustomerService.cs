using Microsoft.Extensions.Logging;
//using ShutterManagement.Repositories;
using ShutterManagement.Core;
using ShutterManagement.Interfaces;
using ShutterManagement.GHL.Service;

namespace ShutterManagement.Services
{
    public class CustomerService 
    {
        private readonly CustomerRepository _customerRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(CustomerRepository customerRepository, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> CheckDuplicateAsync(Customer contact)
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(contact.Email);
            return existingCustomer != null;
        }

        public async Task InsertContactAsync(Customer contact)
        {
            var customer = new Customer
            {
                Id = contact.Id,
                FirstName = contact.FirstName ?? "",
                LastName = contact.LastName ?? "",
                Email = contact.Email ?? "",
                MobilePh = contact.MobilePh ?? "",
                SyncStatus = true, 
                IsFromGHL = true 
            };

            await _customerRepository.Add(customer);
        }

        public async Task<List<Customer>> GetUnsyncedContactsAsync()
        {
            var customers = await _customerRepository.GetUnsyncedCustomersAsync();
            return customers.Select(c => new Customer
            {
                Id = c.Id,
                FirstName = c.FirstName.Split(' ')[0],
                LastName = string.Join(" ", c.LastName.Split(' ').Skip(1)),
                Email = c.Email,
                MobilePh = c.MobilePh
            }).ToList();
        }

        public async Task UpdateSyncStatusAsync(Customer contact)
        {
            var customer = await _customerRepository.GetByEmailAsync(contact.Email);
            if (customer != null)
            {
                customer.SyncStatus = true;
                await _customerRepository.Update(customer);
            }
        }
    }
}
