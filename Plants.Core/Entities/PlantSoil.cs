using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantSoil
    {
        public PlantSoil()
        {
            Plants = new HashSet<Plant>();
        }

        public int Id { get; set; }
        public string SoilName { get; set; }

        public virtual ICollection<Plant> Plants { get; set; }
    }
}
