using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantsV
    {
        public int? Id { get; set; }
        public string PlantName { get; set; }
        public string Description { get; set; }
        public bool? IsMine { get; set; }
        public int? GroupId { get; set; }
        public int? SoilId { get; set; }
        public int[] Images { get; set; }
        public int[] Regions { get; set; }
        public DateTime? Created { get; set; }
    }
}
