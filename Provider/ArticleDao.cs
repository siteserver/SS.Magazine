using System.Collections.Generic;
using System.Data;
using SiteServer.Plugin;
using SS.Magazine.Core;
using SS.Magazine.Model;

namespace SS.Magazine.Provider
{
    public static class ArticleDao
    {
        public const string TableName = "ss_magazine_article";

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

        public static int Insert(ArticleInfo articleInfo)
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
                Context.DatabaseApi.GetParameter(nameof(articleInfo.SiteId), articleInfo.SiteId),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.ContentId), articleInfo.ContentId),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.Taxis), taxis),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.Title), articleInfo.Title),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.IsFree), articleInfo.IsFree),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.Content), articleInfo.Content)
            };

            return Context.DatabaseApi.ExecuteNonQueryAndReturnId(TableName, nameof(ArticleInfo.Id), Context.ConnectionString, sqlString, parameters.ToArray());
        }

        public static void Update(ArticleInfo articleInfo)
        {
            string sqlString = $@"UPDATE {TableName} SET
                {nameof(ArticleInfo.Title)} = @{nameof(ArticleInfo.Title)}, 
                {nameof(ArticleInfo.IsFree)} = @{nameof(ArticleInfo.IsFree)}, 
                {nameof(ArticleInfo.Content)} = @{nameof(ArticleInfo.Content)}
            WHERE {nameof(ArticleInfo.Id)} = @{nameof(ArticleInfo.Id)}";

            var parameters = new List<IDataParameter>
            {
                Context.DatabaseApi.GetParameter(nameof(articleInfo.Title), articleInfo.Title),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.IsFree), articleInfo.IsFree),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.Content), articleInfo.Content),
                Context.DatabaseApi.GetParameter(nameof(articleInfo.Id), articleInfo.Id)
            };

            Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString, parameters.ToArray());
        }

        public static void Delete(List<int> deleteIdList)
        {
            string sqlString =
                $"DELETE FROM {TableName} WHERE Id IN ({string.Join(",", deleteIdList)})";
            Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString);
        }

        public static string GetSelectString(int siteId, int contentId)
        {
            return $"SELECT {nameof(ArticleInfo.Id)}, {nameof(ArticleInfo.Taxis)} FROM {TableName} WHERE {nameof(ArticleInfo.SiteId)} = {siteId} AND {nameof(ArticleInfo.ContentId)} = {contentId} ORDER BY Taxis, Id";
        }

        public static List<ArticleInfo> GetArticleInfoList(int siteId, int contentId)
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
                Context.DatabaseApi.GetParameter(nameof(ArticleInfo.SiteId), siteId),
                Context.DatabaseApi.GetParameter(nameof(ArticleInfo.ContentId), contentId)
            };

            using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString, parameters))
            {
                while (rdr.Read())
                {
                    list.Add(GetArticleInfo(rdr));
                }
                rdr.Close();
            }

            return list;
        }

        public static ArticleInfo GetArticleInfo(int articleId)
        {
            ArticleInfo articleInfo = null;

            string sqlString = $@"SELECT {nameof(ArticleInfo.Id)}, 
            {nameof(ArticleInfo.SiteId)}, 
            {nameof(ArticleInfo.ContentId)}, 
            {nameof(ArticleInfo.Title)}, 
            {nameof(ArticleInfo.IsFree)}, 
            {nameof(ArticleInfo.Content)}
            FROM {TableName} WHERE {nameof(ArticleInfo.Id)} = {articleId}";

            using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString))
            {
                if (rdr.Read())
                {
                    articleInfo = GetArticleInfo(rdr);
                }
                rdr.Close();
            }

            return articleInfo;
        }

        private static int GetTaxis(int id)
        {
            string sqlString = $"SELECT Taxis FROM {TableName} WHERE Id = {id}";
            var taxis = 0;

            using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString))
            {
                if (rdr.Read())
                {
                    taxis = rdr.GetInt32(0);
                }
                rdr.Close();
            }

            return taxis;
        }

        private static void SetTaxis(int id, int taxis)
        {
            string sqlString = $"UPDATE {TableName} SET Taxis = {taxis} WHERE Id = {id}";
            Context.DatabaseApi.ExecuteNonQuery(Context.ConnectionString, sqlString);
        }

        private static int GetMaxTaxis(int contentId)
        {
            string sqlString = $"SELECT MAX(Taxis) FROM {TableName} WHERE {nameof(ArticleInfo.ContentId)} = {contentId}";
            return Dao.GetIntResult(sqlString);
        }

        public static bool UpdateTaxisToDown(int contentId, int id)
        {
            var sqlString = Utils.GetTopSqlString(Context.DatabaseType, TableName, "Id, Taxis", $"WHERE ((Taxis > (SELECT Taxis FROM {TableName} WHERE Id = {id})) AND {nameof(ArticleInfo.ContentId)} = {contentId}) ORDER BY Taxis", 1);

            var higherId = 0;
            var higherTaxis = 0;

            using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString))
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

        public static bool UpdateTaxisToUp(int contentId, int id)
        {
            var sqlString = Utils.GetTopSqlString(Context.DatabaseType, TableName, "Id, Taxis", $"WHERE ((Taxis < (SELECT Taxis FROM {TableName} WHERE (Id = {id}))) AND {nameof(ArticleInfo.ContentId)} = {contentId}) ORDER BY Taxis DESC", 1);

            var lowerId = 0;
            var lowerTaxis = 0;

            using (var rdr = Context.DatabaseApi.ExecuteReader(Context.ConnectionString, sqlString))
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
