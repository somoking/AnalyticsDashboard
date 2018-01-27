using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using DbReportGenerator.Models;
using PagedList;

namespace DbReportGenerator.Controllers
{
    public class DBdataController : Controller
    {
        private DataDBContext db = new DataDBContext();

        // Post method  
        [HttpPost]
        public ActionResult Submit(string Encrypted,string Accounted, string Production,string search, int page)
        {
            System.Diagnostics.Debug.WriteLine("action triggered",Encrypted);
            var Dbset = from m in db.Dbset
                        select m;
            if (Encrypted!="None")
            {
                bool searchBool = (Convert.ToBoolean(Encrypted));
                Dbset = from m in Dbset
                            where m.Encrypted == searchBool
                        select m;
            }

            if (Accounted!="None")
            {
                bool searchBool = (Convert.ToBoolean(Accounted));
                Dbset = from m in Dbset
                            where m.Accounted == searchBool
                        select m;
            }

            if (Production!="None")
            {
                bool searchBool = (Convert.ToBoolean(Production));
                Dbset = from m in Dbset
                            where m.Production == searchBool
                        select m;
            }
            if(search != null)
            {
                Dbset = Dbset.Where(s => s.Name.Contains(search));
            }

            Dbset = from m in Dbset
                        orderby m.Name descending
                        select m;


            int pageNumber = page ; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfProducts = Dbset.ToPagedList(pageNumber, 5); // will only contain 25 products max because of the pageSize

            return PartialView("PartialReportView",onePageOfProducts);
        }
        // GET: DBdatas
        public ActionResult Index()
        {
            var test = from m in db.Dbset
                        orderby m.Name descending
                        select m; //returns IQueryable<Product> representing an unknown number of products. a thousand maybe?
            int pageCount = (test.Count() / 7);
            //int Pages = Math.Floor(pageCount);
            System.Diagnostics.Debug.WriteLine(pageCount);
            int pageNumber = 2; // if no page was specified in the querystring, default to the first page (1)
            var onePageOfProducts = test.ToPagedList(pageNumber, 5); // will only contain 25 products max because of the pageSize

            return View(onePageOfProducts);


            /*var Dbset = from m in db.Dbset
                        select m;
            return View(Dbset);*/
        }

        // GET: DBdatas/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DBdata dBdata = db.Dbset.Find(id);
            if (dBdata == null)
            {
                return HttpNotFound();
            }
            return View(dBdata);
        }

        // GET: DBdatas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DBdatas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Accounted,Encrypted,Production")] DBdata dBdata)
        {
            if (ModelState.IsValid)
            {
                db.Dbset.Add(dBdata);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dBdata);
        }

        // GET: DBdatas/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DBdata dBdata = db.Dbset.Find(id);
            if (dBdata == null)
            {
                return HttpNotFound();
            }
            return View(dBdata);
        }

        // POST: DBdatas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Accounted,Encrypted,Production")] DBdata dBdata)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dBdata).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dBdata);
        }

        // GET: DBdatas/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DBdata dBdata = db.Dbset.Find(id);
            if (dBdata == null)
            {
                return HttpNotFound();
            }
            return View(dBdata);
        }

        // POST: DBdatas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            DBdata dBdata = db.Dbset.Find(id);
            db.Dbset.Remove(dBdata);
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
