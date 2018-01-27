using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity.Core.Objects;

namespace DbReportGenerator.Models
{
    public class DBdata
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public bool Accounted { get; set; }
        public bool Encrypted { get; set; }
        public bool Production { get; set; }
        [NotMapped]
        [AllowHtml]
        public string Priority
        {
            get {
                int PriorityValue = 0;
                string PriorityPlainTxt;
                if (!Accounted) { PriorityValue += 1; }
                if (!Encrypted) { PriorityValue += 1; }
                if (Production) { PriorityValue += 1; }
                switch (PriorityValue)
                {
                    case 1:
                        PriorityPlainTxt = "Minor";
                        break;
                    case 2:
                        PriorityPlainTxt = "Medium";
                        break;
                    case 3:
                        PriorityPlainTxt = "Major";
                        break;
                    default:
                        PriorityPlainTxt = "";
                        break;
                }
                return PriorityPlainTxt;
            }
        }
    }

    public class DataDBContext : System.Data.Entity.DbContext
    {
        public DbSet<DBdata> Dbset { get; set; }
    }



}


    