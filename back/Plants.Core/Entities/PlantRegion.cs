using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantRegion
    {
        public PlantRegion()
        {
            DeliveryAddresses = new HashSet<DeliveryAddress>();
            PlantToRegions = new HashSet<PlantToRegion>();
        }

        public int Id { get; set; }
        public string RegionName { get; set; }

        public virtual ICollection<DeliveryAddress> DeliveryAddresses { get; set; }
        public virtual ICollection<PlantToRegion> PlantToRegions { get; set; }
    }
}
