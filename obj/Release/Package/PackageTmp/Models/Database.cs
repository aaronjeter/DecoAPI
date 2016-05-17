using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace DecoAPI.Models
{
    public class Database
    {
        SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
        SqlConnection paintConnection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DecoSQLConnectionString"].ToString());

        SqlDataReader reader;

        public enum DBType
        {
            DecoSQL02,
            DecoSQL
        }

        public void SetReader(SqlDataReader reader)
        {
            this.reader = reader;
        }

        protected SqlConnection GetConnection(DBType db)
        {
            switch (db)
            {
                case DBType.DecoSQL02: //Default Database
                    return connection;
                case DBType.DecoSQL: //Paint's Database (wonderware and such)
                    return paintConnection;
            }

            throw new Exception("Failed to Connect to Database");
        }

        public void RunCommand(SqlCommand command)
        {
            command.Connection = connection;

            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void RunCommand(string query)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            RunCommand(command);
        }

        public SqlDataReader RunCommandReturnReader(SqlCommand command, DBType db)
        {
            SqlConnection con = GetConnection(db);
            command.Connection = con;
            OpenConnection(db);
            SqlDataReader reader = command.ExecuteReader();
            return reader;
        }

        /// <summary>
        /// Default run command. Assumes query directed to DecoSql02, aka the main production database
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public SqlDataReader RunCommandReturnReader(SqlCommand command)
        {
            return RunCommandReturnReader(command, DBType.DecoSQL02);
        }

        public SqlDataReader RunCommandReturnReader(string query)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return RunCommandReturnReader(command);
        }

        public int ExecuteNonQuery(SqlCommand command)
        {
            int returnValue = 0;

            command.Connection = connection;
            OpenConnection();
            returnValue = command.ExecuteNonQuery();
            CloseConnection();

            return returnValue;
        }

        /// <summary>
        /// Method to safely withdraw nullable string values from database
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public string SafeGetString(string field)
        {
            string value = null;
            int index = reader.GetOrdinal(field);
            if (!reader.IsDBNull(index))
            {
                value = reader.GetString(index);
            }

            return value;
        }

        /// <summary>
        /// Method to safely withdraw nullable date values from database
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public DateTime? SafeGetDate(string field)
        {
            DateTime? value = null;
            int index = reader.GetOrdinal(field);
            if (! reader.IsDBNull(index))
            {
                value = reader.GetDateTime(index);
            }

            return value;
        }

        #region Single Result Queries

        /// <summary>
        /// Method to return a string from a SqlCommand. Useful for queries that expect a single string return value
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public string RunSingleResultQuery(SqlCommand command)
        {
            return RunSingleResultQuery(command, DBType.DecoSQL02);            
        }

        public string RunSingleResultQuery(SqlCommand command, DBType db)
        {
            SqlDataReader reader = RunCommandReturnReader(command, db);
            string retval = "";

            if (reader.Read())
            {
                retval = reader.GetValue(0).ToString();
            }
            else
            {
                throw new Exception("Failed To read from Database");
            }

            CloseConnection(db);
            return retval;
        }

        public string RunSingleResultQuery(string query, DBType db)
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = query;
            return RunSingleResultQuery(command, db);
        }

        #endregion


        #region Open/Close Connections

        /// <summary>
        /// Method to open Sql Connection
        /// </summary>
        /// <returns></returns>
        public bool OpenConnection()
        {
            return OpenConnection(DBType.DecoSQL02);
        }

        public bool OpenConnection(DBType db)
        {
            SqlConnection con = GetConnection(db);

            if (con != null && con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Method to Close Sql Connection
        /// </summary>
        /// <returns></returns>
        public bool CloseConnection()
        {
            return CloseConnection(DBType.DecoSQL02);
        }

        public bool CloseConnection(DBType db)
        {
            SqlConnection con = GetConnection(db);
            if (con != null && con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
                return true;
            }
            return false;
        }

        #endregion
    }
}