using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class Stock
    {
        public Int32 StockID { get; set; }

        [Display(Name = "Ticker Symbol")]
        [Required]
        public String TickerSymbol { get; set; }

        [Display(Name = "Stock Name")]
        [Required]
        public String StockName { get; set; }

        [Display(Name = "Type of Stock")]
        [Required]
        public String StockType { get; set; }

        [Display(Name = "Current Price")]
        [DataType(DataType.Currency)]
        public Decimal CurrentPrice { get; set; }

        [DataType(DataType.Currency)]
        public Decimal Fee { get; set; }

        public String StockDescription
        {
            get { return TickerSymbol + ", " + StockName + ", " + StockType + ", Current Price: " + CurrentPrice.ToString("c") + ", Fee: " + Fee.ToString("c"); }
        }


        //navigational properties
        public virtual List<StockPurchase> StockPurchases { get; set; }
    }
}