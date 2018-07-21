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
    [Authorize]
    public class TransactionsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Transactions
        public ActionResult Index()
        {
            return View(db.Transactions.ToList());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            if (transaction.CheckingAccountAffected != null)
            {
                CheckingAccount checkingAccount = transaction.CheckingAccountAffected;
                var query = from t in checkingAccount.Transactions
                            where t.TransactionType.Equals(transaction.TransactionType)
                            select t;
                query.OrderBy(t => t.TransactionDate);
                List<Transaction> prelist = query.ToList();
                List<Transaction> list = new List<Transaction>();
                if (prelist.Count() == 1) { list.Add(prelist[0]); }
                else if (prelist.Count() == 2) { list.Add(prelist[0]); list.Add(prelist[1]); }
                else if (prelist.Count() == 3) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); }
                else if (prelist.Count() == 4) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); }
                else { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); list.Add(prelist[4]); }
                TransactionsDetailsViewModel model = new TransactionsDetailsViewModel { TransactionID = id, Transaction = transaction, FiveTransactions = list };
                return View(model);
            }
            else if (transaction.SavingsAccountAffected != null)
            {
                SavingsAccount savingsAccount = transaction.SavingsAccountAffected;
                var query = from t in savingsAccount.Transactions
                            where t.TransactionType.Equals(transaction.TransactionType)
                            select t;
                query.OrderBy(t => t.TransactionDate);
                List<Transaction> prelist = query.ToList();
                List<Transaction> list = new List<Transaction>();
                if (prelist.Count() == 1) { list.Add(prelist[0]); }
                else if (prelist.Count() == 2) { list.Add(prelist[0]); list.Add(prelist[1]); }
                else if (prelist.Count() == 3) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); }
                else if (prelist.Count() == 4) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); }
                else { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); list.Add(prelist[4]); }
                TransactionsDetailsViewModel model = new TransactionsDetailsViewModel { TransactionID = id, Transaction = transaction, FiveTransactions = list };
                return View(model);
            }
            else if (transaction.IRAccountAffected != null)
            {
                IRAccount iRAccount = transaction.IRAccountAffected;
                var query = from t in iRAccount.Transactions
                            where t.TransactionType.Equals(transaction.TransactionType)
                            select t;
                query.OrderBy(t => t.TransactionDate);
                List<Transaction> prelist = query.ToList();
                List<Transaction> list = new List<Transaction>();
                if (prelist.Count() == 1) { list.Add(prelist[0]); }
                else if (prelist.Count() == 2) { list.Add(prelist[0]); list.Add(prelist[1]); }
                else if (prelist.Count() == 3) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); }
                else if (prelist.Count() == 4) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); }
                else { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); list.Add(prelist[4]); }
                TransactionsDetailsViewModel model = new TransactionsDetailsViewModel { TransactionID = id, Transaction = transaction, FiveTransactions = list };
                return View(model);
            }
            else
            {
                StockPortfolio stockPortfolio = transaction.StockPortfolioAffected;
                var query = from t in stockPortfolio.Transactions
                            where t.TransactionType.Equals(transaction.TransactionType)
                            select t;
                query.OrderBy(t => t.TransactionDate);
                List<Transaction> prelist = query.ToList();
                List<Transaction> list = new List<Transaction>();
                if (prelist.Count() == 1) { list.Add(prelist[0]); }
                else if (prelist.Count() == 2) { list.Add(prelist[0]); list.Add(prelist[1]); }
                else if (prelist.Count() == 3) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); }
                else if (prelist.Count() == 4) { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); }
                else { list.Add(prelist[0]); list.Add(prelist[1]); list.Add(prelist[2]); list.Add(prelist[3]); list.Add(prelist[4]); }
                TransactionsDetailsViewModel model = new TransactionsDetailsViewModel { TransactionID = id, Transaction = transaction, FiveTransactions = list };
                return View(model);
            }
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TransactionID,TransactionType,TransactionDate,Amount,Description,isPending,isBeingDisputed,EmployeeComments")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TransactionID,TransactionType,TransactionDate,Amount,Description,isPending,isBeingDisputed,EmployeeComments")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transaction);
        }

        [Authorize(Roles = "Manager")]
        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Transactions/Deposit
        public ActionResult Deposit()
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                ViewBag.AccountTypes = new SelectList(Utility.AccountTypes);
                return View();
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        // POST: Transactions/Deposit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit(String AccountType)
        {
            if (AccountType == "Checking Account")
            {
                return RedirectToAction("Deposit", "CheckingAccounts");
            }
            else if (AccountType == "Savings Account")
            {
                return RedirectToAction("Deposit", "SavingsAccounts");
            }
            else if (AccountType == "IRA")
            {
                return RedirectToAction("Deposit", "IRAccounts");
            }
            else
            {
                return RedirectToAction("Deposit", "StockPortfolios");
            }
        }

        // GET: Transactions/Withdrawal
        public ActionResult Withdrawal()
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                ViewBag.AccountTypes = new SelectList(Utility.AccountTypes);
                return View();
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        // POST: Transactions/Withdrawal
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdrawal(String AccountType)
        {
            if (AccountType == "Checking Account")
            {
                return RedirectToAction("Withdrawal", "CheckingAccounts");
            }
            else if (AccountType == "Savings Account")
            {
                return RedirectToAction("Withdrawal", "SavingsAccounts");
            }
            else if (AccountType == "IRA")
            {
                return RedirectToAction("Withdrawal", "IRAccounts");
            }
            else
            {
                return RedirectToAction("Withdrawal", "StockPortfolios");
            }
        }

        [Authorize(Roles = "Manager")]
        public ActionResult ApproveTransaction(int? id)
        {
            Transaction transaction = db.Transactions.Find(id);
            if (transaction.CheckingAccountAffected != null)
            {
                EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Transaction Approved", "The manager has approved your transaction. ");
                CheckingAccount CheckingAccountToAffect = transaction.CheckingAccountAffected;
                CheckingAccountToAffect.Balance += transaction.Amount;
                transaction.isPending = false;
                db.SaveChanges();
                return RedirectToAction("EmployeePortal", "Manage");
            }
            else if (transaction.SavingsAccountAffected != null)
            {
                EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Transaction Approved", "The manager has approved your transaction. ");
                SavingsAccount SavingsAccountToAffect = transaction.SavingsAccountAffected;
                SavingsAccountToAffect.Balance += transaction.Amount;
                transaction.isPending = false;
                db.SaveChanges();
                return RedirectToAction("EmployeePortal", "Manage");
            }
            else if (transaction.IRAccountAffected != null)
            {
                EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Transaction Approved", "The manager has approved your transaction. ");
                IRAccount IRAccountToAffect = transaction.IRAccountAffected;
                IRAccountToAffect.Balance += transaction.Amount;
                transaction.isPending = false;
                db.SaveChanges();
                return RedirectToAction("EmployeePortal", "Manage");
            }
            else
            {
                EmailMessaging.SendEmail(transaction.StockPortfolioAffected.Customer.Email, "Transaction Approved", "The manager has approved your transaction. ");
                StockPortfolio StockPortfolioToAffect = transaction.StockPortfolioAffected;
                StockPortfolioToAffect.CashValueBalance += transaction.Amount;
                transaction.isPending = false;
                db.SaveChanges();
                return RedirectToAction("EmployeePortal", "Manage");
            }
        }

        //GET: Transactions/PurchaseStock
        public ActionResult PurchaseStock()
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            int id = customer.StockPortfolio.StockPortfolioID;
            ViewBag.id = id;

            List<String> AccountTypes = Utility.AccountTypes;
            AccountTypes.Remove("IRA");
            ViewBag.AccountTypes = new SelectList(AccountTypes);

            return View();
        }

        //POST: Transactions/PurchaseStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PurchaseStock(String AccountType)
        {
            if (AccountType == "Checking Account")
            {
                return RedirectToAction("PurchaseStock", "CheckingAccounts");
            }
            else if (AccountType == "Savings Account")
            {
                return RedirectToAction("PurchaseStock", "SavingsAccounts");
            }
            else
            {
                return RedirectToAction("PurchaseStock", "StockPortfolios");
            }
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
