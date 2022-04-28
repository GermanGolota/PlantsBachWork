using System;
using System.Collections.Generic;

#nullable disable

namespace Plants.Core.Entities
{
    public partial class Person
    {
        public Person()
        {
            DeliveryAddresses = new HashSet<DeliveryAddress>();
            PlantCaringInstructions = new HashSet<PlantCaringInstruction>();
            PlantOrders = new HashSet<PlantOrder>();
            PlantPosts = new HashSet<PlantPost>();
            Plants = new HashSet<Plant>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }

        public virtual PersonToLogin PersonToLogin { get; set; }
        public virtual ICollection<DeliveryAddress> DeliveryAddresses { get; set; }
        public virtual ICollection<PlantCaringInstruction> PlantCaringInstructions { get; set; }
        public virtual ICollection<PlantOrder> PlantOrders { get; set; }
        public virtual ICollection<PlantPost> PlantPosts { get; set; }
        public virtual ICollection<Plant> Plants { get; set; }
    }
}
