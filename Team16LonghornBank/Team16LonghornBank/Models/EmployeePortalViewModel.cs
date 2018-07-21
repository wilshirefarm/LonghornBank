using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team16LonghornBank.Models
{
    public class EmployeePortalViewModel
    {
        public IEnumerable<AppUser> Customers { get; set; }
        public IEnumerable<AppUser> Employees { get; set; }
        public IEnumerable<AppUser> Managers { get; set; }
        public List<Transaction> PendingTransactions { get; set; }
        public List<Dispute> Disputes { get; set; }
        public List<StockPortfolio> PendingStockPortfolios { get; set; }
    }
}