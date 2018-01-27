using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DbReportGenerator.Models;
using PagedList;

namespace DbReportGenerator.Controllers
{
    public class statusSQLsController : Controller
    {
        private StatusSqlEntities db = new StatusSqlEntities();
        //Due to the frequency of calls to statusSQLs.ToList I created dblist. Test performance to see if nesscary
        private List<statusSQL> dbList = (new StatusSqlEntities()).statusSQLs.ToList();

        /// <summary>
        /// Search Controller, Takes in data from each field layed out as either a dropdown box or a search function. 
        /// Procedurly tests if the filter was requested, If not skips the filter, If so applies filter to main List
        /// </summary>
        /// <param name="Application"></param>
        /// <returns> Data Grid with the new filtered data</returns>

        public ActionResult Submit(QueryArray queryValues)
        {
            ViewBag.Query = queryValues;
            
            
            var querydata = from x in dbList
                            select x;

            if (queryValues.EncryptionStatus != "None")
            {
                querydata = from x in querydata
                            where x.EncryptionStatus == queryValues.EncryptionStatus
                            select x;
            }
            if (queryValues.Application != "None")
            {//Fix for white space errors in Tanium
                if (queryValues.Application.Trim() == "Tanium")
                {
                    querydata = querydata.Where(s => s.Application.Trim() == "Tanium");
                    querydata = from x in querydata
                                where x.Application.Trim() == "Tanium"
                                select x;
                }
                else
                {
                    querydata = from x in querydata
                                where x.Application == queryValues.Application
                                select x;
                }
            }
            

            if (queryValues.FileType != "None")
            {
                querydata = from x in querydata
                            where x.FileType == queryValues.FileType
                            select x;
            }
            
            if (queryValues.State != "None")
            {
                querydata = from x in querydata
                            where x.State == queryValues.State
                            select x;
            }
            
            if (queryValues.DBname != null)
            {
                querydata = querydata.Where(s => s.DBname.ToLower().Contains(queryValues.DBname.ToLower()));
            }

            if (queryValues.Instance != null)
            {
                querydata = querydata.Where(s => s.Instance.Contains(queryValues.Instance));
            }

            ViewBag.searchTotal = querydata.Count();

            //pagination requirments passing valeus for max page & current page
            int maxpage = (querydata.Count() / 1000);
            if (maxpage % 1000 > 0){maxpage++;}
            if (maxpage == 0) { maxpage = 1; };
            ViewBag.maxPage = maxpage;
            querydata = from x in querydata
                        orderby x.DBname ascending
                            select x;
            int currentPage = Int32.Parse(queryValues.page);
            ViewData["page"]= currentPage;
            var onePageOfRecords = querydata.ToPagedList(currentPage, 1000); 



            return PartialView("DataGrid",onePageOfRecords);
        }
        //search insights
      
        // GET: statusSQLs

        public ActionResult Index()
        {
            ViewData["page"] = 1;
               //Creating Drop Down boxes for each of the fields
            var applicationQuery = (from v in dbList
                                    orderby v.Application ascending
                                   select v.Application).Distinct();
            ViewBag.ApplicationList = applicationQuery;

            var fileTypeQuery = (from v in dbList
                                    select v.FileType).Distinct();
            ViewBag.fileTypeList = fileTypeQuery;
            
            var StateQuery = (from v in dbList
                                    select v.State).Distinct();
            ViewBag.StateList = StateQuery;

            var EncryptionQuery = (from v in dbList
                              select v.EncryptionStatus).Distinct();
            ViewBag.EncryptionList = EncryptionQuery;

            return View();
        }

        // GET: statusSQLs/Details/5
        public ActionResult Details(string id)
        {

           
            int idNum=Int32.Parse(id);
  
            if (idNum == null)
            {


                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            statusSQL statusSQL = db.statusSQLs.Find(idNum);
           
            if (statusSQL == null)
            {


                return HttpNotFound();
            }

            return PartialView("PopUpRecord",statusSQL);
        }

        // GET: statusSQLs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: statusSQLs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,NodeName,Instance,DBname,LogicalFileName,PathFromSQL,FileType,NodeNameVormetric,GuardPointPath,EncryptionStatus,Application,State,Environment,LastUpdate")] statusSQL statusSQL)
        {
            if (ModelState.IsValid)
            {
                db.statusSQLs.Add(statusSQL);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(statusSQL);
        }

        // GET: statusSQLs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statusSQL statusSQL = db.statusSQLs.Find(id);
            if (statusSQL == null)
            {
                return HttpNotFound();
            }
            return View(statusSQL);
        }

        // POST: statusSQLs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,NodeName,Instance,DBname,LogicalFileName,PathFromSQL,FileType,NodeNameVormetric,GuardPointPath,EncryptionStatus,Application,State,Environment,LastUpdate")] statusSQL statusSQL)
        {
            if (ModelState.IsValid)
            {
                db.Entry(statusSQL).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(statusSQL);
        }

        // GET: statusSQLs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            statusSQL statusSQL = db.statusSQLs.Find(id);
            if (statusSQL == null)
            {
                return HttpNotFound();
            }
            return View(statusSQL);
        }

        // POST: statusSQLs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            statusSQL statusSQL = db.statusSQLs.Find(id);
            db.statusSQLs.Remove(statusSQL);
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
