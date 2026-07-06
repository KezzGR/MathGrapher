using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using MathGrapher.Core.Models;

namespace MathGrapher.Core.Data
{
    public static class HistoryRepository
    {
        public static void AddRecord(string expression, double xMin, double xMax, double step, double? area)
        {
            string sql = @"INSERT INTO GraphHistory (Expression, XMin, XMax, Step, Area) VALUES (@expr, @xMin, @xMax, @step, @area)";
            DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter("@expr", expression), new SqlParameter("@xMin", xMin),
                                                new SqlParameter("@xMax", xMax), new SqlParameter("@step", step), new SqlParameter("@area", (object?)area ?? DBNull.Value));
        }

        public static List<GraphRecord> GetHistory()
        {
            DataTable table = DatabaseHelper.ExecuteQuery("SELECT * FROM GraphHistory ORDER BY CreatedAt DESC");

            var list = new List<GraphRecord>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(new GraphRecord
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Expression = row["Expression"].ToString()!,
                    XMin = Convert.ToDouble(row["XMin"]),
                    XMax = Convert.ToDouble(row["XMax"]),
                    Step = Convert.ToDouble(row["Step"]),
                    Area = row["Area"] == DBNull.Value ? null : Convert.ToDouble(row["Area"]),
                    CreatedAt = Convert.ToDateTime(row["CreatedAt"])
                });
            }

            return list;
        }
    }
}