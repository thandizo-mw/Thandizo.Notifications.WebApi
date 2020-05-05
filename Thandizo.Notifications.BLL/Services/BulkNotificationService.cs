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
    public class BulkNotificationService : IBulkNotificationService
    {
        private thandizoContext _context;

        public BulkNotificationService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(int notificationId)
        {
            var bulkNotification = await _context.BulkNotifications.Where(x => x.NotificationId.Equals(notificationId)).
                Select(x => new BulkNotificationResponse
                {
                    NotificationId = x.NotificationId,
                    ChannelId = x.ChannelId,
                    ChanneldName = x.Channel.ChannelName,
                    Message = x.Message,
                    SendDate = x.SendDate,
                    CreatedBy = x.CreatedBy,
                    DateCreated = x.DateCreated,
                    DateModified = x.DateModified,
                    ModifiedBy = x.ModifiedBy
                }).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = bulkNotification
            };
        }

        public async Task<OutputResponse> Get()
        {
            var bulkNotifications = await _context.BulkNotifications.OrderBy(x => x.Channel.ChannelName).
               Select(x => new BulkNotificationResponse
               {
                   NotificationId = x.NotificationId,
                   ChannelId = x.ChannelId,
                   ChanneldName = x.Channel.ChannelName,
                   Message = x.Message,
                   SendDate = x.SendDate,
                   CreatedBy = x.CreatedBy,
                   DateCreated = x.DateCreated,
                   DateModified = x.DateModified,
                   ModifiedBy = x.ModifiedBy
               }).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = bulkNotifications
            };
        }

        public async Task<OutputResponse> Add(BulkNotificationDTO bulkNotification)
        {

            var mappedBulkNotification = new AutoMapperHelper<BulkNotificationDTO, BulkNotifications>().MapToObject(bulkNotification);
            mappedBulkNotification.RowAction = "I";
            mappedBulkNotification.DateCreated = DateTime.UtcNow.AddHours(2);

            await _context.BulkNotifications.AddAsync(mappedBulkNotification);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

        public async Task<OutputResponse> Update(BulkNotificationDTO bulkNotification)
        {
            var bulkNotificationToUpdate = await _context.BulkNotifications.FirstOrDefaultAsync(x => x.NotificationId.Equals(bulkNotification.NotificationId));

            if (bulkNotificationToUpdate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "BulkNotification specified does not exist, update cancelled"
                };
            }

            //update details
            bulkNotificationToUpdate.ChannelId = bulkNotification.ChannelId;
            bulkNotificationToUpdate.Message = bulkNotification.Message;
            bulkNotificationToUpdate.SendDate = bulkNotification.SendDate;
            bulkNotificationToUpdate.RowAction = "U";
            bulkNotificationToUpdate.ModifiedBy = bulkNotification.CreatedBy;
            bulkNotificationToUpdate.DateModified = DateTime.UtcNow.AddHours(2);

            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.UpdateSuccess
            };
        }

        public async Task<OutputResponse> Delete(int notificationId)
        {

            var bulkNotification = await _context.BulkNotifications.FirstOrDefaultAsync(x => x.NotificationId.Equals(notificationId));

            if (bulkNotification == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Bulk notification specified does not exist, deletion cancelled"
                };
            }

            //deletes the record permanently
            _context.BulkNotifications.Remove(bulkNotification);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.DeleteSuccess
            };
        }
    }
}
