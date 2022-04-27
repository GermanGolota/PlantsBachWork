using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class Plant
    {
        public Plant()
        {
            PlantToImages = new HashSet<PlantToImage>();
            PlantToRegions = new HashSet<PlantToRegion>();
        }

        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SoilId { get; set; }
        public int CareTakerId { get; set; }
        public string PlantName { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }

        public virtual Person CareTaker { get; set; }
        public virtual PlantGroup Group { get; set; }
        public virtual PlantSoil Soil { get; set; }
        public virtual PlantPost PlantPost { get; set; }
        public virtual ICollection<PlantToImage> PlantToImages { get; set; }
        public virtual ICollection<PlantToRegion> PlantToRegions { get; set; }
    }
}
