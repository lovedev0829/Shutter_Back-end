using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShutterManagment.GHL.ViewModel.Response
{
    public class GetUserVM
    {
        public List<User> users { get; set; }
        public string traceId { get; set; }
    }
    public class LcPhone
    {
    }

    public class Permissions
    {
        public bool campaignsEnabled { get; set; }
        public bool campaignsReadOnly { get; set; }
        public bool workflowsEnabled { get; set; }
        public bool workflowsReadOnly { get; set; }
        public bool contactsEnabled { get; set; }
        public bool triggersEnabled { get; set; }
        public bool opportunitiesEnabled { get; set; }
        public bool settingsEnabled { get; set; }
        public bool tagsEnabled { get; set; }
        public bool leadValueEnabled { get; set; }
        public bool dashboardStatsEnabled { get; set; }
        public bool bulkRequestsEnabled { get; set; }
        public bool opportunitiesBulkActionsEnabled { get; set; }
        public bool appointmentsEnabled { get; set; }
        public bool reviewsEnabled { get; set; }
        public bool onlineListingsEnabled { get; set; }
        public bool phoneCallEnabled { get; set; }
        public bool conversationsEnabled { get; set; }
        public bool assignedDataOnly { get; set; }
        public bool funnelsEnabled { get; set; }
        public bool websitesEnabled { get; set; }
        public bool marketingEnabled { get; set; }
        public bool adwordsReportingEnabled { get; set; }
        public bool facebookAdsReportingEnabled { get; set; }
        public bool attributionsReportingEnabled { get; set; }
        public bool membershipEnabled { get; set; }
        public bool botService { get; set; }
        public bool agentReportingEnabled { get; set; }
        public bool socialPlanner { get; set; }
        public bool bloggingEnabled { get; set; }
        public bool invoiceEnabled { get; set; }
        public bool affiliateManagerEnabled { get; set; }
        public bool contentAiEnabled { get; set; }
        public bool refundsEnabled { get; set; }
        public bool recordPaymentEnabled { get; set; }
        public bool cancelSubscriptionEnabled { get; set; }
        public bool paymentsEnabled { get; set; }
        public bool communitiesEnabled { get; set; }
        public bool exportPaymentsEnabled { get; set; }
        public bool certificatesEnabled { get; set; }
        public bool mediaStorageEnabled { get; set; }
        public bool reportingEnabled { get; set; }
        public bool adPublishingEnabled { get; set; }
        public bool adPublishingReadOnly { get; set; }
        public bool wordpressEnabled { get; set; }
        public bool customMenuLinkReadOnly { get; set; }
        public bool customMenuLinkWrite { get; set; }
        public bool gokollabEnabled { get; set; }
    }

    public class Roles
    {
        public string type { get; set; }
        public string role { get; set; }
        public List<string> locationIds { get; set; }
    }
    

    public class User
    {
        public string id { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        // Add these fields for calendar info
        public string? CalendarId { get; set; }
        public string? CalendarName { get; set; }
        public Permissions permissions { get; set; }
        public Roles roles { get; set; }
        public List<string> scopes { get; set; }
        public List<string> scopesAssignedToOnly { get; set; }
        public LcPhone lcPhone { get; set; }
        public bool deleted { get; set; }
        public string membershipContactId { get; set; }
        public string freshdeskContactId { get; set; }
        public List<Calendar> Calendars { get; set; } // <-- Add this

        
    }

}
