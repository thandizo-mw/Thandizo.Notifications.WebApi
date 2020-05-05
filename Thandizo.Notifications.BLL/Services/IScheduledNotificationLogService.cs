using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface IScheduledNotificationLogService
    {
        Task<OutputResponse> Add(ScheduledNotificationLogDTO scheduledNotificationLog);
        Task<OutputResponse> Get(long notificationLogId);
        Task<OutputResponse> GetByNotificationId(long notificationId);
    }
}