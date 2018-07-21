using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team16LonghornBank.Models
{
    public class StockQuote
    {
            public String Symbol { get; set; }
            public String Name { get; set; }
            public Decimal PreviousClose { get; set; }
            public Decimal LastTradePrice { get; set; }
            public Double Volume { get; set; }
    }
}
