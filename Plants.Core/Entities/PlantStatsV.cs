using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantStatsV
    {
        public string GroupName { get; set; }
        public long? PlantsCount { get; set; }
        public long? PlantsCountRank { get; set; }
        public long? Popularity { get; set; }
        public long? PopularityRank { get; set; }
        public decimal? Income { get; set; }
        public long? IncomeRank { get; set; }
        public long? Instructions { get; set; }
        public long? InstructionsRank { get; set; }
    }
}
