using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;
using Thandizo.DataModels.Notifications.Requests;

namespace Thandizo.Notifications.BLL.Services
{
    public interface IBulkNotificationService
    {
        Task<OutputResponse> Add(BulkNotificationRequest bulkNotificationRequest, string smsQueueAddress="", string emailQueueAddress = "");
        Task<OutputResponse> Delete(int notificationId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int notificationId);
        Task<OutputResponse> Update(BulkNotificationDTO bulkNotification);
    }
}