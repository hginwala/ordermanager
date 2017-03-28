using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ordersmanager.Controllers
{
    public class HomeController : Controller
    {
        private readonly string mongoconn;
        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        public HomeController()
        {
            mongoconn = ConfigurationManager.AppSettings["mongoConnection"];
            client = new MongoClient(mongoconn);
            database = client.GetDatabase("pmdatastore");
        }
        public string path = @"C:\orders";
        public ActionResult Index()
        {
            ViewBag.Title = "my home Page";
            return View();
        }
    }
}
