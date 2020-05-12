using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface ISubscriberService
    {
        Task<OutputResponse> Add(SubscriberDTO subscriber);
        Task<OutputResponse> Delete(int suscriberId);
        Task<OutputResponse> GetByChannel(int channelId);
        Task<OutputResponse> GetBySubscriber(string phoneNumber);
        Task<OutputResponse> Get(int suscriberId);
        Task<OutputResponse> Update(SubscriberDTO subscriber);
    }
}