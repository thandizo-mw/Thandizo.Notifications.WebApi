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
    public class ScheduledNotificationEscalationRuleService : IScheduledNotificationEscalationRuleService
    {
        private thandizoContext _context;

        public ScheduledNotificationEscalationRuleService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(int ruleId)
        {
            var notificationEscalationRule = await _context.ScheduledNotificationEscalationRules.FirstOrDefaultAsync(x => x.RuleId.Equals(ruleId));

            var mappedScheduledNotificationEscalationRule = new AutoMapperHelper<ScheduledNotificationEscalationRules, ScheduledNotificationEscalationRuleDTO>().MapToObject(notificationEscalationRule);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedScheduledNotificationEscalationRule
            };
        }

        public async Task<OutputResponse> Get()
        {
            var notificationEscalationRules = await _context.ScheduledNotificationEscalationRules.OrderBy(x => x.Name).ToListAsync();

            var mappedScheduledNotificationEscalationRules = new AutoMapperHelper<ScheduledNotificationEscalationRules, ScheduledNotificationEscalationRuleDTO>().MapToList(notificationEscalationRules);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedScheduledNotificationEscalationRules
            };
        }

        public async Task<OutputResponse> Add(ScheduledNotificationEscalationRuleDTO notificationEscalationRule)
        {
            var isFound = await _context.ScheduledNotificationEscalationRules.AnyAsync(x => x.Name.ToLower() == notificationEscalationRule.Name.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification escalation rule name already exist, duplicates not allowed"
                };
            }

            var mappedScheduledNotificationEscalationRule = new AutoMapperHelper<ScheduledNotificationEscalationRuleDTO, ScheduledNotificationEscalationRules>().MapToObject(notificationEscalationRule);
            mappedScheduledNotificationEscalationRule.RowAction = "I";
            mappedScheduledNotificationEscalationRule.DateCreated = DateTime.UtcNow.AddHours(2);

            await _context.ScheduledNotificationEscalationRules.AddAsync(mappedScheduledNotificationEscalationRule);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

        public async Task<OutputResponse> Update(ScheduledNotificationEscalationRuleDTO notificationEscalationRule)
        {
            var notificationEscalationRuleToUpdate = await _context.ScheduledNotificationEscalationRules.FirstOrDefaultAsync(x => x.RuleId.Equals(notificationEscalationRule.RuleId));

            if (notificationEscalationRuleToUpdate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification escalation rule specified does not exist, update cancelled"
                };
            }

            var isFound = await _context.ScheduledNotificationEscalationRules.Where(x => x.RuleId != notificationEscalationRule.RuleId).AnyAsync(x => x.Name.ToLower() == notificationEscalationRule.Name.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification escalation rule name already exist, duplicates not allowed"
                };
            }

            //update details
            notificationEscalationRuleToUpdate.Name = notificationEscalationRule.Name;
            notificationEscalationRuleToUpdate.EscalateTo = notificationEscalationRule.EscalateTo;
            notificationEscalationRuleToUpdate.Message = notificationEscalationRule.Message;
            notificationEscalationRuleToUpdate.RowAction = "U";
            notificationEscalationRuleToUpdate.ModifiedBy = notificationEscalationRule.CreatedBy;
            notificationEscalationRuleToUpdate.DateModified = DateTime.UtcNow.AddHours(2);

            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.UpdateSuccess
            };
        }

        public async Task<OutputResponse> Delete(int ruleId)
        {
            //check if there are any records associated with the specified notification templates allocation
            var isFound = await _context.ScheduledNotifications.AnyAsync(x => x.RuleId.Equals(ruleId));
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "The specified notification template is allocated toa scheduled notification, deletion denied"
                };
            }

            var notificationEscalationRule = await _context.ScheduledNotificationEscalationRules.FirstOrDefaultAsync(x => x.RuleId.Equals(ruleId));

            if (notificationEscalationRule == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Notification escalation rule specified does not exist, deletion cancelled"
                };
            }

            //deletes the record permanently
            _context.ScheduledNotificationEscalationRules.Remove(notificationEscalationRule);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.DeleteSuccess
            };
        }
    }
}
