using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class InstructionToCover
    {
        public int InstructionId { get; set; }
        public byte[] Image { get; set; }

        public virtual PlantCaringInstruction Instruction { get; set; }
    }
}
