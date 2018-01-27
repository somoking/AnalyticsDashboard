using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace DbReportGenerator.Models
{
    public static class LINQExtension
    {
        /// <summary>
        /// Applied on a table with a search string, this is then returns all distinct sets as Iqueryable
        /// </summary>
        /// <param name="inputTable"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Dictionary<string, IQueryable<statusSQL>> returnDistinct(this IQueryable<statusSQL> inputTable, string columnName)
        {
            Type ElementType = inputTable.ElementType;
            
            Dictionary<string, IQueryable<statusSQL>> distinctCounts = new Dictionary<string, IQueryable<statusSQL>>();

            var inputColumn = inputTable.singleColumn(columnName);
            List<string> distinctVal = inputColumn.Distinct().ToList();
            foreach (string Value in distinctVal)
            {
                ParameterExpression c = Expression.Parameter(ElementType, "dbQuery");

                Expression right = Expression.Constant(Value);
                Expression e1 = Expression.Equal(Expression.Property(c, columnName), right);

                MethodCallExpression whereCallExpression = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new Type[] { inputTable.ElementType },
                    inputTable.Expression,
                    Expression.Lambda<Func<statusSQL, bool>>(e1, new ParameterExpression[] { c }));

                distinctCounts.Add(Value, inputTable.Provider.CreateQuery<statusSQL>(whereCallExpression));
            }

            return distinctCounts;

        }
        /// <summary>
        /// Method takes all the distinct values in a column then tallies all repeats of those values. This method uses the singleColumnMethod.
        /// </summary>
        /// <param name="inputTable"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static Dictionary<string, int> countDistinct(this IQueryable<statusSQL> inputTable, string columnName)
        {
            var inputColumn = inputTable.singleColumn(columnName);
            Dictionary<string, int> distinctCounts = new Dictionary<string, int>();
            IEnumerable<string> distinctVal = inputColumn.Distinct();
            foreach (string value in distinctVal)
            {

                //var query = (from x in EncryptionList where x == value select x);
                var query = inputColumn.Where(x => (x == value));
                distinctCounts.Add(value, query.Count());
            }
            return distinctCounts;

        }
        /// <summary>
        /// Takes in a Dictionary  Key:string ("Column Name")  Value:string ("Search Value") of Columns which will filter the column for the Specific Value.
        /// 
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public static IQueryable<statusSQL> FilterSpecific(this IQueryable<statusSQL> Table, Dictionary<string, string> Filters)
        {

            foreach (string key in Filters.Keys)
            {

                if (Filters[key] != "All" && Filters[key] != "None")
                {
                    
                    ParameterExpression paramameter = Expression.Parameter(typeof(statusSQL), "x");
                   

                    Expression selector = Expression.Property(paramameter, typeof(statusSQL).GetProperty(key));
                   
                    Expression right = Expression.Constant(Filters[key]);
                    
                    Expression EqualityComparison = Expression.Equal(selector, right);
                    

                    MethodCallExpression whereCallExpression = Expression.Call(
                   typeof(Queryable),
                   "Where",
                   new Type[] { Table.ElementType },
                   Table.Expression,
                   Expression.Lambda<Func<statusSQL, bool>>(EqualityComparison, new ParameterExpression[] { paramameter }));
                    



                    Table = Table.Provider.CreateQuery<statusSQL>(whereCallExpression);
                }
            }
            //possible null pass
            return Table;
        }
        /// <summary>
        /// Takes in a Dictionary  Key:string ("Column Name")  Value:string ("Search Value") which will filter the column by the Value using a .Contains() method in the table.
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="Filters"></param>
        /// <returns></returns>
        public static IQueryable<statusSQL> FilterContains(this IQueryable<statusSQL> Table, Dictionary<string, string> Filters)
        {
            
            foreach (string key in Filters.Keys)
            {
               
                if (Filters[key] != "")
                {

                    ParameterExpression paramameter = Expression.Parameter(typeof(statusSQL), "x");
                    Expression selector = Expression.Property(paramameter, typeof(statusSQL).GetProperty(key));
                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                    var Value = Expression.Constant(Filters[key], typeof(string));
                    Expression Contains = Expression.Call(selector, method, Value);

                   MethodCallExpression whereCallExpression = Expression.Call(
                   typeof(Queryable),
                   "Where",
                   new Type[] { Table.ElementType },
                   Table.Expression,
                   Expression.Lambda<Func<statusSQL, bool>>(Contains, new ParameterExpression[] { paramameter }));
                   
                    Table = Table.Provider.CreateQuery<statusSQL>(whereCallExpression);
                }
       
            }
            
            return Table;
        }


        /// <summary>
        /// Method is applied to the IQueryable and takes in the | EXACT | columnName
        /// The Method will return a List containing all the strings in the column.
        /// </summary>
        /// <param name="inputTable"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static List<string> singleColumn(this IQueryable<statusSQL> inputTable, string columnName)
        {
            var ElementType = inputTable.ElementType;


            //bulid a expression tree to create a paramter

            ParameterExpression paramameter = Expression.Parameter(ElementType, "Table");
            //bulid expression tree:data.Field1

            Expression selector = Expression.Property(paramameter, ElementType.GetProperty(columnName));// typeof(Object).GetProperty(columnName));
            Expression pred = Expression.Lambda(selector, paramameter);

            //bulid expression tree:Select(d=>d.Field1)
            Expression expr = Expression.Call(typeof(Queryable),
                "Select",
                new Type[] { ElementType, typeof(string) },
                Expression.Constant(inputTable.AsQueryable()), pred);

            //create dynamic query
            IQueryable<string> query = inputTable.Provider.CreateQuery<string>(expr);

            return query.ToList();
        }
    }


    public class BarChartReturn
    {
        public string ID { get; set; }
        public List<BarChartData> dataset { get; set; }
        public List<string> labelset { get; set; }

    }
    /// <summary>
    /// Takes values as such List<int> dataValues, string color, string Label, string Stack
    /// </summary>
    public class BarChartData
    {

        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string stack { get; set; }
        public List<int> data { get; set; }
        public BarChartData(List<int> dataValues, string color, string Label, string Stack)
        {
            data = dataValues;
            backgroundColor = color;
            label = Label;
            stack = Stack;
        }

    }
    public class LinechartReturn
    {
        public string ID { get; set; }
        public List<LineChartData> dataset { get; set; }
        public List<string> labelset { get; set; }

    }

    public class LineChartData
    {
        public string label { get; set; }
        public string backgroundColor { get; set; }
        public string borderColor { get; set; }
        public string fill { get; set; }
        public List<int> data { get; set; }
        public LineChartData(List<int> dataValues, string color, string Label, string Fill, string bordercolor)
        {
            data = dataValues;
            borderColor = bordercolor;
            backgroundColor = color;
            label = Label;
            fill = Fill;
        }

    }
    public class Query
    { 
        public Dictionary<string,string> QuerySpecific { get;set;}
        public Dictionary<string, string> QueryContains { get; set; }
        public string FilterNumber { get; set; }

        public string Application { get; set; }
    }

    
    public class QueryArray
    {
        //public int ID { get; set; }
        //public string NodeName { get; set; }
        public string Instance { get; set; }
        public string DBname { get; set; }
        //public string LogicalFileName { get; set; }
        //public string PathFromSQL { get; set; }
        public string FileType { get; set; }
        //public string NodeNameVormetric { get; set; }
        //public string GuardPointPath { get; set; }
        public string EncryptionStatus { get; set; }
        public string Application { get; set; }
        public string State { get; set; }

        public string page { get; set; }
        //public string Environment { get; set; }
    }

    
}

