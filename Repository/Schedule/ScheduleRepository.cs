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

        public virtual async Task Delete(ObjectId id)
        {
            await _collection.DeleteOneAsync(Builders<Schedule>.Filter.Eq("_id", id));
        }
    }
}
