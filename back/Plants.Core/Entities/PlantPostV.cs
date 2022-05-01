using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantPostV
    {
        public int? Id { get; set; }
        public string PlantName { get; set; }
        public decimal? Price { get; set; }
        public string GroupName { get; set; }
        public string SoilName { get; set; }
        public string Description { get; set; }
        public string[] Regions { get; set; }
        public string SellerName { get; set; }
        public string SellerPhone { get; set; }
        public long? CaretakerExperience { get; set; }
    }
}
