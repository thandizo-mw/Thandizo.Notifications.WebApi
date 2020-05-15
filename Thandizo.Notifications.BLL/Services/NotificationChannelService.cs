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
    public class NotificationChannelService : INotificationChannelService
    {
        private thandizoContext _context;

        public NotificationChannelService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(int channelId)
        {
            var notificationChannel = await _context.NotificationChannels.FirstOrDefaultAsync(x => x.ChannelId.Equals(channelId));

            var mappedNotificationChannel = new AutoMapperHelper<NotificationChannels, NotificationChannelDTO>().MapToObject(notificationChannel);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedNotificationChannel
            };
        }

        public async Task<OutputResponse> Get()
        {
            var notificationChannels = await _context.NotificationChannels.OrderBy(x => x.ChannelName).ToListAsync();

            var mappedNotificationChannels = new AutoMapperHelper<NotificationChannels, NotificationChannelDTO>().MapToList(notificationChannels);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedNotificationChannels
            };
        }

        public async Task<OutputResponse> Add(NotificationChannelDTO notificationChannel)
        {
            var isFound = await _context.NotificationChannels.AnyAsync(x => x.ChannelName.ToLower() == notificationChannel.ChannelName.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification channel name already exist, duplicates not allowed"
                };
            }

            var mappedNotificationChannel = new AutoMapperHelper<NotificationChannelDTO, NotificationChannels>().MapToObject(notificationChannel);
            mappedNotificationChannel.RowAction = "I";
            mappedNotificationChannel.DateCreated = DateTime.UtcNow;

            await _context.NotificationChannels.AddAsync(mappedNotificationChannel);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

        public async Task<OutputResponse> Update(NotificationChannelDTO notificationChannel)
        {
            var notificationChannelToUpdate = await _context.NotificationChannels.FirstOrDefaultAsync(x => x.ChannelId.Equals(notificationChannel.ChannelId));

            if (notificationChannelToUpdate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification channel specified does not exist, update cancelled"
                };
            }

            var isFound = await _context.NotificationChannels.Where(x => x.ChannelId != notificationChannel.ChannelId).AnyAsync(x => x.ChannelName.ToLower() == notificationChannel.ChannelName.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification channel name already exist, duplicates not allowed"
                };
            }

            //update details
            notificationChannelToUpdate.ChannelName = notificationChannel.ChannelName;
            notificationChannelToUpdate.RowAction = "U";
            notificationChannelToUpdate.ModifiedBy = notificationChannel.CreatedBy;
            notificationChannelToUpdate.DateModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.UpdateSuccess
            };
        }

        public async Task<OutputResponse> Delete(int channelId)
        {
            //check if there are any records associated with the specified notification channels allocation
            var isFound = await _context.ScheduledNotifications.AnyAsync(x => x.ChannelId.Equals(channelId));
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "The specified notification channel is allocated toa scheduled notification, deletion denied"
                };
            }

            isFound = await _context.Subscribers.AnyAsync(x => x.ChannelId.Equals(channelId));
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "The specified notification channel is allocated to a subscriber, deletion denied"
                };
            }

            var notificationChannel = await _context.NotificationChannels.FirstOrDefaultAsync(x => x.ChannelId.Equals(channelId));

            if (notificationChannel == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification channel specified does not exist, deletion cancelled"
                };
            }

            //deletes the record permanently
            _context.NotificationChannels.Remove(notificationChannel);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.DeleteSuccess
            };
        }
    }
}
