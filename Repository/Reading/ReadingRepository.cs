using IsIoTWeb.Context;
using IsIoTWeb.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public class ReadingRepository : BaseRepository<Reading>, IReadingRepository
    {
        private const string CollectionName = "readings";

        public ReadingRepository(IMongoDbContext context) : base(context, CollectionName)
        {
        }

        // Override Update method because no Reading should be modified
        public override async Task Update(Reading obj)
        {
            await Task.CompletedTask;
        }

        // Override Update method because no Reading should be modified
        public override async Task Delete(string id)
        {
            await Task.CompletedTask;
        }

        private async Task<FilterDefinition<Reading>> BuildDefaultFilter()
        {
            var ids = await GetCurrentCollectorIds();
            return Builders<Reading>.Filter.In("collectorId", ids);
        }

        public async Task<IEnumerable<Reading>> GetAllByFilter(ReadingFilter filter)
        {
            var descending = Builders<Reading>.Sort.Descending("timestamp");
            var mongoFilter = await BuildDefaultFilter();
            int pageSize = 1000;

            if (filter != null)
            {
                if (filter.PageSize != null)
                {
                    pageSize = (int)filter.PageSize;
                }

                if (!string.IsNullOrEmpty(filter.CollectorId))
                {
                    mongoFilter &= Builders<Reading>.Filter.Eq("collectorId", filter.CollectorId);
                }

                if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
                {
                    var fromDatetime = new DateTimeOffset(DateTime.Parse(filter.FromDate)).ToUnixTimeSeconds();
                    var toDatetime = new DateTimeOffset(DateTime.Parse(filter.ToDate)).ToUnixTimeSeconds();
                    mongoFilter &= Builders<Reading>.Filter.Gte("timestamp", fromDatetime);
                    mongoFilter &= Builders<Reading>.Filter.Lte("timestamp", toDatetime);
                }
            }

            List<Reading> data = new List<Reading>();
            if (pageSize == -1)
            {
                data = await _collection.Find(mongoFilter).Sort(descending).ToListAsync();
            }
            else
            {
                data = await _collection.Find(mongoFilter).Sort(descending).Limit(pageSize).ToListAsync();
            }

            data.Reverse();
            return data;
        }
    }
}
