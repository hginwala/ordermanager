using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ordersmanager.Models.Dashboard;
using ordersmanager.Models.Order;
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
    public class OrderController : Controller
    {
        private readonly string mongoconn;
        private readonly MongoClient client;
        private readonly IMongoDatabase database;
        public string path = @"C:\orders";
        private HashSet<string> sizeDict = new HashSet<string>() { "3m", "6m", "9m", "12m", "2t", "24m", "3t", "4t", "5t", "6t", "36m", "18m" };

        public OrderController()
        {
            mongoconn = ConfigurationManager.AppSettings["mongoConnection"];
            client = new MongoClient(mongoconn);
            database = client.GetDatabase("pmdatastore");

        }
        // GET: Order
        public ActionResult Index()
        {
            var collection = database.GetCollection<BsonDocument>("seasons");

            var seasonModel = collection.Find(new BsonDocument()).ToEnumerable();

            var seasonView = new SeasonsViewModel();

            foreach (var a in seasonModel)
            {
                seasonView.Seasons.Add(new SelectListItem() { Value = a["name"].ToString(), Text = a["name"].ToString() });
            }

            return View(seasonView);
        }

        public PartialViewResult AddClient(string id)
        {
            if (string.IsNullOrEmpty(id))
                return PartialView(new Client());
            var clientscollection = database.GetCollection<BsonDocument>("clients");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));
            var clientModel = clientscollection.Find(filter).SingleOrDefault();
            Client vModel = BsonSerializer.Deserialize<Client>(clientModel.ToJson());
            vModel.ClientId = vModel._id.ToString();
            return PartialView(vModel);

        }

        public ActionResult Upsert(long orderId = 0)
        {
            var clientscollection = database.GetCollection<BsonDocument>("clients");

            var clientModel = clientscollection.Find(new BsonDocument()).ToEnumerable();

            var clientView = new ClientsViewModel();

            foreach (var a in clientModel)
            {
                clientView.ClientSelectList.Add(new SelectListItem() { Value = a["_id"].ToString(), Text = a["CompanyName"].ToString() });
            }

            return View(clientView);
        }

        [HttpPost]
        public ActionResult Upsert(Client client, HttpPostedFileBase upload)
        {
            var clientscollection = database.GetCollection<BsonDocument>("clients");

            if (client.ClientId == ObjectId.Empty.ToString())
            {
                // client._id = ObjectId.GenerateNewId();
                var jsondata = JsonConvert.SerializeObject(client);
                var document = BsonSerializer.Deserialize<BsonDocument>(jsondata);
                clientscollection.InsertOne(document);
            }

            if (upload != null && upload.ContentLength > 0)
            {
                var fileName = Path.Combine(path, upload.FileName);
                string connectionString;
                if (fileName.Contains("xlsx"))
                {
                    connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Excel 12.0 Xml;HDR=Yes;IMEX=1", fileName);
                }
                else
                {
                    connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0", fileName);
                }

                var orderitems = new OleDbDataAdapter("SELECT * FROM [orderdetails$]", connectionString);
                //   var buyer = new OleDbDataAdapter("SELECT * FROM [orderinfo$]", connectionString);
                var ds = new DataSet();

                orderitems.Fill(ds, "orderitems");
                /// buyer.Fill(ds, "buyerinfo");

                var orderdata = ds.Tables["orderitems"];
                //  var buyerdata = ds.Tables["buyerinfo"];

                foreach (var column in orderdata.Columns.Cast<DataColumn>().ToArray())
                {
                    if (orderdata.AsEnumerable().All(dr => dr.IsNull(column)))
                        orderdata.Columns.Remove(column);
                }
                try
                {
                    var j = SaveDataTableToCollection(orderdata, client);
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
            return Upsert(0);
        }

        //public void ImportFiles()
        //{
        //    if (Directory.Exists(path))
        //    {
        //        string[] fileEntries = Directory.GetFiles(path);
        //        foreach (string fileName in fileEntries)
        //        {
        //            string connectionString;
        //            if (fileName.Contains("xlsx"))
        //            {
        //                connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Extended Properties=Excel 12.0;", fileName);
        //            }
        //            else
        //            {
        //                connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", fileName);
        //            }

        //            var orderitems = new OleDbDataAdapter("SELECT * FROM [orderdetails$]", connectionString);
        //            var buyer = new OleDbDataAdapter("SELECT * FROM [orderinfo$]", connectionString);
        //            var ds = new DataSet();

        //            orderitems.Fill(ds, "orderitems");
        //            buyer.Fill(ds, "buyerinfo");

        //            var orderdata = ds.Tables["orderitems"];
        //            var buyerdata = ds.Tables["buyerinfo"];

        //            foreach (var column in orderdata.Columns.Cast<DataColumn>().ToArray())
        //            {
        //                if (orderdata.AsEnumerable().All(dr => dr.IsNull(column)))
        //                    orderdata.Columns.Remove(column);
        //            }
        //            try
        //            {
        //                var j = SaveDataTableToCollection(orderdata, buyerdata);
        //            }
        //            catch (Exception e)
        //            {
        //                throw e;
        //            }
        //            //   return ImportOrderResult();

        //        }
        //    }
        //}

        public async Task SaveDataTableToCollection(DataTable orderDetails, Client buyerInfo)
        {
            var collection = database.GetCollection<BsonDocument>("orderdetails");
            Dictionary<string, string> orderItems = new Dictionary<string, string>();
            BsonArray sizes = new BsonArray();
            foreach (DataRow dr in orderDetails.Rows)
            {
                orderItems = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName].ToString());

                var emptykeys = orderItems.Where(pair => string.IsNullOrEmpty(pair.Value))
                        .Select(pair => pair.Key)
                        .ToList();
                foreach (var badKey in emptykeys)
                {
                    orderItems.Remove(badKey);
                }
                if (orderItems.Keys.Count > 0)
                {

                    foreach (var key in orderItems.Keys)
                    {
                        string cleankey = key.Trim().Replace(" ", string.Empty).ToLower();
                        if (sizeDict.Contains(cleankey))
                        {
                            Sizing s = new Sizing()
                            {
                                size = cleankey,
                                Quantity = int.Parse(orderItems[key])
                            };
                            sizes.Add(s.ToBson());
                        }

                    }

                    // order.Add(new BsonDocument(orderItems));
                }
            }
            BsonDocument mongoDoc = new BsonDocument();
            var clientObj = new JObject();
            clientObj.Add("Id", buyerInfo.ClientId);
            clientObj.Add("Name", buyerInfo.CompanyName);
            mongoDoc.Add(new BsonElement("Client", clientObj.ToJson()));
            mongoDoc.Add(new BsonElement("Season", "Fall2015"));
            mongoDoc.Add(new BsonElement("OrderItems", sizes));

            await collection.InsertOneAsync(mongoDoc);
        }
    }


}