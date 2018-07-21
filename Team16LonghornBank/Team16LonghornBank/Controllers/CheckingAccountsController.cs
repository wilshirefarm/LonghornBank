using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Team16LonghornBank.Models;
using Team16LonghornBank.Utilities;
using Microsoft.AspNet.Identity;

namespace Team16LonghornBank.Controllers
{
    public enum SortBy { Ascending, Descending }

    [Authorize]
    public class CheckingAccountsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: CheckingAccounts
        public ActionResult Index()
        {
            return View(db.CheckingAccounts.ToList());
        }

        // GET: CheckingAccounts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CheckingAccount checkingAccount = db.CheckingAccounts.Find(id);
            if (checkingAccount == null)
            {
                return HttpNotFound();
            }
            CheckingAccountDetailsViewModel model = new CheckingAccountDetailsViewModel { CheckingAccountID = id, CheckingAccount = checkingAccount, Transactions = checkingAccount.Transactions };
            ViewBag.Count = checkingAccount.Transactions.Count();
            ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
            return View(model);
        }

        //POST: CheckingAccounts/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(CheckingAccountDetailsViewModel model, String TransactionNumber, String DateRange, String Description, String TransactionType, String PriceRange, String RangeFrom, String RangeTo, SortBy TransactionNumberSort, SortBy TransactionTypeSort, SortBy DescriptionSort, SortBy AmountSort, SortBy DateSort)
        {
            int? id = model.CheckingAccountID;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CheckingAccount checkingAccount = db.CheckingAccounts.Find(id);
            CheckingAccountDetailsViewModel modelToPass = new CheckingAccountDetailsViewModel { CheckingAccountID = id, CheckingAccount = checkingAccount, Transactions = checkingAccount.Transactions };
            if (checkingAccount == null)
            {
                return HttpNotFound();
            }

            var query = from t in checkingAccount.Transactions
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
                    ViewBag.Count = checkingAccount.Transactions.Count();
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
                    ViewBag.Count = checkingAccount.Transactions.Count();
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
            modelToPass = new CheckingAccountDetailsViewModel { CheckingAccountID = id, CheckingAccount = checkingAccount, Transactions = list };
            ViewBag.Count = list.Count();
            ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
            return View(modelToPass);
        }

        // GET: CheckingAccounts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CheckingAccounts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CheckingAccountID,Balance")] CheckingAccount checkingAccount)
        {
            if (ModelState.IsValid)
            {
                checkingAccount.AccountNumber = Utility.AccountNumber;
                Utility.AccountNumber += 1;

                checkingAccount.AccountName = "Longhorn Checking";
                checkingAccount.Transactions = new List<Transaction>();
                AppUser user = db.Users.Find(User.Identity.GetUserId());
                checkingAccount.Customer = user;
                checkingAccount.Customer.HasAccount = true;
                if (checkingAccount.Balance != 0)
                {
                    Transaction transaction = new Transaction();
                    transaction.CheckingAccountAffected = checkingAccount;
                    transaction.TransactionType = "Deposit";
                    transaction.TransactionDate = DateTime.Now;
                    transaction.Amount = checkingAccount.Balance;
                    transaction.Description = "Initial Deposit into " + checkingAccount.AccountNumber;
                    transaction.isBeingDisputed = false;
                    transaction.EmployeeComments = "";
                    if (checkingAccount.Balance > 5000)
                    {
                        transaction.isPending = true;
                        checkingAccount.Balance = 0;
                    }
                    else
                    {
                        transaction.isPending = false;
                    }
                    db.Transactions.Add(transaction);
                    checkingAccount.Transactions.Add(transaction);
                }
                
                db.CheckingAccounts.Add(checkingAccount);
                db.SaveChanges();
                return RedirectToAction("BankAccountCreatedConfirmation", "Account");
            }

            return View(checkingAccount);
        }

        // GET: CheckingAccounts/Edit/5
        public ActionResult Edit(int? id)
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                CheckingAccount checkingAccount = db.CheckingAccounts.Find(id);
                if (checkingAccount == null)
                {
                    return HttpNotFound();
                }
                return View(checkingAccount);
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        // POST: CheckingAccounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CheckingAccount checkingAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(checkingAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }
            return View(checkingAccount);
        }

        // GET: CheckingAccounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CheckingAccount checkingAccount = db.CheckingAccounts.Find(id);
            if (checkingAccount == null)
            {
                return HttpNotFound();
            }
            return View(checkingAccount);
        }

        // POST: CheckingAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CheckingAccount checkingAccount = db.CheckingAccounts.Find(id);
            db.CheckingAccounts.Remove(checkingAccount);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: CheckingAccounts/Deposit
        public ActionResult Deposit()
        {
            ViewBag.CheckingAccounts = GetCheckingAccounts();
            return View();
        }

        // POST: CheckingAccounts/Deposit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction, int CheckingAccountID)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionType = "Deposit";
                transaction.isBeingDisputed = false;
                transaction.EmployeeComments = "";
                CheckingAccount CheckingAccountToChange = db.CheckingAccounts.Find(CheckingAccountID);
                if (transaction.Amount > 5000)
                {
                    transaction.isPending = true;
                }
                else
                {
                    transaction.isPending = false;
                    CheckingAccountToChange.Balance += transaction.Amount;
                }
                if (transaction.Description == null)
                {
                    transaction.Description = "";
                }
                transaction.CheckingAccountAffected = CheckingAccountToChange;
                CheckingAccountToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }

            ViewBag.CheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }

        // GET: CheckingAccounts/Withdrawal
        public ActionResult Withdrawal()
        {
            ViewBag.CheckingAccounts = GetCheckingAccounts();
            return View();
        }

        // POST: CheckingAccounts/Withdrawal
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdrawal([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction, int CheckingAccountID)
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
                CheckingAccount CheckingAccountToChange = db.CheckingAccounts.Find(CheckingAccountID);
                if (CheckingAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You can't withdraw from an overdrawn account." });
                }
                if (transaction.Amount > CheckingAccountToChange.Balance)
                {
                    return View("Error", new string[] { "You don't have enough funds to withdraw that much money." });
                }
                CheckingAccountToChange.Balance -= transaction.Amount;
                transaction.CheckingAccountAffected = CheckingAccountToChange;
                CheckingAccountToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }

            ViewBag.CheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }

        //GET: CheckingAccounts/PurchaseStock
        public ActionResult PurchaseStock()
        {
            ViewBag.CheckingAccounts = GetCheckingAccountsWithBalance();
            ViewBag.Stocks = GetAvailableStocks();
            return View();
        }

        // POST: CheckingAccounts/PurchaseStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PurchaseStock([Bind(Include = "StockPurchaseID,PurchaseDate,NumberOfShares")] StockPurchase stockPurchase, int CheckingAccountID, int StockID)
        {
            if (ModelState.IsValid)
            {
                AppUser user = db.Users.Find(User.Identity.GetUserId());
                StockPortfolio stockPortfolio = user.StockPortfolio;
                CheckingAccount checkingAccount = db.CheckingAccounts.Find(CheckingAccountID);
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
                stockPurchase.StockPurchaseDisplay = stockPurchase.Stock.StockName + ", Current Price: $" + stock.CurrentPrice.ToString("c") + ", Number of shares: " + stockPurchase.NumberOfShares.ToString();
                if (checkingAccount.Balance < ((stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares) + stockPurchase.Stock.Fee))
                {
                    return View("Error", new string[] { "You do not have enough funds to make the purchase." });
                }

                checkingAccount.Balance -= ((stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares) + stockPurchase.Stock.Fee);
                stockPortfolio.StockPortionValue += (stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares);

                Transaction StockTransaction = new Transaction();
                StockTransaction.Amount = stockPurchase.InitialSharePrice * stockPurchase.NumberOfShares;
                StockTransaction.CheckingAccountAffected = checkingAccount;
                StockTransaction.Description = "Stock Purchase - Account " + stockPortfolio.AccountNumber.ToString();
                StockTransaction.TransactionDate = stockPurchase.PurchaseDate;
                StockTransaction.TransactionType = "Withdrawal";
                StockTransaction.isBeingDisputed = false;
                StockTransaction.isPending = false;
                StockTransaction.EmployeeComments = "";

                Transaction TransactionFee = new Transaction();
                TransactionFee.Amount = stockPurchase.Stock.Fee;
                TransactionFee.CheckingAccountAffected = checkingAccount;
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

                checkingAccount.Transactions.Add(StockTransaction);
                checkingAccount.Transactions.Add(TransactionFee);
                db.Transactions.Add(StockTransaction);
                db.Transactions.Add(TransactionFee);
                db.StockPurchases.Add(stockPurchase);
                db.SaveChanges();
                return RedirectToAction("PurchaseStockConfirmation", "StockPortfolios");
            }

            ViewBag.CheckingAccounts = GetCheckingAccountsWithBalance();
            ViewBag.Stocks = GetAvailableStocks();
            return View(stockPurchase);
        }

        // GET: CheckingAccounts/ChooseAnAccount
        public ActionResult ChooseAnAccount()
        {
            ViewBag.CheckingAccounts = GetCheckingAccounts();
            return View();
        }

        // POST: CheckingAccounts/ChooseAnAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseAnAccount(int CheckingAccountID)
        {
            return View();
        }

        public SelectList GetCheckingAccounts()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<CheckingAccount> CheckingAccounts = user.CheckingAccounts;
            SelectList list = new SelectList(CheckingAccounts, "CheckingAccountID", "AccountName");
            return list;
        }

        public SelectList GetCheckingAccountsWithBalance()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<CheckingAccount> CheckingAccounts = user.CheckingAccounts;
            SelectList list = new SelectList(CheckingAccounts, "CheckingAccountID", "CheckingAccountDisplay");
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
