using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Team16LonghornBank.Models;
using Team16LonghornBank.Utilities;

namespace Team16LonghornBank.Controllers
{
    public class PayeesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Index of all payees
        public ActionResult Index()
        {
            return View(db.Payees.ToList());
        }

        
        //GET list of all payees
        public SelectList GetAllPayees()
        {
            List<Payee> Payees = db.Payees.ToList();
            SelectList list = new SelectList(Payees, "PayeeID", "Name");
            return list;
        }

        // GET: Add Existing Payee
        [Authorize(Roles = "Customer")]
        public ActionResult AddExistingPayee()
        {
            ViewBag.Payees = GetAllPayees();
            return View();
        }

        // POST: Add Existing Payee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public ActionResult AddExistingPayee(int PayeeID)
        {
            if (ModelState.IsValid)
            {
                AppUser user = db.Users.Find(User.Identity.GetUserId());
                Payee payee = db.Payees.Find(PayeeID);
                user.Payees.Add(payee);
                db.SaveChanges();
                return RedirectToAction("PayeeCreatedConfirmation", "Payees");
            }

            ViewBag.Payees = GetAllPayees();
            return View();
        }

        // GET: Payees/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // GET: Payees/Create
        [Authorize(Roles = "Customer")]
        public ActionResult Create()
        {
            ViewBag.PayeeTypes = new SelectList(Utility.PayeeTypes);
            return View();
        }

        // POST: Payees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public ActionResult Create([Bind(Include = "PayeeID,Name,Address,City,State,ZipCode,PhoneNumber,PayeeType")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                AppUser user = db.Users.Find(User.Identity.GetUserId());
                user.Payees.Add(payee);
                db.Payees.Add(payee);
                db.SaveChanges();
                return RedirectToAction("PayeeCreatedConfirmation", "Payees");
            }

            return View(payee);
        }

        public ActionResult PayeeCreatedConfirmation()
        {
            return View();
        }

        // GET: Payees/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // POST: Payees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PayeeID,Name,Address,City,State,ZipCode,PhoneNumber,PayeeType")] Payee payee)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("PayBillsOnlineChecking", "Payees");
            }
            return View(payee);
        }

        // GET: Payees/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payee payee = db.Payees.Find(id);
            if (payee == null)
            {
                return HttpNotFound();
            }
            return View(payee);
        }

        // POST: Payees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payee payee = db.Payees.Find(id);
            db.Payees.Remove(payee);
            db.SaveChanges();
            return RedirectToAction("PayBillsOnlineChecking");
        }

        [Authorize(Roles = "Customer")]
        //GET
        public ActionResult PayBillsOnlineChecking()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<Payee> CustomerPayeeList = user.Payees.ToList();
            PayBillsOnlineViewModel model = new PayBillsOnlineViewModel { Customer = user, Payees = CustomerPayeeList };
            if (user.IsActive)
            {
                ViewBag.Payees = GetCheckingAccountsWithBalance();
                return View(model);
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayBillsOnlineChecking([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction, int CheckingAccountID)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionType = "Bill Payment";
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
                    return RedirectToAction("NegativeBalanceError");
                }

                if (transaction.Amount > CheckingAccountToChange.Balance)
                {
                    if ((transaction.Amount - CheckingAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        CheckingAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.CheckingAccountAffected = CheckingAccountToChange;
                        CheckingAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);

                        EmailMessaging.SendEmail(TransactionODFee.CheckingAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");



                    }


                }


                CheckingAccountToChange.Balance -= transaction.Amount;
                transaction.CheckingAccountAffected = CheckingAccountToChange;
                CheckingAccountToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("PaymentConfirmation", "Payees");
            }
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<Payee> CustomerPayeeList = user.Payees.ToList();
            PayBillsOnlineViewModel model = new PayBillsOnlineViewModel { Customer = user, Payees = CustomerPayeeList };

            ViewBag.Payees = GetCheckingAccountsWithBalance();
            return View(model);

        }

        //GET
        [Authorize(Roles = "Customer")]
        public ActionResult PayBillsOnlineSavings()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<Payee> CustomerPayeeList = user.Payees.ToList();
            PayBillsOnlineViewModel model = new PayBillsOnlineViewModel { Customer = user, Payees = CustomerPayeeList };
            if (user.IsActive)
            {
                ViewBag.Payees = GetSavingsAccountsWithBalance();
                return View(model);
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PayBillsOnlineSavings([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction, int SavingsAccountID)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionType = "Bill Payment";
                transaction.isBeingDisputed = false;
                transaction.EmployeeComments = "";
                transaction.isPending = false;
                if (transaction.Description == null)
                {
                    transaction.Description = "";
                }
                SavingsAccount SavingsAccountToChange = db.SavingsAccounts.Find(SavingsAccountID);

                if (SavingsAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > SavingsAccountToChange.Balance)
                {
                    if ((transaction.Amount - SavingsAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        SavingsAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.SavingsAccountAffected = SavingsAccountToChange;
                        SavingsAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);

                        EmailMessaging.SendEmail(TransactionODFee.SavingsAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");

                    }


                }

                SavingsAccountToChange.Balance -= transaction.Amount;
                transaction.SavingsAccountAffected = SavingsAccountToChange;
                SavingsAccountToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("PaymentConfirmation", "Payees");
            }
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<Payee> CustomerPayeeList = user.Payees.ToList();
            PayBillsOnlineViewModel model = new PayBillsOnlineViewModel { Customer = user, Payees = CustomerPayeeList };

            ViewBag.Payees = GetSavingsAccountsWithBalance();
            return View(model);

        }

        public ActionResult PaymentConfirmation()
        {
            return View();
        }

        public ActionResult NegativeBalanceError()
        {
            return View();
        }

        public SelectList GetCheckingAccountsWithBalance()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<CheckingAccount> CheckingAccounts = user.CheckingAccounts;
            SelectList list = new SelectList(CheckingAccounts, "CheckingAccountID", "CheckingAccountDisplay");
            return list;
        }

        public SelectList GetSavingsAccountsWithBalance()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<SavingsAccount> SavingsAccounts = user.SavingsAccounts;
            SelectList list = new SelectList(SavingsAccounts, "SavingsAccountID", "SavingsAccountDisplay");
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
