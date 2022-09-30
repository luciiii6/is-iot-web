using System.Threading.Tasks;
using IsIoTWeb.Models.Schedule;
using MongoDB.Bson;

namespace IsIoTWeb.Repository
{
    public interface IScheduleRepository : IBaseRepository<Schedule>
    {
        public Task Delete(ObjectId id);
    }
}
