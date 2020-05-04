using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thandizo.ApiExtensions.DataMapping;
using Thandizo.ApiExtensions.General;
using Thandizo.DAL.Models;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Notifications;

namespace Thandizo.Notifications.BLL.Services
{
    public class NotificationTemplateService : INotificationTemplateService
    {
        private thandizoContext _context;

        public NotificationTemplateService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(int templateId)
        {
            var notificationTemplate = await _context.NotificationTemplates.FirstOrDefaultAsync(x => x.TemplateId.Equals(templateId));

            var mappedNotificationTemplate = new AutoMapperHelper<NotificationTemplates, NotificationTemplateDTO>().MapToObject(notificationTemplate);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedNotificationTemplate
            };
        }

        public async Task<OutputResponse> Get()
        {
            var notificationTemplates = await _context.NotificationTemplates.OrderBy(x => x.TemplateName).ToListAsync();

            var mappedNotificationTemplates = new AutoMapperHelper<NotificationTemplates, NotificationTemplateDTO>().MapToList(notificationTemplates);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedNotificationTemplates
            };
        }

        public async Task<OutputResponse> Add(NotificationTemplateDTO notificationTemplate)
        {
            var isFound = await _context.NotificationTemplates.AnyAsync(x => x.TemplateName.ToLower() == notificationTemplate.TemplateName.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification template name already exist, duplicates not allowed"
                };
            }

            var mappedNotificationTemplate = new AutoMapperHelper<NotificationTemplateDTO, NotificationTemplates>().MapToObject(notificationTemplate);
            mappedNotificationTemplate.RowAction = "I";
            mappedNotificationTemplate.DateCreated = DateTime.UtcNow.AddHours(2);

            await _context.NotificationTemplates.AddAsync(mappedNotificationTemplate);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

        public async Task<OutputResponse> Update(NotificationTemplateDTO notificationTemplate)
        {
            var notificationTemplateToUpdate = await _context.NotificationTemplates.FirstOrDefaultAsync(x => x.TemplateId.Equals(notificationTemplate.TemplateId));

            if (notificationTemplateToUpdate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification template specified does not exist, update cancelled"
                };
            }

            var isFound = await _context.NotificationTemplates.Where(x => x.TemplateId != notificationTemplate.TemplateId).AnyAsync(x => x.TemplateName.ToLower() == notificationTemplate.TemplateName.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification template name already exist, duplicates not allowed"
                };
            }

            //update details
            notificationTemplateToUpdate.TemplateName = notificationTemplate.TemplateName;
            notificationTemplateToUpdate.Interval = notificationTemplate.Interval;
            notificationTemplateToUpdate.IntervalUnit = notificationTemplate.IntervalUnit;
            notificationTemplateToUpdate.RepeatCount = notificationTemplate.RepeatCount;
            notificationTemplateToUpdate.RowAction = "U";
            notificationTemplateToUpdate.ModifiedBy = notificationTemplate.CreatedBy;
            notificationTemplateToUpdate.DateModified = DateTime.UtcNow.AddHours(2);

            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.UpdateSuccess
            };
        }

        public async Task<OutputResponse> Delete(int templateId)
        {
            //check if there are any records associated with the specified notification templates allocation
            var isFound = await _context.ScheduledNotifications.AnyAsync(x => x.TemplateId.Equals(templateId));
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "The specified notification template is allocated toa scheduled notification, deletion denied"
                };
            }

            var notificationTemplate = await _context.NotificationTemplates.FirstOrDefaultAsync(x => x.TemplateId.Equals(templateId));

            if (notificationTemplate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification template specified does not exist, deletion cancelled"
                };
            }

            //deletes the record permanently
            _context.NotificationTemplates.Remove(notificationTemplate);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.DeleteSuccess
            };
        }
    }
}
