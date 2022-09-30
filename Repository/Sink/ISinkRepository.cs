using IsIoTWeb.Models;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public interface ISinkRepository : IBaseRepository<Sink>
    {
        public Task<string> GetIdByUser(User user);

        public Task<ICollection<string>> GetCurrentCollectorIds();

        public Task<ICollection<string>> GetCurrentUserIds();

        public Task AddUser(string id);

        public Task RemoveUser(string id);

        public Task AddCollector(string id);

        public Task RemoveCollector(string id);
    }
}
