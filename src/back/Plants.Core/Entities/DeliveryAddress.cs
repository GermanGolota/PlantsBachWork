using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class DeliveryAddress
    {
        public DeliveryAddress()
        {
            PersonToDeliveries = new HashSet<PersonToDelivery>();
            PlantOrders = new HashSet<PlantOrder>();
        }

        public int Id { get; set; }
        public string City { get; set; }
        public short NovaPoshtaNumber { get; set; }

        public virtual ICollection<PersonToDelivery> PersonToDeliveries { get; set; }
        public virtual ICollection<PlantOrder> PlantOrders { get; set; }
    }
}
