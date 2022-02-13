using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantGroup
    {
        public PlantGroup()
        {
            PlantCaringInstructions = new HashSet<PlantCaringInstruction>();
            Plants = new HashSet<Plant>();
        }

        public int Id { get; set; }
        public string GroupName { get; set; }

        public virtual ICollection<PlantCaringInstruction> PlantCaringInstructions { get; set; }
        public virtual ICollection<Plant> Plants { get; set; }
    }
}
