using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace HaloCareCore.Helpers
{
    public class DataAccessHelper
    {
        private IConfiguration Configuration;
        public DataAccessHelper(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        #region HCare-973
        public DataTable ExecuteQuery(string sql)
        {
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter myAdapter = new SqlDataAdapter();
            DataTable dtStore = new DataTable();
            SqlConnection conn = new SqlConnection(Configuration.GetConnectionString("DefaultConnection"));

            try
            {
                conn.Open();
                cmd.CommandTimeout = 0;
                cmd.CommandText = sql;
                cmd.Connection = conn;

                myAdapter.SelectCommand = cmd;
                myAdapter.Fill(dtStore);
            }

            catch (Exception ex)
            {
                //do nothing
                var text = ex.Message;

            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();

                if (cmd != null)
                    cmd.Dispose();

                if (myAdapter != null)
                    myAdapter.Dispose();
            }

            return dtStore;
        }
        #endregion
    }
}