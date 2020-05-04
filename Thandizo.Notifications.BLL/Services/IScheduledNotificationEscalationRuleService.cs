using System.Threading.Tasks;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public interface IScheduledNotificationEscalationRuleService
    {
        Task<OutputResponse> Add(ScheduledNotificationEscalationRuleDTO notificationEscalationRule);
        Task<OutputResponse> Delete(int ruleId);
        Task<OutputResponse> Get();
        Task<OutputResponse> Get(int ruleId);
        Task<OutputResponse> Update(ScheduledNotificationEscalationRuleDTO notificationEscalationRule);
    }
}