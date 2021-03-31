using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionAnalyticsApp
{
    public class DbManager
    {
        //private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();
        private string conString = "Data Source=EmoAnalyticDB.db;Version=3;New=False;Compress=True;";

        private void SetConnection()
        {
            //sql_con = new SQLiteConnection(conString);
        }

        public void ExecuteQuery(string txtQuery)
        {
            using (var sql_con = new SQLiteConnection(conString))
            {
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                sql_cmd.ExecuteNonQuery();
                sql_con.Close();
            }
        }

        public object ExecuteFunctions(string txtQuery)
        {
            object retVal = null;
            using (var sql_con = new SQLiteConnection(conString))
            {
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                retVal = sql_cmd.ExecuteScalar();
                sql_con.Close();
            }

            return retVal;
        }

        public DataTable LoadData(string CommandText)
        {
            using (var sql_con = new SQLiteConnection(conString))
            {
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DS.Reset();
                DB.Fill(DS);
                DT = DS.Tables[0];
                sql_con.Close();
            }
            return DT;
        }
    }
}
