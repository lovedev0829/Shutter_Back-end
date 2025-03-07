using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.GHL.Service
{
    public class GHLAsyncService
    {
        private readonly GhlService ghlService;

        public GHLAsyncService(GhlService ghlService)
        {
            this.ghlService = ghlService;
        }
        //private async Task SyncFromGhlToDb()
        //{
        //    var contacts = await ghlService.GetContactsAsync();

        //    foreach (var contact in contacts)
        //    {
        //        if (contact == null || string.IsNullOrEmpty(contact.Email))
        //        {
        //            //_logger.LogWarning("Skipping contact due to null or invalid data.");
        //            continue;
        //        }

        //        bool isDuplicate = await _databaseService.CheckDuplicateAsync(contact);

        //        if (!isDuplicate)
        //        {
        //            await _databaseService.InsertContactAsync(contact);
        //            _logger.LogInformation($"Inserted contact {contact.FirstName} {contact.LastName} into database.");
        //        }
        //        else
        //        {
        //            _logger.LogInformation($"Duplicate contact {contact.FirstName} {contact.LastName} skipped.");
        //        }
        //    }
        
    }
}
