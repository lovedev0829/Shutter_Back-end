using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShutterManagement.Core.SPModel;
using System.Runtime;
using ShutterManagement.GHL.Service;
public interface IGhlApiClient
{
    Task<List<Customer>> GetContactsAsync();
    Task<bool> CreateContactAsync(Customer contact);
}

public class GhlApiClient : IGhlApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<GhlApiClient> _logger;
    private readonly GhlSettings _ghlSettings;

    public GhlApiClient(HttpClient client, ILogger<GhlApiClient> logger, IOptions<GhlSettings> options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ghlSettings = options.Value;
    }

    public async Task<List<Customer>> GetContactsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching contacts from GHL...");

            var response = await _client.GetAsync("contacts");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var contactResponse = JsonConvert.DeserializeObject<GhlContactResponse>(jsonResponse);

            return contactResponse?.Contacts ?? new List<Customer>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching contacts from GHL.");
            throw;
        }
    }

    public async Task<bool> CreateContactAsync(Customer contact)
    {
        try
        {
            _logger.LogInformation($"Creating contact {contact.FirstName} {contact.LastName} in GHL...");

            var contactData = new
            {
                firstName = contact.FirstName,
                lastName = contact.LastName,
                email = contact.Email,
                phone = contact.MobilePh,
            };

            var contactJson = JsonConvert.SerializeObject(contactData);
            var content = new StringContent(contactJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("contacts", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var contactResponse = JsonConvert.DeserializeObject<ContactResponse>(jsonResponse);

            if (!string.IsNullOrEmpty(contactResponse.Contact?.Id))
            {
                _logger.LogInformation($"Contact created successfully with ID: {contactResponse.Contact.Id}");
                bool opportunityCreated = await CreateOpportunityInGhl(contactResponse.Contact, contactResponse.Contact.Id);
                return opportunityCreated;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contact in GHL.");
            return false;
        }
    }

    public async Task<bool> CreateOpportunityInGhl(Customer contact, string contactId)
    {
        //var pipelineId = "CFoi90n88zrDhMFP8R2I";  // Your actual pipeline ID
        // var stageId = "51740389-e764-4f93-9ff3-5c6b5c046ef1";  // Your actual stage ID

        var opportunityData = new
        {
            title = $"{contact.FirstName} {contact.LastName}",
            status = "open",
            stageId = _ghlSettings.StageId,
            contactId = contactId,
            email = contact.Email,
            phone = contact.MobilePh,
            name = $"{contact.FirstName} {contact.LastName}",
        };

        try
        {
            _logger.LogInformation($"Creating opportunity for contact {contact.FirstName} {contact.LastName}...");

            var endpoint = $"/v1/pipelines/{_ghlSettings.PipelineId}/opportunities/";
            var opportunityJson = JsonConvert.SerializeObject(opportunityData);
            var content = new StringContent(opportunityJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Opportunity created successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating opportunity in GHL.");
            return false;
        }
    }
}