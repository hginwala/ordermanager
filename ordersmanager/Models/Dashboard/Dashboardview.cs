using ordersmanager.Controllers;
using ordersmanager.Models.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ordersmanager.Models.Dashboard
{
    public class Dashboardview
    {
        public SeasonsViewModel SeasonsViewModel { get; set; }
        public ClientsViewModel ClientsViewModel { get; set; }
    }

    public class ClientOrderItem
    {
        public string clientName { get; set; }
        public string clientId { get; set; }
        public string season { get; set; }
        public List<OrderDetails> orderItems = new List<OrderDetails>();
    }

    public class OrderDetails
    {
        public string style { get; set; }
        public string description { get; set; }
        public decimal unitPrice { get; set; }
        public List<Sizing> sizeDistribution = new List<Sizing>();
        public int TotalQuantity
        {
            get { return this.sizeDistribution.Sum(x => x.Quantity); }
        }
        public decimal SubTotal
        {
            get { return this.TotalQuantity * this.unitPrice; }
        }
    }

    public class Sizing
    {
         public string size { get; set; }
         public int Quantity { get; set; }
    }

    public enum Size
    {
       ThreeMonths,
       SixMonths,
       NineMonths,
       TwelveMonths,
       EightenMonths,
       TwoT,
       ThreeT,
       FourT,
       FiveT,
       SixT
    };
   
}