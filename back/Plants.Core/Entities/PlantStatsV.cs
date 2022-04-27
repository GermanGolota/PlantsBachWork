using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantStatsV
    {
        public int? Id { get; set; }
        public string GroupName { get; set; }
        public long? PlantsCount { get; set; }
        public long? Popularity { get; set; }
        public decimal? Income { get; set; }
        public long? Instructions { get; set; }
    }
}
