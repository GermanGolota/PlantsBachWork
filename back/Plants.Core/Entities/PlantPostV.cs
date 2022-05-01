using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantPostV
    {
        public int? Id { get; set; }
        public string PlantName { get; set; }
        public string Description { get; set; }
        public decimal? Price { get; set; }
        public string SoilName { get; set; }
        public string[] Regions { get; set; }
        public string GroupName { get; set; }
        public DateTime? Created { get; set; }
        public string SellerName { get; set; }
        public string SellerPhone { get; set; }
        public long? SellerCared { get; set; }
        public long? SellerSold { get; set; }
        public long? SellerInstructions { get; set; }
        public long? CareTakerCared { get; set; }
        public long? CareTakerSold { get; set; }
        public long? CareTakerInstructions { get; set; }
    }
}
