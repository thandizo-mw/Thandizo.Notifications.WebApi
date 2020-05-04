using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface ISubscriberService
    {
        Task<OutputResponse> Add(SubscriberDTO subscriber);
        Task<OutputResponse> Delete(int suscriberId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int suscriberId);
        Task<OutputResponse> Update(SubscriberDTO subscriber);
    }
}