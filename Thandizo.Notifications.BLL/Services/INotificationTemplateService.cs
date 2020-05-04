using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface INotificationTemplateService
    {
        Task<OutputResponse> Add(NotificationTemplateDTO notificationTemplate);
        Task<OutputResponse> Delete(int templateId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int templateId);
        Task<OutputResponse> Update(NotificationTemplateDTO notificationTemplate);
    }
}