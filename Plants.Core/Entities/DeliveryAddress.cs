using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class DeliveryAddress
    {
        public int Id { get; set; }
        public string City { get; set; }
        public short NovaPoshtaNumber { get; set; }
        public int RegionId { get; set; }
        public int PersonId { get; set; }

        public virtual Person Person { get; set; }
        public virtual PlantRegion Region { get; set; }
    }
}
