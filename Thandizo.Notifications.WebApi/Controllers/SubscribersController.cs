using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Thandizo.ApiExtensions.Filters;
using Thandizo.ApiExtensions.General;
using Thandizo.DataModels.Notifications;
using Thandizo.Notifications.BLL.Services;

namespace Thandizo.Notifications.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        ISubscriberService _service;

        public SubscribersController(ISubscriberService service)
        {
            _service = service;
        }

        [HttpGet("GetById")]
        [CatchException(MessageHelper.GetItemError)]
        public async Task<IActionResult> GetById([FromQuery] int subscriberId)
        {
            var response = await _service.Get(subscriberId);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);
        }

        [HttpGet("GetAll")]
        [CatchException(MessageHelper.GetListError)]
        public async Task<IActionResult> GetAll()
        {
            var response = await _service.Get();

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);
        }

        [HttpPost("Add")]
        [ValidateModelState]
        [CatchException(MessageHelper.AddNewError)]
        [ValidateModelState]
        public async Task<IActionResult> Add([FromBody]SubscriberDTO subscriber)
        {
            var outputHandler = await _service.Add(subscriber);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Created("", outputHandler.Message);
        }

        [HttpPut("Update")]
        [ValidateModelState]
        [CatchException(MessageHelper.UpdateError)]
        public async Task<IActionResult> Update([FromBody]SubscriberDTO subscriber)
        {
            var outputHandler = await _service.Update(subscriber);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Ok(outputHandler.Message);
        }

        [HttpDelete("Delete")]
        [CatchException(MessageHelper.DeleteError)]
        public async Task<IActionResult> Delete([FromQuery]int subscriberId)
        {
            var outputHandler = await _service.Delete(subscriberId);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Ok(outputHandler.Message);
        }
    }
}
