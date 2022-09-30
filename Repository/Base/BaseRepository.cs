using IsIoTWeb.Context;
using IsIoTWeb.Models;
using IsIoTWeb.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public abstract class BaseRepository<TDocument> : IBaseRepository<TDocument> where TDocument : IDocument
    {
        protected readonly IMongoDbContext _context;
        protected IMongoCollection<TDocument> _collection;
        protected IMongoCollection<Sink> _sinkCollection;

        protected BaseRepository(IMongoDbContext context, string collectionName)
        {
            _context = context;
            _collection = _context.GetCollection<TDocument>(collectionName);
            _sinkCollection = _context.GetCollection<Sink>("sinks");
        }

        public virtual async Task Create(TDocument obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(typeof(TDocument).Name + " is null!");
            }
            await _collection.InsertOneAsync(obj);
        }

        public virtual async Task Delete(string id)
        {
            await _collection.DeleteOneAsync(Builders<TDocument>.Filter.Eq("_id", id));
        }

        public virtual async Task Update(TDocument obj)
        {
            await _collection.ReplaceOneAsync(Builders<TDocument>.Filter.Eq("_id", new ObjectId(obj.Id)), obj);
        }

        public virtual async Task<TDocument> Get(string id)
        {
            var objectId = new ObjectId(id);
            FilterDefinition<TDocument> filter = Builders<TDocument>.Filter.Eq("_id", objectId);
            return await _collection.FindAsync(filter).Result.FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TDocument>> GetAll()
        {
            var all = await _collection.FindAsync(Builders<TDocument>.Filter.Empty);
            return await all.ToListAsync();
        }

        public async Task<ICollection<string>> GetCurrentCollectorIds()
        {
            var sink = await GetSink();
            return sink.Collectors;
        }

        public async Task<Sink> GetSink()
        {
            FilterDefinition<Sink> filter = Builders<Sink>.Filter.Eq("sinkId", StaticVariables.SinkId);
            return await _sinkCollection.FindAsync(filter).Result.FirstOrDefaultAsync();
        }

        public async Task<ICollection<string>> GetCurrentUserIds()
        {
            var sink = await GetSink();
            return sink.Users;
        }
    }
}
