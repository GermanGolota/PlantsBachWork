using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PersonAddressesV
    {
        public int? Id { get; set; }
        public string[] Cities { get; set; }
        public short[] Posts { get; set; }
    }
}
