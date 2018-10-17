using System.Collections.Generic;
using System.Data;
using SiteServer.Plugin;
using SS.Magazine.Core;
using SS.Magazine.Model;

namespace SS.Magazine.Provider
{
    public class ArticleDao
    {
        public const string TableName = "ss_magazine_article";

        private readonly DatabaseType _databaseType;
        private readonly string _connectionString;
        private readonly IDatabaseApi _helper;

        public ArticleDao()
        {
            _databaseType = Context.DatabaseType;
            _connectionString = Context.ConnectionString;
            _helper = Context.DatabaseApi;
        }

        public static List<TableColumn> Columns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.Id),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.SiteId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.ContentId),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.Taxis),
                DataType = DataType.Integer
            },
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.Title),
                DataType = DataType.VarChar,
                DataLength = 200
            },
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.IsFree),
                DataType = DataType.Boolean
            },
            new TableColumn
            {
                AttributeName = nameof(ArticleInfo.Content),
                DataType = DataType.Text
            }
        };

        public int Insert(ArticleInfo articleInfo)
        {
            string sqlString = $@"INSERT INTO {TableName}
           ({nameof(ArticleInfo.SiteId)}, 
            {nameof(ArticleInfo.ContentId)}, 
            {nameof(ArticleInfo.Taxis)}, 
            {nameof(ArticleInfo.Title)}, 
            {nameof(ArticleInfo.IsFree)}, 
            {nameof(ArticleInfo.Content)})
     VALUES
           (@{nameof(ArticleInfo.SiteId)}, 
            @{nameof(ArticleInfo.ContentId)}, 
            @{nameof(ArticleInfo.Taxis)}, 
            @{nameof(ArticleInfo.Title)}, 
            @{nameof(ArticleInfo.IsFree)}, 
            @{nameof(ArticleInfo.Content)})";

            var taxis = GetMaxTaxis(articleInfo.ContentId) + 1;

            var parameters = new List<IDataParameter>
            {
                _helper.GetParameter(nameof(articleInfo.SiteId), articleInfo.SiteId),
                _helper.GetParameter(nameof(articleInfo.ContentId), articleInfo.ContentId),
                _helper.GetParameter(nameof(articleInfo.Taxis), taxis),
                _helper.GetParameter(nameof(articleInfo.Title), articleInfo.Title),
                _helper.GetParameter(nameof(articleInfo.IsFree), articleInfo.IsFree),
                _helper.GetParameter(nameof(articleInfo.Content), articleInfo.Content)
            };

            return _helper.ExecuteNonQueryAndReturnId(TableName, nameof(ArticleInfo.Id), _connectionString, sqlString, parameters.ToArray());
        }

        public void Update(ArticleInfo articleInfo)
        {
            string sqlString = $@"UPDATE {TableName} SET
                {nameof(ArticleInfo.Title)} = @{nameof(ArticleInfo.Title)}, 
                {nameof(ArticleInfo.IsFree)} = @{nameof(ArticleInfo.IsFree)}, 
                {nameof(ArticleInfo.Content)} = @{nameof(ArticleInfo.Content)}
            WHERE {nameof(ArticleInfo.Id)} = @{nameof(ArticleInfo.Id)}";

            var parameters = new List<IDataParameter>
            {
                _helper.GetParameter(nameof(articleInfo.Title), articleInfo.Title),
                _helper.GetParameter(nameof(articleInfo.IsFree), articleInfo.IsFree),
                _helper.GetParameter(nameof(articleInfo.Content), articleInfo.Content),
                _helper.GetParameter(nameof(articleInfo.Id), articleInfo.Id)
            };

            _helper.ExecuteNonQuery(_connectionString, sqlString, parameters.ToArray());
        }

        public void Delete(List<int> deleteIdList)
        {
            string sqlString =
                $"DELETE FROM {TableName} WHERE Id IN ({string.Join(",", deleteIdList)})";
            _helper.ExecuteNonQuery(_connectionString, sqlString);
        }

        public string GetSelectString(int siteId, int contentId)
        {
            return $"SELECT {nameof(ArticleInfo.Id)}, {nameof(ArticleInfo.Taxis)} FROM {TableName} WHERE {nameof(ArticleInfo.SiteId)} = {siteId} AND {nameof(ArticleInfo.ContentId)} = {contentId} ORDER BY Taxis, Id";
        }

        public List<ArticleInfo> GetArticleInfoList(int siteId, int contentId)
        {
            var list = new List<ArticleInfo>();

            string sqlString = $@"SELECT {nameof(ArticleInfo.Id)}, 
                {nameof(ArticleInfo.SiteId)}, 
                {nameof(ArticleInfo.ContentId)}, 
                {nameof(ArticleInfo.Title)}, 
                {nameof(ArticleInfo.IsFree)}, 
                {nameof(ArticleInfo.Content)}
                FROM {TableName} WHERE 
                {nameof(ArticleInfo.SiteId)} = @{nameof(ArticleInfo.SiteId)} AND
                {nameof(ArticleInfo.ContentId)} = @{nameof(ArticleInfo.ContentId)}
                ORDER BY {nameof(ArticleInfo.Id)}";

            var parameters = new[]
            {
                _helper.GetParameter(nameof(ArticleInfo.SiteId), siteId),
                _helper.GetParameter(nameof(ArticleInfo.ContentId), contentId)
            };

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString, parameters))
            {
                while (rdr.Read())
                {
                    list.Add(GetArticleInfo(rdr));
                }
                rdr.Close();
            }

            return list;
        }

        public ArticleInfo GetArticleInfo(int articleId)
        {
            ArticleInfo articleInfo = null;

            string sqlString = $@"SELECT {nameof(ArticleInfo.Id)}, 
            {nameof(ArticleInfo.SiteId)}, 
            {nameof(ArticleInfo.ContentId)}, 
            {nameof(ArticleInfo.Title)}, 
            {nameof(ArticleInfo.IsFree)}, 
            {nameof(ArticleInfo.Content)}
            FROM {TableName} WHERE {nameof(ArticleInfo.Id)} = {articleId}";

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    articleInfo = GetArticleInfo(rdr);
                }
                rdr.Close();
            }

            return articleInfo;
        }

        private int GetTaxis(int id)
        {
            string sqlString = $"SELECT Taxis FROM {TableName} WHERE Id = {id}";
            var taxis = 0;

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    taxis = rdr.GetInt32(0);
                }
                rdr.Close();
            }

            return taxis;
        }

        private void SetTaxis(int id, int taxis)
        {
            string sqlString = $"UPDATE {TableName} SET Taxis = {taxis} WHERE Id = {id}";
            _helper.ExecuteNonQuery(_connectionString, sqlString);
        }

        private int GetMaxTaxis(int contentId)
        {
            string sqlString = $"SELECT MAX(Taxis) FROM {TableName} WHERE {nameof(ArticleInfo.ContentId)} = {contentId}";
            return Main.Dao.GetIntResult(sqlString);
        }

        public bool UpdateTaxisToDown(int contentId, int id)
        {
            var sqlString = Utils.GetTopSqlString(_databaseType, TableName, "Id, Taxis", $"WHERE ((Taxis > (SELECT Taxis FROM {TableName} WHERE Id = {id})) AND {nameof(ArticleInfo.ContentId)} = {contentId}) ORDER BY Taxis", 1);

            var higherId = 0;
            var higherTaxis = 0;

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    higherId = rdr.GetInt32(0);
                    higherTaxis = rdr.GetInt32(1);
                }
                rdr.Close();
            }

            var selectedTaxis = GetTaxis(id);

            if (higherId != 0)
            {
                SetTaxis(id, higherTaxis);
                SetTaxis(higherId, selectedTaxis);
                return true;
            }
            return false;
        }

        public bool UpdateTaxisToUp(int contentId, int id)
        {
            var sqlString = Utils.GetTopSqlString(_databaseType, TableName, "Id, Taxis", $"WHERE ((Taxis < (SELECT Taxis FROM {TableName} WHERE (Id = {id}))) AND {nameof(ArticleInfo.ContentId)} = {contentId}) ORDER BY Taxis DESC", 1);

            var lowerId = 0;
            var lowerTaxis = 0;

            using (var rdr = _helper.ExecuteReader(_connectionString, sqlString))
            {
                if (rdr.Read())
                {
                    lowerId = rdr.GetInt32(0);
                    lowerTaxis = rdr.GetInt32(1);
                }
                rdr.Close();
            }

            var selectedTaxis = GetTaxis(id);

            if (lowerId != 0)
            {
                SetTaxis(id, lowerTaxis);
                SetTaxis(lowerId, selectedTaxis);
                return true;
            }
            return false;
        }

        private static ArticleInfo GetArticleInfo(IDataRecord rdr)
        {
            if (rdr == null) return null;
            
            var articleInfo = new ArticleInfo();

            var i = 0;
            articleInfo.Id = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            articleInfo.SiteId = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            articleInfo.ContentId = rdr.IsDBNull(i) ? 0 : rdr.GetInt32(i);
            i++;
            articleInfo.Title = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);
            i++;
            articleInfo.IsFree = !rdr.IsDBNull(i) && rdr.GetBoolean(i);
            i++;
            articleInfo.Content = rdr.IsDBNull(i) ? string.Empty : rdr.GetString(i);

            return articleInfo;
        }

    }
}
