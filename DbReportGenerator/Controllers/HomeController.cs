using DbReportGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.Caching;
using PagedList;
using System.Net;

namespace DbReportGenerator.Controllers
{
    public class HomeController : Controller
    {
        MemoryCache cache = MemoryCache.Default;
        private List<string> Catagories = new List<string> {"Application" };
        private List<string> SpecificColumns = new List<string> { "EncryptionStatus","FileType","State", };
        private StatusSqlEntities db = new StatusSqlEntities();
        private IQueryable<statusSQL> dbQuery = new StatusSqlEntities().statusSQLs.AsQueryable();
        private IQueryable<DatabaseEncryptionLog> DatabaseLog = new StatusSqlEntities().DatabaseEncryptionLogs.AsQueryable();
        private int page = 1;
        private int maxpage = 3;
        private int pagesize = 1000;
        public int DateTimetoInt(string DateTimeString)
        {
            string[] array=DateTimeString.Split('/');
            array=array.Reverse().ToArray();
            if (array[2].Count() == 1) { array[2] = 0 + array[2]; }
            var newinteger = Int32.Parse(string.Join("",array));
            return newinteger;
        }
        public ActionResult Index()
        {
            
            List<string> ColumnsList = new List<string>();
            ColumnsList.AddRange(SpecificColumns);ColumnsList.AddRange(Catagories);
            foreach (string specific in ColumnsList)
            {
                if (cache[specific] == null)
                {
                    IEnumerable<string> Query = dbQuery.singleColumn(specific).Distinct();
                    Query = Query.OrderBy(s => s);
                    string key = specific;
                    IEnumerable<string> value = Query;
                    CacheItemPolicy policy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(168, 0, 0) };
                    cache.Add(key, value, policy);
                    ViewData[specific] = Query;
                }
                else
                {
                    ViewData[specific] = cache[specific];
                }
            }
            
            int Totalcount = (from x in dbQuery select x).Count();

            var encryptionDict = dbQuery.returnDistinct("EncryptionStatus");

            foreach(string key in encryptionDict.Keys)
            {
                ViewData[key] = encryptionDict[key].Count();
            }


            ViewData["Total"]= Totalcount;


            return View();

        }
        public ActionResult Records(Query QueryObj)
        {
            page = Int32.Parse(QueryObj.FilterNumber);
            var ChartQuery = dbQuery;
            QueryObj.QuerySpecific.Add("Application", QueryObj.Application);
            ChartQuery = ChartQuery.FilterSpecific(QueryObj.QuerySpecific);
            ChartQuery = ChartQuery.FilterContains(QueryObj.QueryContains);
            int searchTotal = ChartQuery.Count();
            ViewBag.searchTotal = searchTotal;


            var resultGroup = ChartQuery.OrderByDescending(r => r.id)
                                               .Skip(pagesize * (page - 1))
                                               .Take(pagesize);



            ViewData["page"] = page;
            if (searchTotal % pagesize > 0)
            { ViewData["maxpage"] = (searchTotal / pagesize) + 1; }
            else { ViewData["maxpage"] = (searchTotal / pagesize); }

            return PartialView("DataGrid", resultGroup.ToList());

        }

        public ActionResult BarSubmit(Query QueryObj)
        {


            Dictionary<string, string> QuerySpecific = QueryObj.QuerySpecific;

            BarChartReturn BarChartReturn = new BarChartReturn();
           
            BarChartReturn.labelset = new List<string>();

            var ChartQuery = dbQuery;
        
           
            
            ChartQuery = ChartQuery.FilterSpecific(QuerySpecific);
            ChartQuery = ChartQuery.FilterContains(QueryObj.QueryContains);

            Dictionary<string, IQueryable<statusSQL>> QueryDict = new Dictionary<string, IQueryable<statusSQL>>();
            
            if (QueryObj.Application != "None")
            {
                
                if (QueryObj.Application == "All")
                {
                    QueryDict= ChartQuery.returnDistinct("Application");
                }
                else
                {
                    
                    QueryDict[QueryObj.Application] =ChartQuery.Where(x => x.Application == QueryObj.Application);
                    
                }
               
            }
            else
            {
                QueryDict[QueryObj.Application] = ChartQuery;
            }



            //initializer for bar chart
            //----------------------------------------------------------------------------------------



            Dictionary<string, BarChartData> DatasetDictionary = new Dictionary<string, BarChartData>();
                List<BarChartData> dataset = new List<BarChartData>();
                int count = 0;

            
            Dictionary<string, IEnumerable<string>> AllColumnKeys = new Dictionary<string, IEnumerable<string>>();
            Tuple<string> PrimaryColumn = Tuple.Create <string>(null);

            foreach (string ColumnString in QuerySpecific.Keys)
            {
                if (QuerySpecific[ColumnString] == "All")
                {
                    IEnumerable<string> ColumnKeys = ChartQuery.singleColumn(ColumnString).Distinct();
                    AllColumnKeys.Add(ColumnString,ColumnKeys);


                    string stack = "stack" + count;
                    foreach (string key in ColumnKeys)
                    {
                        DatasetDictionary.Add(key, new BarChartData(new List<int>(), "#000000", key, stack));
                    }
                    count++;
                }
                else
                {

                    if (QuerySpecific.Keys.First()==ColumnString)
                    {
                        PrimaryColumn = Tuple.Create<string>(ColumnString);
                        string stack = "stack" + count;
                        var title = "Total ";
                        if (QuerySpecific[ColumnString] != "None") { title += QuerySpecific[ColumnString]; }
                        DatasetDictionary.Add(QuerySpecific[ColumnString], new BarChartData(new List<int>(), "#000000", title, stack));
                        count++;
                    }
                }
            }

            //----------------------------------------------------------------------------------------
            List<int> TimeList = new List<int>();
            int FilterNumber = Int32.Parse(QueryObj.FilterNumber);
            
            foreach (string key in QueryDict.Keys)
            {
                
                

                ChartQuery = (IQueryable<statusSQL>)QueryDict[key];
                if (ChartQuery.Count() < FilterNumber) { continue; };
                BarChartReturn.labelset.Add(key);
                foreach (string Column in AllColumnKeys.Keys)
                {
                    
                    var AllocationDictionary = ChartQuery.returnDistinct(Column);
                        
                        foreach (string Value in AllColumnKeys[Column])
                        {
                            if (AllocationDictionary.ContainsKey(Value))
                            {
                                DatasetDictionary[Value].data.Add(AllocationDictionary[Value].Count());
                            }
                            else
                            {
                                DatasetDictionary[Value].data.Add(0);
                            }
                        }

                }
                if (PrimaryColumn.Item1 != null)
                {
                    DatasetDictionary[QuerySpecific[PrimaryColumn.Item1]].data.Add(QueryDict[key].Count());
                }
            }

            BarChartReturn.dataset = DatasetDictionary.Values.ToList();




            return Json(BarChartReturn, JsonRequestBehavior.AllowGet);
            }


        public ActionResult LineSubmit(Query QueryObj)//QueryChartArray queryValues)
        {
            LinechartReturn LinechartReturn = new LinechartReturn();

            IQueryable<DatabaseEncryptionLog> ChartQuery = DatabaseLog;
            if (QueryObj.Application != "None")
            {

                ChartQuery = ChartQuery.Where(x => x.Application == QueryObj.Application);


            }


            var DatesOnRecord = ChartQuery.Select(x => x.LastUpdate).Distinct().ToList();

            var i = 1;
            while (i < DatesOnRecord.Count())
            {
                string x = DatesOnRecord[i];
                int k = i - 1;
                while ( k >= 0 && DateTimetoInt(DatesOnRecord[k]) > DateTimetoInt(x))
                {
                    DatesOnRecord[k+1] = DatesOnRecord[k];
                    k --;
                }
                DatesOnRecord[k + 1] = x;
                i++;
            }
            

       

               Dictionary <string, IQueryable<DatabaseEncryptionLog>> TotalRecords = new Dictionary<string, IQueryable<DatabaseEncryptionLog>>();
 

            List<string> EncryptionTypes = ChartQuery.Select(x => x.EncryptionStatus).Distinct().ToList();

            Dictionary<string, LineChartData> LineChartData = new Dictionary<string, LineChartData>();

            LineChartData.Add("Total", new LineChartData(new List<int>(), "#000000", "Total", "false", "#000000"));
            foreach (string EncryptionType in EncryptionTypes)
            {

                LineChartData.Add(EncryptionType, new LineChartData(new List<int>(), "#000000", EncryptionType, "false", "#000000"));
            }
            //var color = colorList.Dequeue();
            //LineChartData.Add("Encrypted", new LineChartData(new List<int>(), color, "Encrypted", "false", color));
            //color = colorList.Dequeue();
            //LineChartData.Add("Unencrypted", new LineChartData(new List<int>(), color, "Unencrypted", "false", color));
            //color = colorList.Dequeue();
            //LineChartData.Add("Other", new LineChartData(new List<int>(), color, "Other", "false", color));
           // color = colorList.Dequeue();
            //LineChartData.Add("Total", new LineChartData(new List<int>(), color, "Total", "false", color));

            foreach (string Date in DatesOnRecord)
            {
              
                TotalRecords.Add(Date, DatabaseLog.Where(x => x.LastUpdate == Date));
                
            };

            LinechartReturn.labelset = TotalRecords.Keys.ToList();

            foreach (string TimeSeries in TotalRecords.Keys)
            {
              
                int? Total = TotalRecords[TimeSeries].Select(x => x.Tally).Sum();
                if (Total == null) { Total = 0; }
                LineChartData["Total"].data.Add((int)Total);
                
                foreach(string EncryptionType in EncryptionTypes)
                {

                    int? count = TotalRecords[TimeSeries].Where(y => y.EncryptionStatus == EncryptionType).Select(z => z.Tally).Sum(); ;
                    if (count == null) { count = 0; }
                    LineChartData[EncryptionType].data.Add((int)count);
                }
                /*int? unencrypted = TotalRecords[TimeSeries].Where(y => y.EncryptionStatus == "unencrypted/non-compliant. hostname and guardpoint not defined in the dsm" || y.EncryptionStatus == "not encrypted").Select(z => z.Tally).Sum(); ;
                if (unencrypted == null) { unencrypted = 0; }
                LineChartData["Unencrypted"].data.Add((int)unencrypted);
          
                int? encrypted = TotalRecords[TimeSeries].Where(y => y.EncryptionStatus == "encrypted").Select(z => z.Tally).Sum();
                if (encrypted == null) { encrypted = 0; }
                LineChartData["Encrypted"].data.Add((int)encrypted);

                LineChartData["Other"].data.Add(((int)Total - (int)unencrypted - (int)encrypted));*/

            }

            LinechartReturn.dataset = LineChartData.Values.ToList();

            return Json(LinechartReturn, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Details(string id)
        {


            int idNum = Int32.Parse(id);

            statusSQL statusSQL = db.statusSQLs.Find(idNum);

            if (statusSQL == null)
            {


                return HttpNotFound();
            }

            return PartialView("DialogeDetails", statusSQL);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}