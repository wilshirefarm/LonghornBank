using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team16LonghornBank.Models
{
    public class TransactionsDetailsViewModel
    {
        public int? TransactionID { get; set; }
        public Transaction Transaction { get; set; }
        public List<Transaction> FiveTransactions { get; set; }
    }
}