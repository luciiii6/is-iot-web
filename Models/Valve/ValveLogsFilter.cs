﻿namespace IsIoTWeb.Models
{
    public class ValveLogsFilter
    {
        public int? ValveId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }

        public int? PageSize { get; set; }
    }
}
