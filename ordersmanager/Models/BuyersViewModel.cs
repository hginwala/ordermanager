using MongoDB.Bson;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ordersmanager.Controllers
{
    public class ClientsViewModel
    {
        public ClientsViewModel()
        {
            ClientSelectList = new List<SelectListItem>();
            ClientsList = new List<Client>();
        }
        public List<SelectListItem> ClientSelectList { get; set; }
        public List<Client> ClientsList { get; set; }
        public string currentClientId { get; set; }
    }

    public class Client
    {       
        public ObjectId _id { get; set; }
        [Display(AutoGenerateField =true,Name ="Company")]
        public string CompanyName { get; set; }
        [Display(AutoGenerateField = true)]
        public string Phone { get; set; }
        [Display(AutoGenerateField = true, Name = "Contact person")]
        public string ContactName { get; set; } //make this list to add multiple
        [Display(AutoGenerateField = true, Name = "Address line 1")]
        public string StreetAddress1 { get; set; }
        [Display(AutoGenerateField = true, Name = "Address line 2")]        
        public string StreetAddress2 { get; set; }
        [Display(AutoGenerateField = true)]
        public string City { get; set; }
        [Display(AutoGenerateField = true)]
        public string State { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string ClientId { get; set; }

    }
}