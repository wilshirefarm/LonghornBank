using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Net;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Team16LonghornBank.Models;
using System.Data.Entity;


namespace Team16LonghornBank.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private AppUserManager _userManager;
        private AppDbContext db = new AppDbContext();

        public ManageController()
        {
        }

        public ManageController(AppUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public AppUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }


        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //GET: Manage/EditInformation
        public ActionResult EditInformation()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            return View(user);
        }

        //POST: Manage/EditInformation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInformation(AppUser Customer)
        {
            if (ModelState.IsValid)
            {
                Customer.UserName = Customer.Email;
                db.Entry(Customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Manage");
            }
            return View(Customer);
        }

        //GET: Manage/EmployeePortal
        [Authorize(Roles = "Employee, Manager")]
        public ActionResult EmployeePortal()
        {
            AppRole CustomerRole = RoleManager.FindByName("Customer");
            string[] CustomerIds = CustomerRole.Users.Select(x => x.UserId).ToArray();
            IEnumerable<AppUser> CustomerList = UserManager.Users.Where(x => CustomerIds.Any(y => y == x.Id));

            AppRole EmployeeRole = RoleManager.FindByName("Employee");
            string[] EmployeeIds = EmployeeRole.Users.Select(x => x.UserId).ToArray();
            IEnumerable<AppUser> EmployeeList = UserManager.Users.Where(x => EmployeeIds.Any(y => y == x.Id));

            AppRole ManagerRole = RoleManager.FindByName("Manager");
            string[] ManagerIds = ManagerRole.Users.Select(x => x.UserId).ToArray();
            IEnumerable<AppUser> ManagerList = UserManager.Users.Where(x => ManagerIds.Any(y => y == x.Id));

            var TransactionsQuery = from t in db.Transactions
                                    where t.isPending.Equals(true)
                                    select t;
            List<Transaction> PendingTransactionsList = TransactionsQuery.ToList();

            var DisputesQuery = from d in db.Disputes
                                where d.CurrentStatus.Equals("Submitted")
                                select d;
            List<Dispute> DisputesList = DisputesQuery.ToList();

            var StockPortfoliosQuery = from s in db.StockPortfolios
                                       where s.isPending.Equals(true)
                                       select s;
            List<StockPortfolio> PendingStockPortfoliosList = StockPortfoliosQuery.ToList();

            EmployeePortalViewModel model = new EmployeePortalViewModel { Customers = CustomerList, Employees = EmployeeList, Managers = ManagerList, PendingTransactions = PendingTransactionsList, Disputes = DisputesList, PendingStockPortfolios = PendingStockPortfoliosList };

            return View(model);
        }

        //GET: Manage/EditCustomer
        [Authorize(Roles = "Employee, Manager")]
        public ActionResult EditCustomer(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //POST: Maange/EditCustomer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCustomer(AppUser Customer)
        {
            if (ModelState.IsValid)
            {
                Customer.UserName = Customer.Email;
                db.Entry(Customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EmployeePortal", "Manage");
            }
            return View(Customer);
        }

        //GET: Manage/ChangeUserPassword
        [Authorize(Roles = "Employee, Manager")]
        public ActionResult ChangeUserPassword(string id)
        {
            ChangeUserPasswordViewModel model = new ChangeUserPasswordViewModel { UserId = id };
            return View(model);
        }

        //POST: Manage/ChangeUserPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeUserPassword(ChangeUserPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var preresult = await UserManager.RemovePasswordAsync(model.UserId);
                if (preresult.Succeeded)
                {
                    var result = await UserManager.AddPasswordAsync(model.UserId, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("EmployeePortal", "Manage");
                    }
                    AddErrors(result);
                }
                AddErrors(preresult);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //GET: Manage/EditEmployee
        [Authorize(Roles = "Manager")]
        public ActionResult EditEmployee(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppUser user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //POST: Manage/EditEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditEmployee(AppUser Employee)
        {
            if (ModelState.IsValid)
            {
                Employee.UserName = Employee.Email;
                db.Entry(Employee).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EmployeePortal", "Manage");
            }
            return View(Employee);
        }

        //GET: Manage/CreateEmployee
        [Authorize(Roles = "Manager")]
        public ActionResult CreateEmployee()
        {
            return View();
        }

        //POST: Manage/CreateEmployee
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateEmployee(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, MI = model.MI, Address = model.Address, City = model.City, State = model.State, ZipCode = model.ZipCode, PhoneNumber = model.PhoneNumber, DOB = DateTime.Now, IsTerminated = false, IsActive = true, HasAccount = false };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Employee");
                    return RedirectToAction("EmployeePortal", "Manage");
                }
                AddErrors(result);
            }
            return View(model);
        }

        //GET: Manage/CreateManager
        [Authorize(Roles = "Manager")]
        public ActionResult CreateManager()
        {
            return View();
        }

        //POST: Manage/CreateManager
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateManager(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, MI = model.MI, Address = model.Address, City = model.City, State = model.State, ZipCode = model.ZipCode, PhoneNumber = model.PhoneNumber, DOB = DateTime.Now, IsTerminated = false, IsActive = true, HasAccount = false };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Manager");
                    return RedirectToAction("EmployeePortal", "Manage");
                }
                AddErrors(result);
            }
            return View(model);
        }

        private AppRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}