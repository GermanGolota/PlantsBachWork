using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantToRegion
    {
        public int Id { get; set; }
        public int PlantId { get; set; }
        public int PlantRegionId { get; set; }

        public virtual Plant Plant { get; set; }
        public virtual PlantRegion PlantRegion { get; set; }
    }
}
