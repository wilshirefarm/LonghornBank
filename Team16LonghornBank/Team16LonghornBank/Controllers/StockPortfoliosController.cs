using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Team16LonghornBank.Utilities;
using Team16LonghornBank.Models;
using Microsoft.AspNet.Identity;

namespace Team16LonghornBank.Controllers
{
    public enum SellStockDecision { Confirm, Cancel };

    [Authorize]
    public class StockPortfoliosController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: StockPortfolios
        public ActionResult Index()
        {
            return View(db.StockPortfolios.ToList());
        }

        // GET: StockPortfolios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockPortfolio stockPortfolio = db.StockPortfolios.Find(id);
            if (stockPortfolio == null)
            {
                return HttpNotFound();
            }

            foreach (var s in db.Stocks)
            {
                s.CurrentPrice = GetQuote.GetStock(s.TickerSymbol).PreviousClose;
            }
            foreach (var p in db.StockPurchases)
            {
                p.TotalStockValue = p.Stock.CurrentPrice * p.NumberOfShares;
            }
            if (stockPortfolio.StockPurchases.Count != 0)
            {
                stockPortfolio.StockPortionValue = 0;
                foreach (var v in stockPortfolio.StockPurchases)
                {
                    stockPortfolio.StockPortionValue += v.TotalStockValue;
                    v.ChangeInPrice = v.Stock.CurrentPrice - v.InitialSharePrice;
                    v.TotalChange = v.TotalStockValue - (v.NumberOfShares * v.InitialSharePrice);
                    v.StockPurchaseDisplay = v.Stock.StockName + ", Current Price: " + v.Stock.CurrentPrice.ToString("c") + ", Number of shares: " + v.NumberOfShares;
                }
            }
            db.SaveChanges();

            StockPortfolioDetailsViewModel model = new StockPortfolioDetailsViewModel { StockPortfolioID = id, StockPortfolio = stockPortfolio, StockPurchases = stockPortfolio.StockPurchases, Transactions = stockPortfolio.Transactions };
            ViewBag.Count = stockPortfolio.Transactions.Count();
            ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
            return View(model);
        }

        //POST: StockPortfolio/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(StockPortfolioDetailsViewModel model, String TransactionNumber, String DateRange, String Description, String TransactionType, String PriceRange, String RangeFrom, String RangeTo, SortBy TransactionNumberSort, SortBy TransactionTypeSort, SortBy DescriptionSort, SortBy AmountSort, SortBy DateSort)
        {
            int? id = model.StockPortfolioID;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockPortfolio stockPortfolio = GetStockPortfolio();
            StockPortfolioDetailsViewModel modelToPass = new StockPortfolioDetailsViewModel { StockPortfolioID = id, StockPortfolio = stockPortfolio, StockPurchases = stockPortfolio.StockPurchases, Transactions = stockPortfolio.Transactions };
            if (stockPortfolio == null)
            {
                return HttpNotFound();
            }

            var query = from t in stockPortfolio.Transactions
                        select t;
            if (TransactionNumber != null && TransactionNumber != "")
            {
                Int32 number;
                try
                {
                    number = Convert.ToInt32(TransactionNumber);
                    query = query.Where(t => t.TransactionID.Equals(number));
                }
                catch
                {
                    ViewBag.TransactionNumberValidation = "Enter a whole number";
                    ViewBag.Count = stockPortfolio.Transactions.Count();
                    ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
                    return View(modelToPass);
                }
            }
            query = query.Where(t => t.Description.Contains(Description));
            if (!DateRange.Equals("Custom") && !DateRange.Equals("All"))
            {
                if (DateRange.Equals("Last 15 days")) { query = query.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-1)); }
                else if (DateRange.Equals("Last 30 days")) { query = query.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-30)); }
                else { query = query.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-60)); }
            }
            if (DateRange.Equals("Custom"))
            {
                query = query.Where(t => t.TransactionDate >= model.DateFrom && t.TransactionDate <= model.DateTo);
            }
            if (!TransactionType.Equals("All"))
            {
                query = query.Where(t => t.TransactionType.Equals(TransactionType));
            }
            if (!PriceRange.Equals("Custom"))
            {
                if (PriceRange.Equals("$0-$100")) { query = query.Where(t => t.Amount >= 0 && t.Amount <= 100); }
                else if (PriceRange.Equals("$100-$200")) { query = query.Where(t => t.Amount >= 100 && t.Amount <= 200); }
                else if (PriceRange.Equals("$200-$300")) { query = query.Where(t => t.Amount >= 200 && t.Amount <= 300); }
                else { query = query.Where(t => t.Amount >= 300); }
            }
            else
            {
                Decimal rangeFrom;
                Decimal rangeTo;
                try
                {
                    rangeFrom = Convert.ToDecimal(RangeFrom);
                    rangeTo = Convert.ToDecimal(RangeTo);
                    query = query.Where(t => t.Amount >= rangeFrom && t.Amount <= rangeTo);
                }
                catch
                {
                    ViewBag.Message = "Enter a valid range of numbers";
                    ViewBag.Count = stockPortfolio.Transactions.Count();
                    ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
                    return View(modelToPass);
                }
            }

            if (TransactionNumberSort.Equals(SortBy.Ascending))
            {
                if (TransactionTypeSort.Equals(SortBy.Ascending))
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
                else
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
            }
            else
            {
                if (TransactionTypeSort.Equals(SortBy.Ascending))
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
                else
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
            }

            query.OrderByDescending(t => t.TransactionID);
            List<Transaction> list = query.ToList();
            modelToPass = new StockPortfolioDetailsViewModel { StockPortfolioID = id, StockPortfolio = stockPortfolio, StockPurchases = stockPortfolio.StockPurchases, Transactions = list };
            ViewBag.Count = list.Count();
            ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
            return View(modelToPass);
        }

        // GET: StockPortfolios/Create
        public ActionResult Create()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            if (user.StockPortfolio != null)
            {
                return View("Error", new string[] { "You have already created a stock portfolio." });
            }
            else
            {
                return View();
            }
            
        }

        // POST: StockPortfolios/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StockPortfolioID")] StockPortfolio stockPortfolio)
        {
            if (ModelState.IsValid)
            {
                stockPortfolio.AccountNumber = Utility.AccountNumber;
                Utility.AccountNumber += 1;

                stockPortfolio.AccountName = "Longhorn Stock Portfolio";

                AppUser user = db.Users.Find(User.Identity.GetUserId());
                stockPortfolio.Customer = user;
                stockPortfolio.Customer.HasAccount = true;
                stockPortfolio.CashValueBalance = 0;
                stockPortfolio.StockPortionValue = 0;
                stockPortfolio.isBalanced = false;
                stockPortfolio.isPending = true;

                db.StockPortfolios.Add(stockPortfolio);
                db.SaveChanges();
                return RedirectToAction("StockPortfolioPendingMessage", "Account");
            }

            return View(stockPortfolio);
        }

        // GET: StockPortfolios/Edit/5
        public ActionResult Edit(int? id)
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                StockPortfolio stockPortfolio = db.StockPortfolios.Find(id);
                if (stockPortfolio == null)
                {
                    return HttpNotFound();
                }
                return View(stockPortfolio);
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        // POST: StockPortfolios/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StockPortfolio stockPortfolio)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stockPortfolio).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }
            return View(stockPortfolio);
        }

        //GET: StockPortfolios/Deposit
        public ActionResult Deposit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //POST: StockPortfolios/Deposit
        public ActionResult Deposit([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionType = "Deposit";
                transaction.isBeingDisputed = false;
                transaction.EmployeeComments = "";
                StockPortfolio StockPortfolioToChange = GetStockPortfolio();
                if (transaction.Amount > 5000)
                {
                    transaction.isPending = true;
                }
                else
                {
                    transaction.isPending = false;
                    StockPortfolioToChange.CashValueBalance += transaction.Amount;
                }
                if (transaction.Description == null)
                {
                    transaction.Description = "";
                }
                transaction.StockPortfolioAffected = StockPortfolioToChange;
                StockPortfolioToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }

            return View(transaction);
        }

        //GET: StockPortfolios/Withdrawal
        public ActionResult Withdrawal()
        {
            return View();
        }

        //POST: StockPortfolios/Withdrawal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdrawal([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionType = "Withdrawal";
                transaction.isBeingDisputed = false;
                transaction.EmployeeComments = "";
                transaction.isPending = false;
                if (transaction.Description == null)
                {
                    transaction.Description = "";
                }
                StockPortfolio StockPortfolioToChange = GetStockPortfolio();
                if (StockPortfolioToChange.CashValueBalance < 0)
                {
                    return View("Error", new string[] { "You can't withdraw from an overdrawn account." });
                }
                if (transaction.Amount > StockPortfolioToChange.CashValueBalance)
                {
                    return View("Error", new string[] { "You don't have enough funds to withdraw that much money." });
                }
                StockPortfolioToChange.CashValueBalance -= transaction.Amount;
                transaction.StockPortfolioAffected = StockPortfolioToChange;
                StockPortfolioToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }

            return View(transaction);
        }

        //GET: StockPortfolios/PurchaseStock
        public ActionResult PurchaseStock()
        {
            ViewBag.StockPortfolio = GetStockPortfolioWithBalance();
            ViewBag.Stocks = GetAvailableStocks();
            return View();
        }

        //POST: StockPortfolios/PurchaseStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PurchaseStock([Bind(Include = "StockPurchaseID,PurchaseDate,NumberOfShares")] StockPurchase stockPurchase, int StockPortfolioID, int StockID)
        {
            if (ModelState.IsValid)
            {
                StockPortfolio stockPortfolio = db.StockPortfolios.Find(StockPortfolioID);
                Stock stock = db.Stocks.Find(StockID);
                stockPurchase.StockPortfolio = stockPortfolio;
                stockPurchase.Stock = stock;
                stockPortfolio.StockPurchases.Add(stockPurchase);
                stock.StockPurchases.Add(stockPurchase);
                StockQuote quote = GetQuote.GetStock(stock.TickerSymbol, stockPurchase.PurchaseDate);
                Decimal stockPrice = quote.PreviousClose;

                stockPurchase.InitialSharePrice = stockPrice;
                stockPurchase.TotalStockValue = stockPrice * stockPurchase.NumberOfShares;
                stockPurchase.ChangeInPrice = stockPrice - stockPurchase.InitialSharePrice;
                stockPurchase.TotalChange = stockPurchase.TotalStockValue - (stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares);
                stockPurchase.StockPurchaseDisplay = stockPurchase.Stock.StockName + ", Current Price: " + stock.CurrentPrice.ToString("c") + ", Number of shares: " + stockPurchase.NumberOfShares;
                if (stockPortfolio.CashValueBalance < ((stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares) + stockPurchase.Stock.Fee))
                {
                    return View("Error", new string[] { "You do not have enough funds to make the purchase." });
                }
                
                stockPortfolio.CashValueBalance -= ((stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares) + stockPurchase.Stock.Fee);
                stockPortfolio.StockPortionValue += (stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares);

                Transaction StockTransaction = new Transaction();
                StockTransaction.Amount = stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares;
                StockTransaction.StockPortfolioAffected = stockPortfolio;
                StockTransaction.Description = "Stock Purchase - Account " + stockPortfolio.AccountNumber.ToString();
                StockTransaction.TransactionDate = stockPurchase.PurchaseDate;
                StockTransaction.TransactionType = "Withdrawal";
                StockTransaction.isBeingDisputed = false;
                StockTransaction.isPending = false;
                StockTransaction.EmployeeComments = "";

                Transaction TransactionFee = new Transaction();
                TransactionFee.Amount = stockPurchase.Stock.Fee;
                TransactionFee.StockPortfolioAffected = stockPortfolio;
                TransactionFee.Description = "Fee for purchase of " + stockPurchase.Stock.StockName;
                TransactionFee.TransactionDate = stockPurchase.PurchaseDate;
                TransactionFee.TransactionType = "Fee";
                TransactionFee.isBeingDisputed = false;
                TransactionFee.isPending = false;
                TransactionFee.EmployeeComments = "";

                int OrdinaryCount = 0;
                int IndexCount = 0;
                int MutalCount = 0;
                foreach (var s in stockPortfolio.StockPurchases)
                {
                    if (s.Stock.StockType.Equals("Ordinary Stock"))
                    {
                        OrdinaryCount += 1;
                    }
                    else if (s.Stock.StockType.Equals("Index Fund"))
                    {
                        IndexCount += 1;
                    }
                    else if (s.Stock.StockType.Equals("Mutual Fund"))
                    {
                        MutalCount += 1;
                    }
                }
                if (OrdinaryCount >= 2 && IndexCount >= 1 && MutalCount >= 1)
                {
                    stockPortfolio.isBalanced = true;
                }
                else { stockPortfolio.isBalanced = false; }

                stockPortfolio.Transactions.Add(StockTransaction);
                stockPortfolio.Transactions.Add(TransactionFee);
                db.Transactions.Add(StockTransaction);
                db.Transactions.Add(TransactionFee);
                db.StockPurchases.Add(stockPurchase);
                db.SaveChanges();
                return RedirectToAction("PurchaseStockConfirmation");
            }

            ViewBag.StockPortfolio = GetStockPortfolioWithBalance();
            ViewBag.Stocks = GetAvailableStocks();
            return View(stockPurchase);
        }

        //GET: StockPortfolios/SellStock
        public ActionResult SellStock()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            StockPortfolio stockPortfolio = user.StockPortfolio;
            SellStockViewModel model = new SellStockViewModel { stockPortfolioID = stockPortfolio.StockPortfolioID };
            ViewBag.CustomerStockPurchases = GetCustomerStockPurchases();
            return View(model);
        }

        //POST: StockPortfolios/SellStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SellStock(SellStockViewModel model, int StockPurchaseID)
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            StockPortfolio stockPortfolio = user.StockPortfolio;
            model.stockPortfolioID = stockPortfolio.StockPortfolioID;
            if (ModelState.IsValid)
            {
                StockPurchase stockPurchase = db.StockPurchases.Find(StockPurchaseID);
                if (model.NumberOfSharesToSell > stockPurchase.NumberOfShares) { return View("Error", new string[] { "You can't sell more shares than what you have." }); }
                if (model.NumberOfSharesToSell == 0) { return View("Error", new string[] { "Why are you trying to sell 0 shares...?" }); }
                if (model.NumberOfSharesToSell < 0) { return View("Error", new string[] { "You can't sell a negative number of shares." }); }

                model.stockPurchaseID = StockPurchaseID;
                model.StockName = stockPurchase.Stock.StockName;
                model.SellDate = DateTime.Now;
                model.NumberOfSharesToSell = model.NumberOfSharesToSell;
                model.NumberOfSharesRemaining = stockPurchase.NumberOfShares - model.NumberOfSharesToSell;
                model.Fees = stockPurchase.Stock.Fee;
                model.NetGainLoss = (stockPurchase.Stock.CurrentPrice * model.NumberOfSharesToSell) - (stockPurchase.InitialSharePrice * model.NumberOfSharesToSell);
                return RedirectToAction("SellStockSummaryPage", model);
            }

            ViewBag.CustomerStockPurchases = GetCustomerStockPurchases();
            return View(model);
        }

        //GET: StockPortfolios/SellStockSummaryPage
        public ActionResult SellStockSummaryPage(SellStockViewModel SellInfo)
        {
            return View(SellInfo);
        }

        //POST: StockPortfolios/SellStockSummaryPage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SellStockSummaryPage(SellStockViewModel SellInfo, SellStockDecision decision)
        {
            switch (decision)
            {
                case SellStockDecision.Cancel:
                    {
                        return RedirectToAction("Details", new { id = SellInfo.stockPortfolioID });
                    }
                case SellStockDecision.Confirm:
                    {
                        StockPortfolio stockPortfolio = db.StockPortfolios.Find(SellInfo.stockPortfolioID);
                        StockPurchase stockPurchase = db.StockPurchases.Find(SellInfo.stockPurchaseID);
                        stockPurchase.NumberOfShares = SellInfo.NumberOfSharesRemaining;
                        stockPurchase.TotalStockValue = SellInfo.NumberOfSharesRemaining * stockPurchase.Stock.CurrentPrice;
                        stockPurchase.TotalChange = (SellInfo.NumberOfSharesRemaining * stockPurchase.Stock.CurrentPrice) - (SellInfo.NumberOfSharesRemaining * stockPurchase.InitialSharePrice);
                        stockPurchase.ChangeInPrice = stockPurchase.Stock.CurrentPrice - stockPurchase.InitialSharePrice;
                        stockPurchase.StockPurchaseDisplay = stockPurchase.Stock.StockName + ", Current Price: " + stockPurchase.Stock.CurrentPrice.ToString("c") + ", Number of shares: " + stockPurchase.NumberOfShares.ToString();

                        stockPortfolio.CashValueBalance += ((SellInfo.NumberOfSharesToSell * stockPurchase.Stock.CurrentPrice) - SellInfo.Fees);
                        stockPortfolio.StockPortionValue -= SellInfo.NumberOfSharesToSell * stockPurchase.Stock.CurrentPrice;

                        Transaction StockTransaction = new Transaction();
                        StockTransaction.Amount = stockPurchase.Stock.CurrentPrice * SellInfo.NumberOfSharesToSell;
                        StockTransaction.StockPortfolioAffected = stockPortfolio;
                        StockTransaction.Description = "Sell Stock - Stock: " + SellInfo.StockName + ", Number of shares sold: " + SellInfo.NumberOfSharesToSell + ", Initial Share price: " + stockPurchase.InitialSharePrice.ToString("c") + ", Current Share Price: " + stockPurchase.Stock.CurrentPrice.ToString("c") + ", Total Gains/Loss: " + SellInfo.NetGainLoss.ToString("c");
                        StockTransaction.TransactionDate = SellInfo.SellDate;
                        StockTransaction.TransactionType = "Deposit";
                        StockTransaction.isBeingDisputed = false;
                        StockTransaction.isPending = false;
                        StockTransaction.EmployeeComments = "";

                        Transaction TransactionFee = new Transaction();
                        TransactionFee.Amount = SellInfo.Fees;
                        TransactionFee.StockPortfolioAffected = stockPortfolio;
                        TransactionFee.Description = "Fee for sale of " + stockPurchase.Stock.StockName;
                        TransactionFee.TransactionDate = SellInfo.SellDate;
                        TransactionFee.TransactionType = "Fee";
                        TransactionFee.isBeingDisputed = false;
                        TransactionFee.isPending = false;
                        TransactionFee.EmployeeComments = "";

                        int OrdinaryCount = 0;
                        int IndexCount = 0;
                        int MutalCount = 0;
                        foreach (var s in stockPortfolio.StockPurchases)
                        {
                            if (s.Stock.StockType.Equals("Ordinary Stock"))
                            {
                                OrdinaryCount += 1;
                            }
                            else if (s.Stock.StockType.Equals("Index Fund"))
                            {
                                IndexCount += 1;
                            }
                            else if (s.Stock.StockType.Equals("Mutual Fund"))
                            {
                                MutalCount += 1;
                            }
                        }
                        if (OrdinaryCount >= 2 && IndexCount >= 1 && MutalCount >= 1)
                        {
                            stockPortfolio.isBalanced = true;
                        }
                        else { stockPortfolio.isBalanced = false; }

                        stockPortfolio.Transactions.Add(StockTransaction);
                        stockPortfolio.Transactions.Add(TransactionFee);
                        db.Transactions.Add(StockTransaction);
                        db.Transactions.Add(TransactionFee);
                        if (stockPurchase.NumberOfShares == 0)
                        {
                            db.StockPurchases.Remove(stockPurchase);
                        }
                        db.SaveChanges();

                        return RedirectToAction("SellStockConfirmation");
                    }
            }
            return View(SellInfo);
        }

        public ActionResult PurchaseStockConfirmation()
        {
            ViewBag.id = GetStockPortfolio().StockPortfolioID;
            return View("PurchaseStockConfirmation");
        }

        public ActionResult SellStockConfirmation()
        {
            ViewBag.id = GetStockPortfolio().StockPortfolioID;
            return View("SellStockConfirmation");
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ApproveStockPortfolio(int? id)
        {
            StockPortfolio stockPortfolio = db.StockPortfolios.Find(id);
            stockPortfolio.isPending = false;
            db.SaveChanges();
            return RedirectToAction("EmployeePortal", "Manage");
        }

        // GET: StockPortfolios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockPortfolio stockPortfolio = db.StockPortfolios.Find(id);
            if (stockPortfolio == null)
            {
                return HttpNotFound();
            }
            return View(stockPortfolio);
        }

        // POST: StockPortfolios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StockPortfolio stockPortfolio = db.StockPortfolios.Find(id);
            db.StockPortfolios.Remove(stockPortfolio);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ProcessStockPortfolios()
        {
            foreach (var s in db.StockPortfolios)
            {
                if (s.isBalanced)
                {
                    Decimal Bonus = s.StockPortionValue * .1m;
                    s.CashValueBalance += Bonus;

                    Transaction transaction = new Transaction();
                    transaction.Amount = Bonus;
                    transaction.StockPortfolioAffected = s;
                    transaction.Description = "Balanced Portfolio Bonus";
                    transaction.TransactionDate = DateTime.Now;
                    transaction.TransactionType = "Bonus";
                    transaction.isBeingDisputed = false;
                    transaction.isPending = false;
                    transaction.EmployeeComments = "";

                    s.Transactions.Add(transaction);
                    db.Transactions.Add(transaction);
                }
            }

            db.SaveChanges();
            return RedirectToAction("ProcessStockPortfoliosConfirmationMessage", "StockPortfolios");
        }

        public ActionResult ProcessStockPortfoliosConfirmationMessage()
        {
            return View();
        }

        public StockPortfolio GetStockPortfolio()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            StockPortfolio stockPortfolio = user.StockPortfolio;
            return stockPortfolio;
        }

        public SelectList GetStockPortfolioWithBalance()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());

            List<StockPortfolio> StockPortfolio = new List<StockPortfolio>();
            StockPortfolio.Add(user.StockPortfolio);

            SelectList list = new SelectList(StockPortfolio, "StockPortfolioID", "StockPortfolioDisplay");
            return list;
        }

        public SelectList GetAvailableStocks()
        {
            foreach (var s in db.Stocks)
            {
                s.CurrentPrice = GetQuote.GetStock(s.TickerSymbol).PreviousClose;
            }
            db.SaveChanges();

            SelectList list = new SelectList(db.Stocks.ToList(), "StockID", "StockDescription");
            return list;
        }

        public SelectList GetCustomerStockPurchases()
        {
            foreach (var s in db.Stocks)
            {
                s.CurrentPrice = GetQuote.GetStock(s.TickerSymbol).PreviousClose;
            }
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            StockPortfolio stockPortfolio = customer.StockPortfolio;
            foreach (var c in stockPortfolio.StockPurchases)
            {
                c.StockPurchaseDisplay = c.Stock.StockName + ", Current Price: " + c.Stock.CurrentPrice.ToString("c") + ", Number of shares: " + c.NumberOfShares;
            }
            db.SaveChanges();
            SelectList list = new SelectList(stockPortfolio.StockPurchases, "StockPurchaseID", "StockPurchaseDisplay");
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
