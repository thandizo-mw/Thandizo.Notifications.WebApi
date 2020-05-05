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
    public class ScheduledNotificationLogService : IScheduledNotificationLogService
    {
        private thandizoContext _context;

        public ScheduledNotificationLogService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(long notificationLogId)
        {
            var scheduledNotificationLog = await _context.ScheduledNotificationLog.FirstOrDefaultAsync(x => x.NotificationLogId.Equals(notificationLogId));

            var mappedScheduledNotificationLog = new AutoMapperHelper<ScheduledNotificationLog, ScheduledNotificationLogDTO>().MapToObject(scheduledNotificationLog);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotificationLog
            };
        }

        public async Task<OutputResponse> GetByNotificationId(long notificationId)
        {
            var scheduledNotificationLogs = await _context.ScheduledNotificationLog.Where(x => x.NotificationId.Equals(notificationId)).OrderBy(x => x.NotificationLogId).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotificationLogs
            };
        }

        public async Task<OutputResponse> Add(ScheduledNotificationLogDTO scheduledNotificationLog)
        {

            var mappedScheduledNotificationLog = new AutoMapperHelper<ScheduledNotificationLogDTO, ScheduledNotificationLog>().MapToObject(scheduledNotificationLog);

            mappedScheduledNotificationLog.DateCreated = DateTime.UtcNow.AddHours(2);

            await _context.ScheduledNotificationLog.AddAsync(mappedScheduledNotificationLog);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

    }
}
