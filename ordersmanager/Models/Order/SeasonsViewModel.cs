using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ordersmanager.Models.Order
{
    public class SeasonsViewModel
    {
        public SeasonsViewModel()
        {
            Seasons = new List<SelectListItem>();
        }
        public List<SelectListItem> Seasons { get; set; }

        public string currentSeason { get; set; }

        public List<OrderOverView> OrderOverViewList { get; set; }
    }
}