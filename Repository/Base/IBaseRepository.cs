using IsIoTWeb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public interface IBaseRepository<TDocument> where TDocument : IDocument
    {
        Task Create(TDocument obj);
        Task Update(TDocument obj);
        Task Delete(string id);
        Task<TDocument> Get(string id);
        Task<IEnumerable<TDocument>> GetAll();

        Task<Sink> GetSink();
    }
}
