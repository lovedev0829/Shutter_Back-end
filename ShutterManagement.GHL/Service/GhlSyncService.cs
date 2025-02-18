using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagement.GHL.Service
{
    public class GhlSyncService
    {
        private readonly IGhlApiClient _ghlApiClient;
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<GhlSyncService> _logger;

        public GhlSyncService(IGhlApiClient ghlApiClient, IDatabaseService databaseService, ILogger<GhlSyncService> logger)
        {
            _ghlApiClient = ghlApiClient ?? throw new ArgumentNullException(nameof(ghlApiClient));
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SyncAsync()
        {
            try
            {
                _logger.LogInformation("Starting sync process...");

                await SyncFromGhlToDb();
                await SyncFromDbToGhl();

                _logger.LogInformation("Sync process completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during sync.");
                throw;
            }
        }

        private async Task SyncFromGhlToDb()
        {
            var contacts = await _ghlApiClient.GetContactsAsync();

            foreach (var contact in contacts)
            {
                if (contact == null || string.IsNullOrEmpty(contact.Email))
                {
                    _logger.LogWarning("Skipping contact due to null or invalid data.");
                    continue;
                }

                bool isDuplicate = await _databaseService.CheckDuplicateAsync(contact);

                if (!isDuplicate)
                {
                    await _databaseService.InsertContactAsync(contact);
                    _logger.LogInformation($"Inserted contact {contact.FirstName} {contact.LastName} into database.");
                }
                else
                {
                    _logger.LogInformation($"Duplicate contact {contact.FirstName} {contact.LastName} skipped.");
                }
            }
        }

        private async Task SyncFromDbToGhl()
        {
            var unsyncedContacts = await _databaseService.GetUnsyncedContactsAsync();

            foreach (var contact in unsyncedContacts)
            {
                if (contact == null)
                {
                    _logger.LogWarning("Skipping contact due to null data.");
                    continue;
                }

                bool isSynced = await _ghlApiClient.CreateContactAsync(contact);

                if (isSynced)
                {
                    await _databaseService.UpdateSyncStatusAsync(contact);
                    _logger.LogInformation($"Updated sync status for contact {contact.FirstName} {contact.LastName}.");
                }
            }
        }
    }
}
