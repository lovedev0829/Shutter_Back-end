using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobScheduleController : ControllerBase
    {
        private readonly IJobSchedule jobSchedule;

        public JobScheduleController(IJobSchedule jobSchedule)
        {
            this.jobSchedule = jobSchedule;
        }
        [HttpPost("Create")]
        [Authorize(Roles = "JobScheduleAdd")]
        public async Task<IActionResult> CreateDiscoount([FromBody] CreateJobSchedule model)
        {
            var res = await jobSchedule.CreateJobSchedule(model);
            return Ok(res);
        }
        [HttpPost("Update")]
        [Authorize(Roles = "JobScheduleEdit")]
        public async Task<IActionResult> UpdateDiscount(UpdateJobSchedule model)
        {
            var res = await jobSchedule.UpdateJobSchedule(model);
            return Ok(res);
        }
        [HttpPost("Delete")]
        [Authorize(Roles = "JobScheduleDelete")]
        public async Task<IActionResult> DeleteDiscount(Guid JobScheduleId)
        {
            var res = await jobSchedule.DeleteJobSchedule(JobScheduleId);
            return Ok(res);
        }

        [HttpPost("ActiveInActive")]
        [Authorize(Roles = "JobScheduleEdit")]
        public async Task<IActionResult> ActiveInActive(Guid JobScheduleId)
        {
            var res = await jobSchedule.ActiveInActive(JobScheduleId);
            return Ok(res);
        }

        [HttpPost]
        [Route("GetAll")]
        [Authorize(Roles = "JobScheduleView")]
        public async Task<IActionResult> GetAll(Pagination filter)
        {
            var res = await jobSchedule.GetAll_JobSchedule(filter);
            return Ok(res);
        }

    }
}



