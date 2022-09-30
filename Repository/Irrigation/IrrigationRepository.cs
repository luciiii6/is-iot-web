using IsIoTWeb.Context;
using IsIoTWeb.Models;
using IsIoTWeb.Utils;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IsIoTWeb.Repository
{
    public class IrrigationRepository : BaseRepository<IrrigationLog>, IIrrigationRepository
    {
        private const string CollectionName = "irrigations_sid";

        public IrrigationRepository(IMongoDbContext context) : base(context, CollectionName)
        {
        }

        private async Task<FilterDefinition<IrrigationLog>> BuildDefaultFilter()
        {
            var sink = await GetSink();
            return Builders<IrrigationLog>.Filter.Eq("sinkId", sink.SinkId);
        }

        public async Task<IEnumerable<IrrigationLog>> GetAllByFilter(IrrigationLogsFilter filter)
        {
            var descending = Builders<IrrigationLog>.Sort.Descending("timestamp");
            var mongoFilter = await BuildDefaultFilter();
            int pageSize = 1000;

            if (filter != null)
            {
                if (filter.PageSize != null)
                {
                    pageSize = (int)filter.PageSize;
                }

                if (!string.IsNullOrEmpty(filter.FromDate) && !string.IsNullOrEmpty(filter.ToDate))
                {
                    var fromDatetime = new DateTimeOffset(DateTime.Parse(filter.FromDate)).ToUnixTimeSeconds();
                    var toDatetime = new DateTimeOffset(DateTime.Parse(filter.ToDate)).ToUnixTimeSeconds();
                    mongoFilter &= Builders<IrrigationLog>.Filter.Gte("timestamp", fromDatetime);
                    mongoFilter &= Builders<IrrigationLog>.Filter.Lte("timestamp", toDatetime);
                }
            }

            List<IrrigationLog> data = new List<IrrigationLog>();
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
