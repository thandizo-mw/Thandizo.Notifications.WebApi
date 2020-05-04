using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface INotificationChannelService
    {
        Task<OutputResponse> Add(NotificationChannelDTO notificationChannel);
        Task<OutputResponse> Delete(int channelId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int channelId);
        Task<OutputResponse> Update(NotificationChannelDTO notificationChannel);
    }
}