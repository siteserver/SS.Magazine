using SiteServer.Plugin;
using SS.Magazine.Core;
using SS.Magazine.Pages;
using SS.Magazine.Parse;
using SS.Magazine.Provider;

namespace SS.Magazine
{
    public class Main : PluginBase
    {
        public static Dao Dao { get; private set; }
        public static ArticleDao ArticleDao { get; private set; }

        public static Main Instance { get; private set; }

        public override void Startup(IService service)
        {
            Instance = this;

            Dao = new Dao();
            ArticleDao = new ArticleDao();

            service
                .AddContentModel(ContentTableUtils.ContentTableName, ContentTableUtils.ContentTableColumns)
                .AddContentMenu(new Menu
                {
                    Text = "杂志文章管理",
                    Href = $"{nameof(PageArticles)}.aspx"
                })
                .AddDatabaseTable(ArticleDao.TableName, ArticleDao.Columns)
                .AddStlElementParser(StlMagazineArticles.ElementName, StlMagazineArticles.Parse)
                .AddStlElementParser(StlMagazineArticle.ElementName, StlMagazineArticle.Parse)
                ;

            service.ApiPost += Service_ApiPost;
        }

        private object Service_ApiPost(object sender, ApiEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.RouteResource))
            {
                if (Utils.EqualsIgnoreCase(args.RouteResource, nameof(StlMagazineArticles.ApiArticles)))
                {
                    return StlMagazineArticles.ApiArticles(args.Request);
                }
            }

            return null;
        }
    }
}