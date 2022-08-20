using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class PersonToLogin
    {
        public int PersonId { get; set; }

        public virtual Person Person { get; set; }
    }
}
