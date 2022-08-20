using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantToImage
    {
        public int RelationId { get; set; }
        public int? PlantId { get; set; }
        public byte[] Image { get; set; }

        public virtual Plant Plant { get; set; }
    }
}
