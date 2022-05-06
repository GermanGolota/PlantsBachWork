using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantShipment
    {
        public int DeliveryId { get; set; }
        public DateTime? Shipped { get; set; }

        public virtual PlantDelivery Delivery { get; set; }
    }
}
