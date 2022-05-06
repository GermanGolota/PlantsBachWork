using MediatR;
using System;
using System.Collections.Generic;

namespace Plants.Application.Requests
{
    public record OrdersRequest() : IRequest<OrdersResult>;

    public record OrdersResult(List<OrdersResultItem> Items);

    public record OrdersResultItem(
        int Status, int PostId,
        string City, int MailNumber, string SellerName,
        string SellerContact, decimal Price, string? DeliveryTrackingNumber)
    {
        //decoder
        public OrdersResultItem() : this(0, 0, "", 0, "", "", 0, null)
        {

        }

        private DateTime ordered;
        private DateTime? deliveryStarted;
        private DateTime? shipped;

        public DateTime Ordered
        {
            get => ordered;
            set
            {
                ordered = value;
                OrderedDate = ordered.ToLongDateString();
            }
        }

        public DateTime? DeliveryStarted
        {
            get => deliveryStarted;
            set
            {
                deliveryStarted = value;
                DeliveryStartedDate = value?.ToLongDateString();
            }
        }

        public DateTime? Shipped
        {
            get => shipped;
            set
            {
                shipped = value;
                ShippedDate = value?.ToLongDateString();
            }
        }

        public string OrderedDate { get; set; }
        public string? DeliveryStartedDate { get; set; }
        public string? ShippedDate { get; set; }
    }
}