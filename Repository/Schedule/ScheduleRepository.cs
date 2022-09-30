using System.Collections.Generic;
using System.Threading.Tasks;
using IsIoTWeb.Context;
using IsIoTWeb.Models.Schedule;
using MongoDB.Bson;
using MongoDB.Driver;

namespace IsIoTWeb.Repository
{
    public class ScheduleRepository : BaseRepository<Schedule>, IScheduleRepository
    {
        private const string CollectionName = "schedules";

        public ScheduleRepository(IMongoDbContext context) : base(context, CollectionName)
        {
        }

        private async Task<FilterDefinition<Schedule>> BuildDefaultFilter()
        {
            var sink = await GetSink();
            return Builders<Schedule>.Filter.Eq("sinkId", sink.SinkId);
        }

        public override async Task<IEnumerable<Schedule>> GetAll()
        {
            var filter = await BuildDefaultFilter();
            return await _collection.Find(filter).ToListAsync();
        }

        public virtual async Task Delete(ObjectId id)
        {
            await _collection.DeleteOneAsync(Builders<Schedule>.Filter.Eq("_id", id));
        }
    }
}
