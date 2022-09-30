using IsIoTWeb.Context;
using IsIoTWeb.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public class ValveRepository : BaseRepository<ValveLog>, IValveRepository
    {
        private const string CollectionName = "valves_sid";

        public ValveRepository(IMongoDbContext context) : base(context, CollectionName)
        {
        }

        private async Task<FilterDefinition<ValveLog>> BuildDefaultFilter()
        {
            var sink = await GetSink();
            return Builders<ValveLog>.Filter.Eq("sinkId", sink.SinkId);
        }

        public async Task<IEnumerable<ValveLog>> GetAllByFilter(ValveLogsFilter filter)
        {
            var descending = Builders<ValveLog>.Sort.Descending("timestamp");
            var mongoFilter = await BuildDefaultFilter();
            int pageSize = 1000;

            if (filter != null)
            {
                if (filter.PageSize != null)
                {
                    pageSize = (int)filter.PageSize;
                }

                if (filter.ValveId != null)
                {
                    mongoFilter &= Builders<ValveLog>.Filter.Eq("valveId", filter.ValveId);
                }

                if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
                {
                    var fromDatetime = new DateTimeOffset(DateTime.Parse(filter.FromDate)).ToUnixTimeSeconds();
                    var toDatetime = new DateTimeOffset(DateTime.Parse(filter.ToDate)).ToUnixTimeSeconds();
                    mongoFilter &= Builders<ValveLog>.Filter.Gte("timestamp", fromDatetime);
                    mongoFilter &= Builders<ValveLog>.Filter.Lte("timestamp", toDatetime);
                }
            }

            List<ValveLog> data = new List<ValveLog>();
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
