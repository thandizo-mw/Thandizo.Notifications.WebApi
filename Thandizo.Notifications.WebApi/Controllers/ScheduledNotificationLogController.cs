using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Thandizo.ApiExtensions.Filters;
using Thandizo.ApiExtensions.General;
using Thandizo.DataModels.Notifications;
using Thandizo.Notifications.BLL.Services;

namespace Thandizo.NotificationLog.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduledNotificationLogController : ControllerBase
    {
        IScheduledNotificationLogService _service;

        public ScheduledNotificationLogController(IScheduledNotificationLogService service)
        {
            _service = service;
        }

        [HttpGet("GetById")]
        [CatchException(MessageHelper.GetItemError)]
        public async Task<IActionResult> GetById([FromQuery] long notificationLogId)
        {
            var response = await _service.Get(notificationLogId);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);
        }

        [HttpGet("GetByNotificationId")]
        [CatchException(MessageHelper.GetListError)]
        public async Task<IActionResult> GetByNotificationId(long notificationId)
        {
            var response = await _service.GetByNotificationId(notificationId);

            if (response.IsErrorOccured)
            {
                return BadRequest(response.Message);
            }

            return Ok(response.Result);
        }

        [HttpPost("Add")]
        [ValidateModelState]
        [CatchException(MessageHelper.AddNewError)]
        public async Task<IActionResult> Add([FromBody]ScheduledNotificationLogDTO scheduledNotificationLog)
        {
            var outputHandler = await _service.Add(scheduledNotificationLog);
            if (outputHandler.IsErrorOccured)
            {
                return BadRequest(outputHandler.Message);
            }

            return Created("", outputHandler.Message);
        }
    }
}
