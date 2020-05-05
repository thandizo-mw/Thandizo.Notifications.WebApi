using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface IBulkNotificationLogService
    {
        Task<OutputResponse> Add(BulkNotificationLogDTO scheduledNotificationLog);
        Task<OutputResponse> Get(long notificationLogId);
        Task<OutputResponse> GetByPhoneNumber(string notificationLogId);
        Task<OutputResponse> GetByNotificationId(long notificationId);
    }
}