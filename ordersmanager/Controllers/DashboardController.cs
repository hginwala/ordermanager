using MongoDB.Bson;
using MongoDB.Driver;
using ordersmanager.Models.Dashboard;
using ordersmanager.Models.Order;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ordersmanager.Controllers
{
    public class DashboardController : Controller
    {
        private readonly string mongoconn;
        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        public string path = @"C:\orders";

        public DashboardController()
        {
            mongoconn = ConfigurationManager.AppSettings["mongoConnection"];
            client = new MongoClient(mongoconn);
            database = client.GetDatabase("pmdatastore");
        }

        // GET: Dashboard
        public ActionResult Index()
        {
            var collection = database.GetCollection<BsonDocument>("seasons");

            var seasonModel = collection.Find(new BsonDocument()).ToEnumerable();

            var seasonView = new SeasonsViewModel();

            foreach (var a in seasonModel)
            {
                seasonView.Seasons.Add(new SelectListItem() { Value = a["name"].ToString(), Text = a["name"].ToString() });
            }

            var clientscollection = database.GetCollection<BsonDocument>("clients");

            var clientModel = clientscollection.Find(new BsonDocument()).ToEnumerable();

            var clientView = new ClientsViewModel();

            foreach (var a in clientModel)
            {
                clientView.ClientSelectList.Add(new SelectListItem() { Value = a["_id"].ToString(), Text = a["CompanyName"].ToString() });
            }

            Dashboardview model = new Dashboardview()
            {
                ClientsViewModel = clientView,
                SeasonsViewModel = seasonView
            };
            return View(model);
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Filter(string currentClientId, string currentSeason)
        {

            var collection = database.GetCollection<BsonDocument>("orderdetails");

            var filter = Builders<BsonDocument>.Filter.Eq("Season", currentSeason);
            var result = await collection.Find(filter).ToListAsync();

            List<OrderDetails> model = new List<OrderDetails>();

            foreach(var o in result)
            {
                OrderDetails d = new OrderDetails()
                {
                    description = o["Description"].ToJson(),
                    style = o["Style"].ToJson(),
                    unitPrice = decimal.Parse(o["Unit Price"].ToString()),
                  //  sizeDistribution = 
                };
            }
            
            return View("Index");
        }
    }
}