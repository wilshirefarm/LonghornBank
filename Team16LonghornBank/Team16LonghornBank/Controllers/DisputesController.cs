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

namespace Team16LonghornBank.Controllers
{
    public class DisputesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Page of all disputes
        public ActionResult Index()
        {
            var DisputesQuery = from d in db.Disputes.Include(d => d.Transaction)
                                where d.CurrentStatus.Equals("Submitted")
                                select d;
            List<Dispute> DisputesList = DisputesQuery.ToList();
            return View(DisputesList);
        }

        public ActionResult SeeAll()
        {
            var disputes = db.Disputes.Include(d => d.Transaction);
            return View(disputes.ToList());
        }

        // GET: Disputes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            return View(dispute);
        }

        // GET: Disputes/Create
        public ActionResult Create(int? id)
        {
            Transaction transaction = db.Transactions.Find(id);
            List<Transaction> TransactionToDisplay = new List<Transaction>();
            TransactionToDisplay.Add(transaction);
            ViewBag.DisputeID = new SelectList(TransactionToDisplay, "TransactionID", "Description");
            return View();
        }

        // POST: Disputes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DisputeID,DisputeComments,CorrectAmount,DeleteTransaction")] Dispute dispute)
        {
            if (ModelState.IsValid)
            {
                Transaction TransactionToLink = db.Transactions.Find(dispute.DisputeID);
                TransactionToLink.isBeingDisputed = true;
                dispute.Transaction = TransactionToLink;
                dispute.CurrentStatus = "Submitted";
                db.Disputes.Add(dispute);
                db.SaveChanges();
                return RedirectToAction("CreateDisputeConfirmation", "Disputes");
            }

            Transaction transaction = db.Transactions.Find(dispute.DisputeID);
            List<Transaction> TransactionToDisplay = new List<Transaction>();
            TransactionToDisplay.Add(transaction);
            ViewBag.DisputeID = new SelectList(TransactionToDisplay, "TransactionID", "Description");
            return View(dispute);
        }    

        public ActionResult CreateDisputeConfirmation()
        {
            return View();
        }

        public ActionResult AdjustADispute(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            return View(dispute);
        }
        
        public ActionResult AdjustDispute(int? id)
        {
            Transaction transaction = db.Transactions.Find(id);
            if (transaction.CheckingAccountAffected != null)
            {
                EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Dispute Adjusted", "The manager has adjusted your dispute. ");
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    transaction.CheckingAccountAffected.Balance += transaction.Amount;
                    transaction.CheckingAccountAffected.Balance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    transaction.CheckingAccountAffected.Balance -= transaction.Amount;
                    transaction.CheckingAccountAffected.Balance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else if (transaction.SavingsAccountAffected != null)
            {
                EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Dispute Adjusted", "The manager has adjusted your dispute. ");
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    transaction.SavingsAccountAffected.Balance += transaction.Amount;
                    transaction.SavingsAccountAffected.Balance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    transaction.SavingsAccountAffected.Balance -= transaction.Amount;
                    transaction.SavingsAccountAffected.Balance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else if (transaction.IRAccountAffected != null)
            {
                EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Dispute Adjusted", "The manager has adjusted your dispute. ");
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    transaction.IRAccountAffected.Balance += transaction.Amount;
                    transaction.IRAccountAffected.Balance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    transaction.IRAccountAffected.Balance -= transaction.Amount;
                    transaction.IRAccountAffected.Balance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else if (transaction.StockPortfolioAffected != null)
            {
                EmailMessaging.SendEmail(transaction.StockPortfolioAffected.Customer.Email, "Dispute Adjusted", "The manager has adjusted your dispute. ");

                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    transaction.StockPortfolioAffected.CashValueBalance += transaction.Amount;
                    transaction.StockPortfolioAffected.CashValueBalance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    transaction.StockPortfolioAffected.CashValueBalance -= transaction.Amount;
                    transaction.StockPortfolioAffected.CashValueBalance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else if (transaction.TransferToCheckingAccount != null)
            {
                EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Dispute Adjusted", "The manager has adjusted your dispute. ");
                CheckingAccount CheckingAccountToAffect = transaction.TransferToCheckingAccount;
                CheckingAccountToAffect.Balance -= transaction.Amount;
                CheckingAccountToAffect.Balance += transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferFromCheckingAccount != null)
            {
                CheckingAccount CheckingAccountToAffect = transaction.TransferFromCheckingAccount;
                CheckingAccountToAffect.Balance += transaction.Amount;
                CheckingAccountToAffect.Balance -= transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferToSavingsAccount != null)
            {
                SavingsAccount SavingsAccountToAffect = transaction.TransferToSavingsAccount;
                SavingsAccountToAffect.Balance -= transaction.Amount;
                SavingsAccountToAffect.Balance += transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferFromSavingsAccount != null)
            {
                SavingsAccount SavingsAccountToAffect = transaction.TransferFromSavingsAccount;
                SavingsAccountToAffect.Balance += transaction.Amount;
                SavingsAccountToAffect.Balance -= transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferToIRAccount != null)
            {
                IRAccount IRAccountToAffect = transaction.TransferToIRAccount;
                IRAccountToAffect.Balance -= transaction.Amount;
                IRAccountToAffect.Balance += transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferFromIRAccount != null)
            {
                IRAccount IRAccountToAffect = transaction.TransferFromIRAccount;
                IRAccountToAffect.Balance += transaction.Amount;
                IRAccountToAffect.Balance -= transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferFromStockPortfolio != null)
            {
                StockPortfolio StockPortfolioToAffect = transaction.TransferFromStockPortfolio;
                StockPortfolioToAffect.CashValueBalance += transaction.Amount;
                StockPortfolioToAffect.CashValueBalance -= transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }
            else if (transaction.TransferToStockPortfolio != null)
            {
                StockPortfolio StockPortfolioToAffect = transaction.TransferToStockPortfolio;
                StockPortfolioToAffect.CashValueBalance -= transaction.Amount;
                StockPortfolioToAffect.CashValueBalance += transaction.Dispute.CorrectAmount;
                transaction.Amount = transaction.Dispute.CorrectAmount;
                transaction.Dispute.CurrentStatus = "Accepted";
                db.SaveChanges();
                return RedirectToAction("ResolveDisputeConfirmed");
            }

            return RedirectToAction("EmployeePortal", "Manage");
        }

        //GET: resolve dispute
        public ActionResult ResolveDispute(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            return View(dispute);
        }


        public ActionResult RejectDispute(int? id)
        {
            Transaction transaction = db.Transactions.Find(id);
            transaction.Dispute.CurrentStatus = "Rejected";
            db.SaveChanges();
            return RedirectToAction("ResolveDisputeConfirmation");
        }

        public ActionResult ResolveDisputeConfirmation()
        {
            return View();
        }

        //POST: Accept dispute
        public ActionResult AcceptDispute(int? id)
        {
            Transaction transaction = db.Transactions.Find(id);
            if (transaction.CheckingAccountAffected != null)
            {
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.CheckingAccountAffected.Balance += transaction.Amount;
                    transaction.CheckingAccountAffected.Balance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.CheckingAccountAffected.Balance -= transaction.Amount;
                    transaction.CheckingAccountAffected.Balance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else if (transaction.SavingsAccountAffected != null)
            {
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.SavingsAccountAffected.Balance += transaction.Amount;
                    transaction.SavingsAccountAffected.Balance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.SavingsAccountAffected.Balance -= transaction.Amount;
                    transaction.SavingsAccountAffected.Balance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else if (transaction.IRAccountAffected != null)
            {
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.IRAccountAffected.Balance += transaction.Amount;
                    transaction.IRAccountAffected.Balance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.IRAccountAffected.Balance -= transaction.Amount;
                    transaction.IRAccountAffected.Balance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            else
            {
                if (transaction.TransactionType == "Withdrawal" || transaction.TransactionType == "Bill Payment" || transaction.TransactionType == "Fee")
                {
                    EmailMessaging.SendEmail(transaction.StockPortfolioAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.StockPortfolioAffected.CashValueBalance += transaction.Amount;
                    transaction.StockPortfolioAffected.CashValueBalance -= transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
                else if (transaction.TransactionType == "Deposit" || transaction.TransactionType == "Bonus")
                {
                    EmailMessaging.SendEmail(transaction.StockPortfolioAffected.Customer.Email, "Dispute Accepted", "The manager has approved your dispute. ");
                    transaction.StockPortfolioAffected.CashValueBalance -= transaction.Amount;
                    transaction.StockPortfolioAffected.CashValueBalance += transaction.Dispute.CorrectAmount;
                    transaction.Amount = transaction.Dispute.CorrectAmount;
                    transaction.Dispute.CurrentStatus = "Accepted";
                    db.SaveChanges();
                    return RedirectToAction("ResolveDisputeConfirmation");
                }
            }
            
            return RedirectToAction("EmployeePortal", "Manage");
        }

        // GET: Disputes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            ViewBag.DisputeID = new SelectList(db.Transactions, "TransactionID", "TransactionType", dispute.DisputeID);
            return View(dispute);
        }

        // POST: Disputes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DisputeID,DisputeComments,CorrectAmount,DeleteTransaction,CurrentStatus")] Dispute dispute)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dispute).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.DisputeID = new SelectList(db.Transactions, "TransactionID", "TransactionType", dispute.DisputeID);
            return View(dispute);
        }

        // GET: Disputes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dispute dispute = db.Disputes.Find(id);
            if (dispute == null)
            {
                return HttpNotFound();
            }
            return View(dispute);
        }

        // POST: Disputes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Dispute dispute = db.Disputes.Find(id);
            db.Disputes.Remove(dispute);
            db.SaveChanges();
            return RedirectToAction("Index");
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
