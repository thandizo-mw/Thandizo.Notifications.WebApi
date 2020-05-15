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
    public class BulkNotificationLogService : IBulkNotificationLogService
    {
        private thandizoContext _context;

        public BulkNotificationLogService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(long notificationLogId)
        {
            var scheduledNotificationLog = await _context.BulkNotificationLog.FirstOrDefaultAsync(x => x.NotificationLogId.Equals(notificationLogId));

            var mappedBulkNotificationLog = new AutoMapperHelper<BulkNotificationLog, BulkNotificationLogDTO>().MapToObject(scheduledNotificationLog);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotificationLog
            };
        }

        public async Task<OutputResponse> GetByNotificationId(long notificationId)
        {
            var scheduledNotificationLogs = await _context.BulkNotificationLog.Where(x => x.NotificationId.Equals(notificationId)).OrderBy(x => x.NotificationLogId).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotificationLogs
            };
        }
        
        public async Task<OutputResponse> GetByPhoneNumber(string phoneNumber)
        {
            var scheduledNotificationLogs = await _context.BulkNotificationLog.Where(x => x.PhoneNumber.Equals(phoneNumber)).OrderBy(x => x.NotificationLogId).ToListAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = scheduledNotificationLogs
            };
        }

        public async Task<OutputResponse> Add(BulkNotificationLogDTO scheduledNotificationLog)
        {

            var mappedBulkNotificationLog = new AutoMapperHelper<BulkNotificationLogDTO, BulkNotificationLog>().MapToObject(scheduledNotificationLog);

            mappedBulkNotificationLog.DateCreated = DateTime.UtcNow;

            await _context.BulkNotificationLog.AddAsync(mappedBulkNotificationLog);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

    }
}
