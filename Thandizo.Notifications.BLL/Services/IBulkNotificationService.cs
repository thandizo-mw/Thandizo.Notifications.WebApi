using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface IBulkNotificationService
    {
        Task<OutputResponse> Add(BulkNotificationDTO bulkNotification);
        Task<OutputResponse> Delete(int notificationId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int notificationId);
        Task<OutputResponse> Update(BulkNotificationDTO bulkNotification);
    }
}