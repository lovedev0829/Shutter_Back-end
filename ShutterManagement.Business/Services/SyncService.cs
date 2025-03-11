//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using ShutterManagement.Core;
//using ShutterManagement.Core.SPModel;
//using ShutterManagement.DAL.Context;
//using ShutterManagement.GHL.Services;

//namespace ShutterManagement.Business.Services
//{
//    public class SyncService
//    {
//        private readonly ShutterDBContext dbContext;
//        private readonly GhlService ghlService;

//        public SyncService(ShutterDBContext dbContext, GhlService ghlService)
//        {
//            this.dbContext = dbContext;
//            this.ghlService = ghlService;
//        }

//        public async Task SyncContactsAsync()
//        {
//            var lastSyncDate = await GetLastSyncDateAsync(); 

//            // Fetch contacts from both sources
//            var ghlContacts = await GetGHLContactsAsync();
//            var appContacts = await GetAppContactsAsync();

//            // Convert lists to dictionaries for quick lookups
//            var ghlDict = ghlContacts.ToDictionary(c => c.ContactId);
//            var appDict = appContacts.ToDictionary(c => c.ContactId);

//            var tasks = new List<Task>();

//            // Step 1: Sync updates between GHL and Application
//            foreach (var appContact in appContacts)
//            {
//                if (ghlDict.TryGetValue(appContact.ContactId, out var ghlContact))
//                {
//                    // Contact exists in both systems, compare timestamps
//                    if (ghlContact.UpdatedAt > appContact.ModifiedAt)
//                    {
//                        // GHL has more recent data, update Application
//                        tasks.Add(UpdateApplicationContactAsync(appContact.ContactId, ghlContact));
//                    }
//                    else if (appContact.ModifiedAt > ghlContact.UpdatedAt)
//                    {
//                        // Application has more recent data, update GHL
//                        tasks.Add(UpdateGHLContactAsync(ghlContact.ContactId, appContact));
//                    }
//                }
//                else
//                {
//                    // Contact exists in Application but NOT in GHL → Create in GHL
//                    tasks.Add(CreateGHLContactAsync(appContact));
//                }
//            }

//            foreach (var ghlContact in ghlContacts)
//            {
//                if (!appDict.ContainsKey(ghlContact.ContactId))
//                {
//                    // Contact exists in GHL but NOT in Application → Create in Application
//                    tasks.Add(CreateApplicationContactAsync(ghlContact));
//                }
//            }

//            // Step 2: Handle Deletions
//            foreach (var appContact in appContacts)
//            {
//                if (!ghlDict.ContainsKey(appContact.ContactId))
//                {
//                    // Contact is in Application but missing in GHL → Soft delete in App
//                    tasks.Add(SoftDeleteApplicationContactAsync(appContact.ContactId));
//                }
//            }

//            foreach (var ghlContact in ghlContacts)
//            {
//                if (!appDict.ContainsKey(ghlContact.ContactId))
//                {
//                    // Contact is in GHL but missing in Application → Delete in GHL
//                    tasks.Add(DeleteGHLContactAsync(ghlContact.ContactId));
//                }
//            }

//            await Task.WhenAll(tasks);

//            // Step 3: Update last sync timestamp
//            await UpdateLastSyncDateAsync(DateTime.UtcNow);
//        }

//        private async Task<DateTime> GetLastSyncDateAsync()
//        {
//            var lastSyncDate = dbContext.TblGhlconfigurations.Where(x => x.TenantId == AppConstant.LoggedInTenantId).Select(x => x.LastSync).FirstOrDefault();
//            return (DateTime)lastSyncDate;
//        }

//        private async Task<GetContactResponse> GetGHLContactsAsync()
//        {
//            var contacts = await dbContext.TblContacts
//                .Where(x => x.TenantId == AppConstant.LoggedInTenantId)
//                .ToListAsync(); 

//            var getContactResponse = new GetContactResponse();

//            foreach (var contact in contacts)
//            {
//                getContactResponse.Add(contact); 
//            }

//            return getContactResponse;
//        }

//    }
//}
