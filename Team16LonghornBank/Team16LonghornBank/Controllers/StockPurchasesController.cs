using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Team16LonghornBank.Models;

namespace Team16LonghornBank.Controllers
{
    public class StockPurchasesController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: StockPurchases
        public ActionResult Index()
        {
            return View(db.StockPurchases.ToList());
        }

        // GET: StockPurchases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockPurchase stockPurchase = db.StockPurchases.Find(id);
            if (stockPurchase == null)
            {
                return HttpNotFound();
            }
            return View(stockPurchase);
        }

        // GET: StockPurchases/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: StockPurchases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StockPurchaseID,PurchaseDate,InitialSharePrice,NumberOfShares,TotalStockValue,ChangeInPrice,TotalChange")] StockPurchase stockPurchase)
        {
            if (ModelState.IsValid)
            {
                db.StockPurchases.Add(stockPurchase);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(stockPurchase);
        }

        // GET: StockPurchases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockPurchase stockPurchase = db.StockPurchases.Find(id);
            if (stockPurchase == null)
            {
                return HttpNotFound();
            }
            return View(stockPurchase);
        }

        // POST: StockPurchases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "StockPurchaseID,PurchaseDate,InitialSharePrice,NumberOfShares,TotalStockValue,ChangeInPrice,TotalChange")] StockPurchase stockPurchase)
        {
            if (ModelState.IsValid)
            {
                db.Entry(stockPurchase).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(stockPurchase);
        }

        // GET: StockPurchases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockPurchase stockPurchase = db.StockPurchases.Find(id);
            if (stockPurchase == null)
            {
                return HttpNotFound();
            }
            return View(stockPurchase);
        }

        // POST: StockPurchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StockPurchase stockPurchase = db.StockPurchases.Find(id);
            db.StockPurchases.Remove(stockPurchase);
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
