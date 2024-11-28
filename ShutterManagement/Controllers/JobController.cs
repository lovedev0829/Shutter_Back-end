using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;
using System.Net;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService jobService;

        public JobController(IJobService jobService)
        {
            this.jobService = jobService;
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
            var result = await jobService.Update(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        [Authorize(Roles = "JobDelete")]
        public async Task<IActionResult> Delete(string Id)
        {
            var result = await jobService.Delete(Id);
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
    }
}
