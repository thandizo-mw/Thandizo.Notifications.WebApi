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

        public BulkNotificationService(thandizoContext context, IBusControl bus)
        {
            _context = context;
            _bus = bus;
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

        public async Task<OutputResponse> Add(BulkNotificationRequest bulkNotificationRequest, string smsQueueAddress, string emailQueueAddress)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var allRecipients = new Dictionary<string, string>();
                if (bulkNotificationRequest.MessageType.Equals("D"))
                {
                    if (bulkNotificationRequest.ToChannels)
                    {
                        var subscribers = _context.Subscribers;
                        foreach (var subscriber in subscribers)
                        {
                            allRecipients.Add(subscriber.RecipientAddress, subscriber.Channel.ChannelName.ToUpper().Trim());
                        }
                    }
                    if (bulkNotificationRequest.ToHealthCareWorkers)
                    {
                        var healthCareWorkers = _context.HealthCareWorkers.Select(x => new { x.PhoneNumber, x.EmailAddress });
                        foreach (var healthCareWorker in healthCareWorkers)
                        {
                            if ((!string.IsNullOrEmpty(healthCareWorker.PhoneNumber)) && (!allRecipients.ContainsKey(healthCareWorker.PhoneNumber)))
                            {
                                allRecipients.Add(healthCareWorker.PhoneNumber, "SMS");
                            }

                            if ((!string.IsNullOrEmpty(healthCareWorker.EmailAddress)) && (!allRecipients.ContainsKey(healthCareWorker.EmailAddress)))
                            {
                                allRecipients.Add(healthCareWorker.EmailAddress, "EMAIL");
                            }

                        }
                    }
                    if (bulkNotificationRequest.ToPatients)
                    {
                        var patients = _context.Patients.Select(x => new { x.PhoneNumber, x.EmailAddress });
                        foreach (var patient in patients)
                        {
                            if ((!string.IsNullOrEmpty(patient.PhoneNumber)) && (!allRecipients.ContainsKey(patient.PhoneNumber)))
                            {
                                allRecipients.Add(patient.PhoneNumber, "SMS");
                            }

                            if ((!string.IsNullOrEmpty(patient.EmailAddress)) && (!allRecipients.ContainsKey(patient.EmailAddress)))
                            {
                                allRecipients.Add(patient.EmailAddress, "EMAIL");
                            }

                        }
                    }
                    if (bulkNotificationRequest.ToTeamMembers)
                    {
                        var teamMembers = _context.ResponseTeamMembers.Select(x => new { x.PhoneNumber, x.EmailAddress });
                        foreach (var teamMember in teamMembers)
                        {
                            if ((!string.IsNullOrEmpty(teamMember.PhoneNumber)) && (!allRecipients.ContainsKey(teamMember.PhoneNumber)))
                            {
                                allRecipients.Add(teamMember.PhoneNumber, "SMS");
                            }

                            if ((!string.IsNullOrEmpty(teamMember.EmailAddress)) && (!allRecipients.ContainsKey(teamMember.EmailAddress)))
                            {
                                allRecipients.Add(teamMember.EmailAddress, "EMAIL");
                            }

                        }
                    }
                    if (bulkNotificationRequest.HasFileUpload)
                    {

                    }
                }
                else
                {
                    var subscribers = _context.Subscribers;
                    foreach (var subscriber in subscribers)
                    {
                        allRecipients.Add(subscriber.RecipientAddress, subscriber.Channel.ChannelName.ToUpper().Trim());
                    }
                }

                var smsRecipients = new List<string>();
                var emailRecipients = new List<string>();

                foreach (var recipient in allRecipients)
                {
                    switch (recipient.Value)
                    {
                        case "SMS":
                            smsRecipients.Add(recipient.Key);
                            break;
                        case "EMAIL":
                            emailRecipients.Add(recipient.Key);
                            break;
                        default:
                            break;
                    }
                }

                if (bulkNotificationRequest.SendNow)
                {
                    bulkNotificationRequest.SendDate = DateTime.UtcNow;
                    var smsEndpoint = await _bus.GetSendEndpoint(new Uri(smsQueueAddress));
                    var emailEndpoint = await _bus.GetSendEndpoint(new Uri(emailQueueAddress));

                    BulkNotificationDTO bulkSmsNotification = new BulkNotificationDTO();
                    if (smsRecipients.Any())
                    {
                        await smsEndpoint.Send(new MessageModelRequest(new MessageModel
                        {
                            SourceAddress = "Thandizo",
                            DestinationRecipients = smsRecipients,
                            MessageBody = bulkNotificationRequest.Message
                        }));;

                        bulkSmsNotification = bulkNotificationRequest;

                        var channelId = await _context.NotificationChannels.Where(x => x.ChannelName.ToUpper().Equals("SMS")).Select(x => x.ChannelId).FirstOrDefaultAsync();
                        
                        var mappedSmsBulkNotification = new AutoMapperHelper<BulkNotificationDTO, BulkNotifications>().MapToObject(bulkSmsNotification);
                        mappedSmsBulkNotification.RowAction = "I";
                        mappedSmsBulkNotification.ChannelId = channelId;
                        mappedSmsBulkNotification.DateCreated = DateTime.UtcNow;

                        await _context.BulkNotifications.AddAsync(mappedSmsBulkNotification);
                        await _context.SaveChangesAsync();

                        var bulkSmsNotificationLogs = new List<BulkNotificationLog>();

                        foreach (var recipient in allRecipients)
                        {
                            var bulkNotificationLog = new BulkNotificationLogDTO
                            {
                                CreatedBy = "SYS",
                                DateCreated = DateTime.UtcNow,
                                NotificationId = mappedSmsBulkNotification.NotificationId,
                                PhoneNumber = recipient.Value,
                                Status = "S"
                            };

                            var mappedBulkNotificationLog = new AutoMapperHelper<BulkNotificationLogDTO, BulkNotificationLog>().MapToObject(bulkNotificationLog);
                            mappedBulkNotificationLog.DateCreated = DateTime.UtcNow;

                            bulkSmsNotificationLogs.Add(mappedBulkNotificationLog);
                        }
                        await _context.BulkNotificationLog.AddRangeAsync(bulkSmsNotificationLogs);
                        await _context.SaveChangesAsync();
                    }

                    if (emailRecipients.Any())
                    {
                        await emailEndpoint.Send(new MessageModelRequest(new MessageModel
                        {
                            SourceAddress = "thandizo@angledimension.com",
                            Subject = "COVID-19 Notification",
                            DestinationRecipients = emailRecipients,
                            MessageBody = $"Dear Sir/Madam,<br />{bulkNotificationRequest.Message}"
                        }));

                        BulkNotificationDTO bulkEmailNotification = bulkNotificationRequest;

                        var channelId = await _context.NotificationChannels.Where(x => x.ChannelName.ToUpper().Equals("EMAIL")).Select(x => x.ChannelId).FirstOrDefaultAsync();

                        var mappedEmailBulkNotification = new AutoMapperHelper<BulkNotificationDTO, BulkNotifications>().MapToObject(bulkEmailNotification);
                        mappedEmailBulkNotification.RowAction = "I";
                        mappedEmailBulkNotification.ChannelId = channelId;
                        mappedEmailBulkNotification.DateCreated = DateTime.UtcNow;

                        await _context.BulkNotifications.AddAsync(mappedEmailBulkNotification);
                        await _context.SaveChangesAsync();

                        var bulkSmsNotificationLogs = new List<BulkNotificationLog>();

                        foreach (var recipient in allRecipients)
                        {
                            var bulkNotificationLog = new BulkNotificationLogDTO
                            {
                                CreatedBy = "SYS",
                                DateCreated = DateTime.UtcNow,
                                NotificationId = mappedEmailBulkNotification.NotificationId,
                                PhoneNumber = recipient.Value,
                                Status = "S"
                            };

                            var mappedBulkNotificationLog = new AutoMapperHelper<BulkNotificationLogDTO, BulkNotificationLog>().MapToObject(bulkNotificationLog);
                            mappedBulkNotificationLog.DateCreated = DateTime.UtcNow;

                            bulkSmsNotificationLogs.Add(mappedBulkNotificationLog);
                        }
                        await _context.BulkNotificationLog.AddRangeAsync(bulkSmsNotificationLogs);
                        await _context.SaveChangesAsync();
                    }

                }

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
            bulkNotificationToUpdate.DateModified = DateTime.UtcNow;

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
