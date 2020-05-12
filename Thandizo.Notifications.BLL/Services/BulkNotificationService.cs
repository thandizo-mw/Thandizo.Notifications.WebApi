using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Thandizo.ApiExtensions.DataMapping;
using Thandizo.ApiExtensions.General;
using Thandizo.DAL.Models;
using Thandizo.DataModels.Contracts;
using Thandizo.DataModels.General;
using Thandizo.DataModels.Messaging;
using Thandizo.DataModels.Notifications;
using Thandizo.DataModels.Notifications.Requests;
using Thandizo.DataModels.Notifications.Responses;

namespace Thandizo.Notifications.BLL.Services
{
    public class BulkNotificationService : IBulkNotificationService
    {
        private readonly thandizoContext _context;
        private readonly IBusControl _bus;

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
                }).FirstOrDefaultAsync();

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

        public async Task<OutputResponse> Add(BulkNotificationRequest bulkNotificationRequest, string smsQueueAddress)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var channel = await _context.NotificationChannels.FirstOrDefaultAsync(x => x.ChannelId.Equals(bulkNotificationRequest.ChannelId));

                var phoneNumbers = await _context.Subscribers.Where(x => x.ChannelId.Equals(bulkNotificationRequest.ChannelId)).
                        Select(x => x.PhoneNumber).ToListAsync();

                if (bulkNotificationRequest.SendNow && channel.ChannelName.Equals("SMS"))
                {
                    var smsEndpoint = await _bus.GetSendEndpoint(new Uri(smsQueueAddress));

                    if (phoneNumbers.Any())
                    {
                        await smsEndpoint.Send(new MessageModelRequest(new MessageModel
                        {
                            SourceAddress = "Thandizo",
                            DestinationRecipients = phoneNumbers,
                            MessageBody = bulkNotificationRequest.Message
                        }));
                    }
                }

                BulkNotificationDTO bulkNotification = bulkNotificationRequest;

                var mappedBulkNotification = new AutoMapperHelper<BulkNotificationDTO, BulkNotifications>().MapToObject(bulkNotification);
                mappedBulkNotification.RowAction = "I";
                mappedBulkNotification.DateCreated = DateTime.UtcNow.AddHours(2);

                await _context.BulkNotifications.AddAsync(mappedBulkNotification);
                await _context.SaveChangesAsync();

                var bulkNotificationLogs = new List<BulkNotificationLog>();

                foreach (string phoneNumber in phoneNumbers)
                {
                    var bulkNotificationLog = new BulkNotificationLogDTO
                    {
                        CreatedBy = "SYS",
                        DateCreated = DateTime.UtcNow.AddHours(2),
                        NotificationId = mappedBulkNotification.NotificationId,
                        PhoneNumber = phoneNumber,
                        Status = "S"
                    };

                    var mappedBulkNotificationLog = new AutoMapperHelper<BulkNotificationLogDTO, BulkNotificationLog>().MapToObject(bulkNotificationLog);
                    mappedBulkNotificationLog.DateCreated = DateTime.UtcNow.AddHours(2);

                    bulkNotificationLogs.Add(mappedBulkNotificationLog);
                }
                await _context.BulkNotificationLog.AddRangeAsync(bulkNotificationLogs);
                await _context.SaveChangesAsync();

                scope.Complete();
            }

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
