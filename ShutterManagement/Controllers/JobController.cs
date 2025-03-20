using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagment.GHL.ViewModel.Request;
using ShutterManagment.GHL.ViewModel.Response;
using System.Net;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService jobService;
        private readonly IContactService contactService;

        public JobController(IJobService jobService,IContactService contactService)
        {
            this.jobService = jobService;
            this.contactService = contactService;
        }


        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "JobAdd")]
        public async Task<IActionResult> Create([FromBody] CreateJob model)
        {
            var result = await jobService.Create(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        [Authorize(Roles = "JobEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateJob model)
        {
            var result = await jobService.UpdateJob(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "JobDelete")]
        public async Task<IActionResult> Delete(string Id)
        {
            var result = await jobService.DeleteJob(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        [Authorize(Roles = "JobEdit")]
        public async Task<IActionResult> ActiveInactive(string Id)
        {
            var result = await jobService.ActiveInactive(Id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "JobView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var result = await jobService.GetAll(filter);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateJobStatus")]
        [Authorize(Roles = "JobEdit")]
        public async Task<IActionResult> UpdateJobStatus(UpdateJobStatus model)
        {
            var result = await jobService.UpdateJobStatus(model);
            return Ok(result);
        }

        [HttpPost("OpportunityFromGHL")]
        [AllowAnonymous]
        public async Task<IActionResult> OpportunityFromGHL(GHLOpportunityTrigger opportunityTrigger)
        {
            try
            {
                var res = await jobService.OpportunityFromGHL(opportunityTrigger);
                return Ok(res);
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating contact: {ExceptionMessage}", ex.Message);
                return StatusCode(500, $"Internal server error by application: {ex.Message}");
            }
        }


        [HttpPost("UpdateOpportunityFromGHL")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateOpporunityFromGHL(GHLOpportunityTrigger opportunityData)
        {
            try
            {
                var res = await jobService.UpdateOpportunityFromGHL(opportunityData);
                return Ok(res);
            }
            catch(Exception ex)
            {
                FileLogger.Log("Error while creating opportunity: {ExceptionMessage}", ex.Message);
                return StatusCode(500, $"Internal server error by application: {ex.Message}");
            }
        }

    }
}
