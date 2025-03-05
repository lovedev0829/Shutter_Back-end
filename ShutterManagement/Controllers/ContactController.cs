using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost("GetAll")]
        [Authorize(Roles = "ContactView,PartnerContactView")]
        public async Task<IActionResult> GetAllContacts(Pagination filter)
        {
            bool isInContactViewRole = HttpContext.User.IsInRole("PartnerContactView");
            var res = await _contactService.GetAll_Contact(filter, isInContactViewRole);
            res.Data = res.Data?.Where(x => x.IsArchive == false).ToList();
            return Ok(res);
        }


        [HttpPost("Create")]
        [Authorize(Roles = "ContactAdd")]
        public async Task<IActionResult> CreateContact([FromBody] Sp_Contact model)
        {

            var res = await _contactService.Create_Contact(model);
            return Ok(res);
        }

        [HttpPost("Update")]
        [Authorize(Roles = "ContactEdit")]
        public async Task<IActionResult> UpdateContact([FromBody] UpdateContact model)
        {

            var res = await _contactService.Update_Contact(model);
            return Ok(res);
        }

        [HttpPost("ActiveInactive")]
        [Authorize(Roles = "ContactEdit")]
        public async Task<IActionResult> ActiveInActiveContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await _contactService.ActiveInActiveContact(guid);
            return Ok(res);
        }

        [HttpPost("Delete")]
        [Authorize(Roles = "ContactDelete")]
        public async Task<IActionResult> DeleteContact(string ContactId)
        {
            Guid guid = Guid.Parse(ContactId);
            var res = await _contactService.DeleteContact(guid);

            return Ok(res);
        }

        [HttpPost("GetAllArchiveContacts")]
        [Authorize(Roles = "ArchiveCustomerView")]
        public async Task<IActionResult> GetAllArchiveContacts(Pagination filter)
        {
            var res = await _contactService.GetAll_Contact(filter, false);
            // res.Data = res.Data?.Where(x => x.De == true && x.IsActive==false).ToList();
            return Ok(res);
        }
        [HttpPost("ArchiveContacts")]
        [Authorize]
        public async Task<IActionResult> ArchiveContact(ArchiveUser archiveModel)
        {
            var res = await _contactService.ArchiveContacts(archiveModel);
            return Ok(res);
        }

        [HttpPost("ContactFromGHL")]
        [AllowAnonymous]
        public async Task<IActionResult> ContactFromGHL([FromBody] JsonElement contactData)
        {
            contactData.TryGetProperty("type", out var typeProp);
            contactData.TryGetProperty("firstName", out var firstNameProp);
            contactData.TryGetProperty("lastName", out var lastNameProp);
            contactData.TryGetProperty("email", out var emailProp);
            contactData.TryGetProperty("phone", out var phoneProp);
            contactData.TryGetProperty("dateAdded", out var dateAddedProp);
            contactData.TryGetProperty("locationId", out var locationId);
            contactData.TryGetProperty("id", out var id);

            DateTime? dateAdded = null;
            if (dateAddedProp.ValueKind != JsonValueKind.Null)
            {
                if (DateTime.TryParse(dateAddedProp.GetString(), out var parsedDate))
                {
                    dateAdded = parsedDate;
                }
                else
                {
                    dateAdded = null;
                }
            }
            string locationIdValue = locationId.ValueKind != JsonValueKind.Null ? locationId.GetString() ?? string.Empty : string.Empty;
            string ghlContactIdValue = id.ValueKind != JsonValueKind.Null ? id.GetString() ?? string.Empty : string.Empty;
            var contact = new Sp_Contact
            {
                ContactType = typeProp.ValueKind != JsonValueKind.Null ? typeProp.GetString() ?? string.Empty : string.Empty,
                FirstName = firstNameProp.ValueKind != JsonValueKind.Null ? firstNameProp.GetString() ?? string.Empty : string.Empty,
                LastName = lastNameProp.ValueKind != JsonValueKind.Null ? lastNameProp.GetString() ?? string.Empty : string.Empty,
                Email = emailProp.ValueKind != JsonValueKind.Null ? emailProp.GetString() ?? string.Empty : string.Empty,
                PhoneNumber = phoneProp.ValueKind != JsonValueKind.Null ? phoneProp.GetString() ?? string.Empty : string.Empty,
                lastAsync = dateAdded,
                locationId = locationIdValue,
                GHLContactId = ghlContactIdValue
            };
            var res = await _contactService.Create_Contact(contact);
            return Ok(res);
        }

        [HttpPost("UpdateContactFromGHL")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateContactFromGHL([FromBody] JsonElement contactData)
        {
            contactData.TryGetProperty("type", out var typeProp);
            contactData.TryGetProperty("firstName", out var firstNameProp);
            contactData.TryGetProperty("lastName", out var lastNameProp);
            contactData.TryGetProperty("email", out var emailProp);
            contactData.TryGetProperty("phone", out var phoneProp);
            contactData.TryGetProperty("dateUpdated", out var dateUpdatedProp);
            contactData.TryGetProperty("locationId", out var locationId);
            contactData.TryGetProperty("id", out var id);

            DateTime? dateAdded = null;
            if (dateUpdatedProp.ValueKind != JsonValueKind.Null)
            {
                if (DateTime.TryParse(dateUpdatedProp.GetString(), out var parsedDate))
                {
                    dateAdded = parsedDate;
                }
                else
                {
                    dateAdded = null;
                }
            }
            string locationIdValue = locationId.ValueKind != JsonValueKind.Null ? locationId.GetString() ?? string.Empty : string.Empty;
            string ghlContactIdValue = id.ValueKind != JsonValueKind.Null ? id.GetString() ?? string.Empty : string.Empty;
            var contact = new UpdateContact
            {
                ContactType = typeProp.ValueKind != JsonValueKind.Null ? typeProp.GetString() ?? string.Empty : string.Empty,
                FirstName = firstNameProp.ValueKind != JsonValueKind.Null ? firstNameProp.GetString() ?? string.Empty : string.Empty,
                LastName = lastNameProp.ValueKind != JsonValueKind.Null ? lastNameProp.GetString() ?? string.Empty : string.Empty,
                Email = emailProp.ValueKind != JsonValueKind.Null ? emailProp.GetString() ?? string.Empty : string.Empty,
                PhoneNumber = phoneProp.ValueKind != JsonValueKind.Null ? phoneProp.GetString() ?? string.Empty : string.Empty,
                lastAsync = dateAdded,
                locationId = locationIdValue,
                GHLContactId = ghlContactIdValue
            };
            var res = await _contactService.Update_Contact(contact);
            return Ok(res);
        }
    }
}

