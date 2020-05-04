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
using Thandizo.DataModels.Notifications.Responses;

namespace Thandizo.Notifications.BLL.Services
{
    public class ScheduledNotificationService : IScheduledNotificationService
    {
        private thandizoContext _context;

        public ScheduledNotificationService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(int notificationId)
        {
            var scheduledNotification = await _context.ScheduledNotifications.Where(x => x.NotificationId.Equals(notificationId)).
                Select(x => new ScheduledNotificationResponse
                {
                    ChannelId = x.ChannelId,
                    ChanneldName = x.Channel.ChannelName,
                    Message = x.Message,
                    StartDate = x.StartDate,
                    TemplateId = x.TemplateId,
                    TemplateName = x.Template.TemplateName,
                    Interval = x.Interval,
                    IsActive = x.IsActive,
                    NotificationId = x.NotificationId,
                    PatientId = x.PatientId,
                    PatientName = $"{x.Patient.FirstName} {x.Patient.OtherNames} {x.Patient.LastName}",
                    RuleId = x.RuleId,
                    RuleName = x.Rule.Name,
                    CreatedBy = x.CreatedBy,
                    DateCreated = x.DateCreated,
                    DateModified = x.DateModified,
                    ModifiedBy = x.ModifiedBy
                }).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotification
            };
        }

        public async Task<OutputResponse> Get()
        {
            var scheduledNotifications = await _context.ScheduledNotifications.OrderBy(x => x.Channel.ChannelName).
               Select(x => new ScheduledNotificationResponse
               {
                   ChannelId = x.ChannelId,
                   ChanneldName = x.Channel.ChannelName,
                   Message = x.Message,
                   StartDate = x.StartDate,
                   TemplateId = x.TemplateId,
                   TemplateName = x.Template.TemplateName,
                   Interval = x.Interval,
                   IsActive = x.IsActive,
                   NotificationId = x.NotificationId,
                   PatientId = x.PatientId,
                   PatientName = $"{x.Patient.FirstName} {x.Patient.OtherNames} {x.Patient.LastName}",
                   RuleId = x.RuleId,
                   RuleName = x.Rule.Name,
                   CreatedBy = x.CreatedBy,
                   DateCreated = x.DateCreated,
                   DateModified = x.DateModified,
                   ModifiedBy = x.ModifiedBy
               }).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotifications
            };
        }

        public async Task<OutputResponse> Add(ScheduledNotificationDTO scheduledNotification)
        {

            var mappedScheduledNotification = new AutoMapperHelper<ScheduledNotificationDTO, ScheduledNotifications>().MapToObject(scheduledNotification);
            mappedScheduledNotification.RowAction = "I";
            mappedScheduledNotification.DateCreated = DateTime.UtcNow.AddHours(2);

            await _context.ScheduledNotifications.AddAsync(mappedScheduledNotification);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

        public async Task<OutputResponse> Update(ScheduledNotificationDTO scheduledNotification)
        {
            var scheduledNotificationToUpdate = await _context.ScheduledNotifications.FirstOrDefaultAsync(x => x.NotificationId.Equals(scheduledNotification.NotificationId));

            if (scheduledNotificationToUpdate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "ScheduledNotification specified does not exist, update cancelled"
                };
            }

            //update details
            scheduledNotificationToUpdate.ChannelId = scheduledNotification.ChannelId;
            scheduledNotificationToUpdate.Interval = scheduledNotification.Interval;
            scheduledNotificationToUpdate.IsActive = scheduledNotification.IsActive;
            scheduledNotificationToUpdate.Message = scheduledNotification.Message;
            scheduledNotificationToUpdate.PatientId = scheduledNotification.PatientId;
            scheduledNotificationToUpdate.TemplateId = scheduledNotification.TemplateId;
            scheduledNotificationToUpdate.RuleId = scheduledNotification.RuleId;
            scheduledNotificationToUpdate.StartDate = scheduledNotification.StartDate;
            scheduledNotificationToUpdate.RowAction = "U";
            scheduledNotificationToUpdate.ModifiedBy = scheduledNotification.CreatedBy;
            scheduledNotificationToUpdate.DateModified = DateTime.UtcNow.AddHours(2);

            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.UpdateSuccess
            };
        }

        public async Task<OutputResponse> Delete(int notificationId)
        {

            var scheduledNotification = await _context.ScheduledNotifications.FirstOrDefaultAsync(x => x.NotificationId.Equals(notificationId));

            if (scheduledNotification == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Scheduled notification specified does not exist, deletion cancelled"
                };
            }

            //deletes the record permanently
            _context.ScheduledNotifications.Remove(scheduledNotification);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.DeleteSuccess
            };
        }
    }
}
