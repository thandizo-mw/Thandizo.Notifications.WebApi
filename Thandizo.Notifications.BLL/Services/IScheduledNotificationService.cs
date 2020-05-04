using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface IScheduledNotificationService
    {
        Task<OutputResponse> Add(ScheduledNotificationDTO scheduledNotification);
        Task<OutputResponse> Delete(int notificationId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int notificationId);
        Task<OutputResponse> Update(ScheduledNotificationDTO scheduledNotification);
    }
}