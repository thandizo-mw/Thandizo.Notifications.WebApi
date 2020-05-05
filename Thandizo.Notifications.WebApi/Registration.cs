using Microsoft.Extensions.DependencyInjection;
using Thandizo.Notifications.BLL.Services;

namespace Thandizo.Notifications.WebApi
{
    public static class Registrations
    {
        /// <summary>
        /// Registers domain services to the specified
        /// service descriptor
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<INotificationChannelService, NotificationChannelService>();
            services.AddScoped<IBulkNotificationService, BulkNotificationService>();
            services.AddScoped<IScheduledNotificationService, ScheduledNotificationService>();
            services.AddScoped<ISubscriberService, SubscriberService>();
            services.AddScoped<IScheduledNotificationEscalationRuleService, ScheduledNotificationEscalationRuleService>();
            return services.AddScoped<INotificationTemplateService, NotificationTemplateService>();
        }
    }
}
