using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PlantCaringInstruction
    {
        public int Id { get; set; }
        public string InstructionText { get; set; }
        public int PostedById { get; set; }
        public int PlantGroupId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public virtual PlantGroup PlantGroup { get; set; }
        public virtual Person PostedBy { get; set; }
        public virtual InstructionToCover InstructionToCover { get; set; }
    }
}
