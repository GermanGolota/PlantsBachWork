using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class CurrentUserAddress
    {
        public string[] Cities { get; set; }
        public short[] Posts { get; set; }
    }
}
