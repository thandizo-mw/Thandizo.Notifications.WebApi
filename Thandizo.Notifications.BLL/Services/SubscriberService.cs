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
    public class SubscriberService : ISubscriberService
    {
        private thandizoContext _context;

        public SubscriberService(thandizoContext context)
        {
            _context = context;
        }

        public async Task<OutputResponse> Get(int subscriberId)
        {
            var subscriber = await _context.Subscribers.FirstOrDefaultAsync(x => x.SubscriberId.Equals(subscriberId));

            var mappedSubscriber = new AutoMapperHelper<Subscribers, SubscriberDTO>().MapToObject(subscriber);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedSubscriber
            };
        }

        public async Task<OutputResponse> GetByChannel(int channelId)
        {
            var subscribers = await _context.Subscribers.Where(x => x.ChannelId.Equals(channelId)).OrderBy(x => x.PhoneNumber).ToListAsync();

            var mappedSubscribers = new AutoMapperHelper<Subscribers, SubscriberDTO>().MapToList(subscribers);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedSubscribers
            };
        }

        public async Task<OutputResponse> GetBySubscriber(string phoneNumber)
        {
            var subscribers = await _context.Subscribers.Where(x => x.PhoneNumber.Equals(phoneNumber)).ToListAsync();

            var mappedSubscribers = new AutoMapperHelper<Subscribers, SubscriberDTO>().MapToList(subscribers);

            return new OutputResponse
            {
                IsErrorOccured = false,
                Result = mappedSubscribers
            };
        }

        public async Task<OutputResponse> Add(SubscriberDTO subscriber)
        {
            /*var isFound = await _context.Subscribers.AnyAsync(x => x.PhoneNumber.ToLower() == subscriber.PhoneNumber.ToLower());
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Subscriber phone number already exist, duplicates not allowed"
                };
            }*/

            var mappedSubscriber = new AutoMapperHelper<SubscriberDTO, Subscribers>().MapToObject(subscriber);
            mappedSubscriber.RowAction = "I";
            mappedSubscriber.DateCreated = DateTime.UtcNow.AddHours(2);

            await _context.Subscribers.AddAsync(mappedSubscriber);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.AddNewSuccess
            };
        }

        public async Task<OutputResponse> Update(SubscriberDTO subscriber)
        {
            var subscriberToUpdate = await _context.Subscribers.FirstOrDefaultAsync(x => x.SubscriberId.Equals(subscriber.SubscriberId));

            if (subscriberToUpdate == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Subscriber specified does not exist, update cancelled"
                };
            }

            var isFound = await _context.Subscribers.Where(x => x.SubscriberId != subscriber.SubscriberId).AnyAsync(x => x.PhoneNumber == subscriber.PhoneNumber);
            if (isFound)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Subscriber phone number already exist, duplicates not allowed"
                };
            }

            //update details
            subscriberToUpdate.PhoneNumber = subscriber.PhoneNumber;
            subscriberToUpdate.IsRegisteredPatient = subscriber.IsRegisteredPatient;
            subscriberToUpdate.ChannelId = subscriber.ChannelId;
            subscriberToUpdate.RowAction = "U";
            subscriberToUpdate.ModifiedBy = subscriber.CreatedBy;
            subscriberToUpdate.DateModified = DateTime.UtcNow.AddHours(2);

            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.UpdateSuccess
            };
        }

        public async Task<OutputResponse> Delete(int subscriberId)
        {

            var subscriber = await _context.Subscribers.FirstOrDefaultAsync(x => x.SubscriberId.Equals(subscriberId));

            if (subscriber == null)
            {
                return new OutputResponse
                {
                    IsErrorOccured = true,
                    Message = "Subscriber specified does not exist, deletion cancelled"
                };
            }

            //deletes the record permanently
            _context.Subscribers.Remove(subscriber);
            await _context.SaveChangesAsync();

            return new OutputResponse
            {
                IsErrorOccured = false,
                Message = MessageHelper.DeleteSuccess
            };
        }
    }
}
