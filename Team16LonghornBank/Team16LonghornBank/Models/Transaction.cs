using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class Transaction
    {
        public Int32 TransactionID { get; set; }
        public String TransactionType { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TransactionDate { get; set; }

        [Range(.01, Double.MaxValue, ErrorMessage = "Please enter an amount greater than 1 cent")]
        [DataType(DataType.Currency)]
        public Decimal Amount { get; set; }
        public String Description { get; set; }

        [Display(Name = "Pending?")]
        public Boolean isPending { get; set; }

        [Display(Name = "Disputed?")]
        public Boolean isBeingDisputed { get; set; }

        [Display(Name = "Employee Comments")]
        public String EmployeeComments { get; set; }

        public virtual CheckingAccount CheckingAccountAffected { get; set; }
        public virtual SavingsAccount SavingsAccountAffected { get; set; }
        public virtual IRAccount IRAccountAffected { get; set; }
        public virtual StockPortfolio StockPortfolioAffected { get; set; }

        public virtual CheckingAccount TransferToCheckingAccount { get; set; }
        public virtual CheckingAccount TransferFromCheckingAccount { get; set; }

        public virtual SavingsAccount TransferToSavingsAccount { get; set; }
        public virtual SavingsAccount TransferFromSavingsAccount { get; set; }

        public virtual IRAccount TransferToIRAccount { get; set; }
        public virtual IRAccount TransferFromIRAccount { get; set; }

        public virtual StockPortfolio TransferToStockPortfolio { get; set; }
        public virtual StockPortfolio TransferFromStockPortfolio { get; set; }

        public virtual Dispute Dispute { get; set; }
        public virtual Payee Payee { get; set; }
    }
}