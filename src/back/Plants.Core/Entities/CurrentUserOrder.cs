using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class CurrentUserOrder
    {
        public int? Status { get; set; }
        public int? PostId { get; set; }
        public DateTime? Ordered { get; set; }
        public string City { get; set; }
        public short? MailNumber { get; set; }
        public string SellerName { get; set; }
        public string SellerContact { get; set; }
        public decimal? Price { get; set; }
        public string DeliveryTrackingNumber { get; set; }
        public DateTime? DeliveryStarted { get; set; }
        public DateTime? Shipped { get; set; }
    }
}
