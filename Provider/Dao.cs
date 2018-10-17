using System.Collections.Generic;
using System.Data;
using SiteServer.Plugin;

namespace SS.Magazine.Provider
{
    public class Dao
    {
        private readonly string _connectionString;
        private readonly IDatabaseApi _helper;

        public Dao()
        {
            _connectionString = Context.ConnectionString;
            _helper = Context.DatabaseApi;
        }

        public int GetIntResult(string sqlString)
        {
            var count = 0;

            using (var conn = _helper.GetConnection(_connectionString))
            {
                conn.Open();
                using (var rdr = _helper.ExecuteReader(conn, sqlString))
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

        public int GetIntResult(string sqlString, IDataParameter[] parameters)
        {
            var count = 0;

            using (var conn = _helper.GetConnection(_connectionString))
            {
                conn.Open();
                using (var rdr = _helper.ExecuteReader(conn, sqlString, parameters))
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

        public bool IsPurchased(int siteId, int contentId, string userName)
        {
            var isPurchased = false;

            const string sqlString = @"SELECT Id FROM ss_payment_record WHERE SiteId = @SiteId AND ProductId = @ProductId AND UserName = @UserName AND IsPaied = @IsPaied";

            var parameters = new List<IDataParameter>
            {
                _helper.GetParameter("SiteId", siteId),
                _helper.GetParameter("ProductId", contentId),
                _helper.GetParameter("UserName", userName),
                _helper.GetParameter("IsPaied", true)
            };

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString, parameters.ToArray()))
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