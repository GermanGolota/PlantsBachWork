using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantShipment
    {
        public int OrderId { get; set; }
        public DateTime Shipped { get; set; }

        public virtual PlantOrder Order { get; set; }
    }
}
