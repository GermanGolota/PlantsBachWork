using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PersonToDelivery
    {
        public int Id { get; set; }
        public int? PersonId { get; set; }
        public int? DeliveryAddressId { get; set; }

        public virtual DeliveryAddress DeliveryAddress { get; set; }
        public virtual Person Person { get; set; }
    }
}
