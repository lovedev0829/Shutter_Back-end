using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Business.Services;
using ShutterManagement.Core.Common;
using ShutterManagement.Core.SPModel;
using ShutterManagement.Core.ViewModels;
using ShutterManagement.Core.ViewModels.Request;
using ShutterManagement.Core.ViewModels.Response;
using ShutterManagement.Services;
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
        private readonly IAuthService authService;
        private readonly IGHLConfiguration gHLConfiguration;
        private readonly IAppointmentScheduleService appointmentScheduleService;

        public JobController(IJobService jobService,IContactService contactService
            ,IAuthService authService,IGHLConfiguration gHLConfiguration ,IAppointmentScheduleService appointmentScheduleService)
        {
            this.jobService = jobService;
            this.contactService = contactService;
            this.authService = authService;
            this.gHLConfiguration = gHLConfiguration;
            this.appointmentScheduleService = appointmentScheduleService;
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
        public async Task<IActionResult> OpportunityFromGHL(GHLOpportunityTrigger opportunityTrigger)
        {
            try
            {
                FileLogger.Log("Token print message....");

                if (!Request.Headers.TryGetValue("Authentication", out var token))
                {
                    FileLogger.Log("Token not found in request headers.");
                    return Unauthorized("Token is missing.");
                }

                FileLogger.Log("Token: {0}", token);
                var tenantId = authService.ReadTokenDetails(token);

                if (tenantId == null || string.IsNullOrEmpty(tenantId.Data))
                {
                    FileLogger.Log("Invalid token: Unable to extract tenant ID.");
                    return Unauthorized("Invalid token.");
                }

                bool isTenantExist = await gHLConfiguration.AnyAsync(x => x.TenantId.ToString().ToLower() == tenantId.Data.ToLower());
                if (!isTenantExist)
                {
                    FileLogger.Log($"Tenant ID {tenantId.Data} does not exist.");
                    return NotFound("Tenant does not exist.");
                }
                opportunityTrigger.WebhookTenantId = tenantId?.Data;
                var res = await jobService.OpportunityFromGHL(opportunityTrigger);
                if (res == null)
                {
                    return StatusCode(500, "Failed to process contact.");
                }

                FileLogger.Log("Contact saved: {0}", res.Message);
                return Ok(res);
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating contact: {0}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("UpdateOpportunityFromGHL")]
        public async Task<IActionResult> UpdateOpporunityFromGHL(GHLOpportunityTrigger opportunityData)
        {
            try
            {
                FileLogger.Log("Token print message....");

                if (!Request.Headers.TryGetValue("Authentication", out var token))
                {
                    FileLogger.Log("Token not found in request headers.");
                    return Unauthorized("Token is missing.");
                }

                FileLogger.Log("Token: {0}", token);
                var tenantId = authService.ReadTokenDetails(token);

                if (tenantId == null || string.IsNullOrEmpty(tenantId.Data))
                {
                    FileLogger.Log("Invalid token: Unable to extract tenant ID.");
                    return Unauthorized("Invalid token.");
                }

                bool isTenantExist = await gHLConfiguration.AnyAsync(x => x.TenantId.ToString().ToLower() == tenantId.Data.ToLower());
                if (!isTenantExist)
                {
                    FileLogger.Log($"Tenant ID {tenantId.Data} does not exist.");
                    return NotFound("Tenant does not exist.");
                }
                opportunityData.WebhookTenantId = tenantId?.Data;
                var res = await jobService.UpdateOpportunityFromGHL(opportunityData);
                if (res == null)
                {
                    return StatusCode(500, "Failed to process contact.");
                }

                FileLogger.Log("Contact saved: {0}", res.Message);
                return Ok(res);
            }
            catch (Exception ex)
            {
                FileLogger.Log("Error while creating contact: {0}", ex.Message);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("CreateAppointment")]
        [Authorize(Roles = "JobEdit")]
        public async Task<IActionResult> CreateAppointmentSchedule(CreateAppointment model)
        {
            var result = await appointmentScheduleService.CreateAppointmentSchedule(model);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAppointment")]
        [Authorize(Roles = "JobView")]
        public async Task<IActionResult> GetAppointment(Guid? JobId)
        {
            var result = await appointmentScheduleService.GetAppointmentSchedule(JobId);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateJobPowderCoatColor")]
        [Authorize(Roles = "JobEdit")]
        public async Task<IActionResult> UpdateJobPowderCoatColor(Guid? JobId, Guid? powderCoatColorId)
        {
            var res = await jobService.UpdateJobPowderCoatColor(JobId, powderCoatColorId);
            return Ok(res);
        }

        [HttpPost]
        [Route("AddOrUpdateOpportunityInGHL")]
        //[Authorize(Roles = "JobEdit")]
        public async Task<IActionResult> AddOrUpdateOpportunityInGHL(CreateJob model, bool? IsforUpdate)
        {
            var res = await jobService.AddOrUpdateOpportunityInGHL(model, (bool)IsforUpdate);
            return Ok(res);
        }
    }
}
