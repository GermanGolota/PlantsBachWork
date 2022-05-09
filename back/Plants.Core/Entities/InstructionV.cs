using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class InstructionV
    {
        public int? Id { get; set; }
        public int? PlantGroupId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string InstructionText { get; set; }
        public bool? Hascover { get; set; }
    }
}
