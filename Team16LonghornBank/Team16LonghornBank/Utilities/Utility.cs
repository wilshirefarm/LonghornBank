using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team16LonghornBank.Utilities
{
    public static class Utility
    {
        public static Int32 AccountNumber = 1000000023;
        public static List<String> AccountTypes = new List<String>() { "Checking Account", "Savings Account", "IRA", "Stock Portfolio" };
        public static List<String> TranscationTypes = new List<String>() { "All", "Deposit", "Withdrawal", "Transfer", "Bill Payment", "Fee", "Bonus" };
        public static List<String> PayeeTypes = new List<String>() { "Credit Card", "Utilities", "Rent", "Mortgage", "Other" };
        public static List<String> CurrentStatus = new List<String>() { "Submitted", "Accepted", "Rejected", "Adjusted" };
        public static List<String> StockTypes = new List<String>() { "Ordinary Stock", "Index Fund", "Exchange-Traded Fund", "Mutual Fund", "Future Share" };
    }
}