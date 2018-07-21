using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class SellStockViewModel
    {
        [Display(Name = "Stock Name")]
        public String StockName { get; set; }

        [Display(Name = "Sell Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime SellDate { get; set; }

        [Display(Name = "Shares to sell")]
        public Int64 NumberOfSharesToSell { get; set; }

        [Display(Name = "Shares remaining")]
        public Int64 NumberOfSharesRemaining { get; set; }

        [DataType(DataType.Currency)]
        public Decimal Fees { get; set; }

        [Display(Name = "Net Gain/Loss")]
        [DataType(DataType.Currency)]
        public Decimal NetGainLoss { get; set; }

        public Int32 stockPurchaseID { get; set; }

        public Int32 stockPortfolioID { get; set; }
    }
}