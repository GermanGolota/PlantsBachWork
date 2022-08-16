using Humanizer;
using MediatR;
using System;

namespace Plants.Application.Requests
{
    public record PreparedPostRequest(int PlantId) : IRequest<PreparedPostResult>;

    public record PreparedPostResult(bool Exists, PreparedPostResultItem Item)
    {
        public PreparedPostResult() : this(false, null)
        {

        }

        public PreparedPostResult(PreparedPostResultItem item) : this(true, item)
        {

        }
    }

    public class PreparedPostResultItem
    {
        public long Id { get; set; }
        public string PlantName { get; set; }
        public string Description { get; set; }
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
        public int[] Images { get; set; }
        public PreparedPostResultItem()
        {

        }

        public PreparedPostResultItem(long id, string plantName, string description,
            string soilName, string[] regions, string groupName, DateTime created, string sellerName,
            string sellerPhone, long sellerCared, long sellerSold, long sellerInstructions,
            long careTakerCared, long careTakerSold, long careTakerInstructions, int[] images)
        {
            Id = id;
            PlantName = plantName;
            Description = description;
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
            Images = images;
        }
        public string CreatedHumanDate { get; set; }
        public string CreatedDate { get; set; }
    }
}
