using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team16LonghornBank.Models
{
    public class PayBillsOnlineViewModel
    {
        public AppUser Customer { get; set; }
        public List<Payee> Payees { get; set; }
        public Transaction Transaction { get; set; }
    }
}