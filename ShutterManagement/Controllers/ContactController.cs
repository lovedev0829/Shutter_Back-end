using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Authorize(Roles = "ContactView")]
        public async Task<IActionResult> GetAllContacts(Pagination filter)
        {
            var res = await _contactService.GetAll_Contact(filter);
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
            var res = await _contactService.GetAll_Contact(filter);
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
    }
}
