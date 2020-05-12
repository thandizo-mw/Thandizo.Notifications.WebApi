using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Thandizo.ApiExtensions.Filters;
using Thandizo.ApiExtensions.General;
using Thandizo.DataModels.Notifications;
using Thandizo.DataModels.Notifications.Requests;
using Thandizo.Notifications.BLL.Services;

namespace Thandizo.Notifications.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BulkNotificationsController : ControllerBase
    {
        private readonly IBulkNotificationService _service;
        private readonly IConfiguration _configuration;

        public string SmsQueueAddress =>
           string.Concat(_configuration["RabbitMQHost"], "/", _configuration["SmsQueue"]);

        public string EmailQueueAddress =>
            string.Concat(_configuration["RabbitMQHost"], "/", _configuration["EmailQueue"]);

        public BulkNotificationsController(IBulkNotificationService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        [HttpGet("GetById")]
        [CatchException(MessageHelper.GetItemError)]
        public async Task<IActionResult> GetById([FromQuery] int notificationId)
        {
            var response = await _service.Get(notificationId);

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
        public async Task<IActionResult> Add([FromBody]BulkNotificationRequest bulkNotificationRequest)
        {
            var outputHandler = await _service.Add(bulkNotificationRequest, SmsQueueAddress);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Created("", outputHandler.Message);
        }

        [HttpPut("Update")]
        [ValidateModelState]
        [CatchException(MessageHelper.UpdateError)]
        public async Task<IActionResult> Update([FromBody]BulkNotificationDTO bulkNotification)
        {
            var outputHandler = await _service.Update(bulkNotification);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Ok(outputHandler.Message);
        }

        [HttpDelete("Delete")]
        [CatchException(MessageHelper.DeleteError)]
        public async Task<IActionResult> Delete([FromQuery]int notificationId)
        {
            var outputHandler = await _service.Delete(notificationId);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Ok(outputHandler.Message);
        }
    }
}
