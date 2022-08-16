using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PersonCredsV
    {
        public int? Id { get; set; }
        public long? CaredCount { get; set; }
        public long? SoldCount { get; set; }
        public long? InstructionsCount { get; set; }
    }
}
