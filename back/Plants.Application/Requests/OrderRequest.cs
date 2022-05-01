using Humanizer;
using MediatR;
using System;

namespace Plants.Application.Requests
{
    public record OrderRequest(int OrderId) : IRequest<OrderResult>;
    public class OrderResult
    {
        public long Id { get; set; }
        public string PlantName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string SoilName { get; set; }
        public string[] Regions { get; set; }
        public string GroupName { get; set; }
        private DateTime created;

        public DateTime Created
        {
            get { return created; }
            set
            {
                created = value;
                CreatedHumanDate = value.Humanize();
                CreatedDate = value.ToShortDateString();
            }
        }

        public string SellerName { get; set; }
        public string SellerPhone { get; set; }
        public long SellerCared { get; set; }
        public long SellerSold { get; set; }
        public long SellerInstructions { get; set; }
        public long CareTakerCared { get; set; }
        public long CareTakerSold { get; set; }
        public long CareTakerInstructions { get; set; }
        public OrderResult()
        {

        }

        public OrderResult(long id, string plantName, string description, decimal price,
            string soilName, string[] regions, string groupName, DateTime created, string sellerName,
            string sellerPhone, long sellerCared, long sellerSold, long sellerInstructions,
            long careTakerCared, long careTakerSold, long careTakerInstructions)
        {
            Id = id;
            PlantName = plantName;
            Description = description;
            Price = price;
            SoilName = soilName;
            Regions = regions;
            GroupName = groupName;
            Created = created;
            SellerName = sellerName;
            SellerPhone = sellerPhone;
            SellerCared = sellerCared;
            SellerSold = sellerSold;
            SellerInstructions = sellerInstructions;
            CareTakerCared = careTakerCared;
            CareTakerSold = careTakerSold;
            CareTakerInstructions = careTakerInstructions;
        }
        public string CreatedHumanDate { get; set; }
        public string CreatedDate { get; set; }
    }
}
