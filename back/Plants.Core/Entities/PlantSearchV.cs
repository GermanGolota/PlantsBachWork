using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantSearchV
    {
        public int? Id { get; set; }
        public string PlantName { get; set; }
        public decimal? Price { get; set; }
        public DateTime? Created { get; set; }
        public int? GroupId { get; set; }
        public int? SoilId { get; set; }
        public int[] Regions { get; set; }
    }
}
