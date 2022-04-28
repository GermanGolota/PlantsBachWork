using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantPost
    {
        public int PlantId { get; set; }
        public int SellerId { get; set; }
        public decimal Price { get; set; }
        public DateTime Created { get; set; }

        public virtual Plant Plant { get; set; }
        public virtual Person Seller { get; set; }
        public virtual PlantOrder PlantOrder { get; set; }
    }
}
