using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantDelivery
    {
        public int OrderId { get; set; }
        public string DeliveryTrackingNumber { get; set; }
        public DateTime? Created { get; set; }

        public virtual PlantOrder Order { get; set; }
        public virtual PlantShipment PlantShipment { get; set; }
    }
}
