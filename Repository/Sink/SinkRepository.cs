using IsIoTWeb.Context;
using IsIoTWeb.Models;
using IsIoTWeb.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public class SinkRepository : BaseRepository<Sink>, ISinkRepository
    {
        private const string CollectionName = "sinks";

        public SinkRepository(IMongoDbContext context) : base(context, CollectionName)
        {
        }

        public async Task<string> GetIdByUser(User user)
        {
            try
            {
                var asyncCursor = await _collection.FindAsync(Builders<Sink>.Filter.Empty);
                var all = await asyncCursor.ToListAsync();
                string userId = Utils.Utils.DynamicObjectIdToString(user.Id.Timestamp, user.Id.Machine, user.Id.Pid, user.Id.Increment);
                var sink = all.Find(x => x.Users.Contains(userId));
                return sink == null ? null : sink.SinkId;
            }
            catch (System.NullReferenceException)
            {
                return null;
            }
        }

        public async Task AddUser(string id)
        {
            Sink sink = await GetSink();
            sink.Users.Add(id);
            await Update(sink);
        }

        public async Task RemoveUser(string id)
        {
            Sink sink = await GetSink();
            sink.Users.Remove(id);
            await Update(sink);
        }

        public async Task AddCollector(string id)
        {
            Sink sink = await GetSink();
            sink.Collectors.Add(id);
            await Update(sink);
        }

        public async Task RemoveCollector(string id)
        {
            Sink sink = await GetSink();
            sink.Collectors.Remove(id);
            await Update(sink);
        }
    }
}
