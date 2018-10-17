using System.Collections.Generic;
using System.Data;
using SiteServer.Plugin;

namespace SS.Magazine.Provider
{
    public static class Dao
    {
        public static int GetIntResult(string sqlString)
        {
            var count = 0;

            using (var conn = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                conn.Open();
                using (var rdr = Context.DatabaseApi.ExecuteReader(conn, sqlString))
                {
                    if (rdr.Read() && !rdr.IsDBNull(0))
                    {
                        count = rdr.GetInt32(0);
                    }
                    rdr.Close();
                }
            }
            return count;
        }

        public static int GetIntResult(string sqlString, IDataParameter[] parameters)
        {
            var count = 0;

            using (var conn = Context.DatabaseApi.GetConnection(Context.ConnectionString))
            {
                conn.Open();
                using (var rdr = Context.DatabaseApi.ExecuteReader(conn, sqlString, parameters))
                {
                    if (rdr.Read() && !rdr.IsDBNull(0))
                    {
                        count = rdr.GetInt32(0);
                    }
                    rdr.Close();
                }
            }
            return count;
        }

        public static bool IsPurchased(int siteId, int contentId, string userName)
        {
            var isPurchased = false;

            const string sqlString = @"SELECT Id FROM ss_payment_record WHERE SiteId = @SiteId AND ProductId = @ProductId AND UserName = @UserName AND IsPaied = @IsPaied";

            var parameters = new List<IDataParameter>
            {
                Context.DatabaseApi.GetParameter("SiteId", siteId),
                Context.DatabaseApi.GetParameter("ProductId", contentId),
                Context.DatabaseApi.GetParameter("UserName", userName),
                Context.DatabaseApi.GetParameter("IsPaied", true)
            };

            using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString, parameters.ToArray()))
            {
                if (rdr.Read() && !rdr.IsDBNull(0))
                {
                    isPurchased = true;
                }
                rdr.Close();
            }

            return isPurchased;
        }
    }
}