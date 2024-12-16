using Microsoft.AspNetCore.Authorization;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShutterManagement.Business.Interfaces;
using ShutterManagement.Core.ViewModels.Request;

namespace ShutterManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpressionController : ControllerBase
    {
        private readonly IExpressionService expression;

        public ExpressionController(IExpressionService expression)
        {
            this.expression = expression;
        }


        [HttpPost]
        [Route("Create")]
       // [Authorize(Roles = "AddressAdd")]
        public async Task<IActionResult> Create([FromBody] CreateExpression model)
        {
            var result = await expression.Create_Expression(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> Update([FromBody] UpdateExpression model)
        {
            var result = await expression.Update_Expression(model);
            return Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[Authorize(Roles = "AddressDelete")]
        public async Task<IActionResult> Delete(Guid ExpressionId)
        {
            var result = await expression.Delete(ExpressionId);
            return Ok(result);
        }

        [HttpPost]
        [Route("ActiveInActive")]
        //[Authorize(Roles = "AddressEdit")]
        public async Task<IActionResult> ActiveInactive(Guid ExpressionId)
        {
            var result = await expression.ActiveInActive(ExpressionId);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetAll")]
        //[Authorize(Roles = "AddressView")]
        public async Task<IActionResult> GetAll_Expression(Pagination filter)
        {
            var result = await expression.Get_AllExpression(filter);
            return Ok(result);
        }



    }
}
