using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class StockPortfolio
    {
        public Int32 StockPortfolioID { get; set; }
        [Display(Name = "Account Number")]
        public Int32 AccountNumber { get; set; }

        [Display(Name = "Account Number")]
        public String FourDigitNumber
        {
            get { return "XXXXXX" + AccountNumber.ToString().Substring(6); }
        }

        [Display(Name = "Account Name")]
        public String AccountName { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Cash Value")]
        public Decimal CashValueBalance { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Stock Portion Value")]
        public Decimal StockPortionValue { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Total Stock Portfolio Value")]
        public Decimal TotalValue
        {
            get { return CashValueBalance + StockPortionValue; }
        }

        [Display(Name = "Is this portfolio balanced?")]
        public Boolean isBalanced { get; set; }

        public Boolean isPending { get; set; }

        public String StockPortfolioDisplay
        {
            get { return AccountName + ", Account Number: XXXXXX" + AccountNumber.ToString().Substring(6) + ", Balance: $" + CashValueBalance.ToString(); }
        }


        //navigational properties
        public virtual AppUser Customer { get; set; }
        public virtual List<StockPurchase> StockPurchases { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
    }
}