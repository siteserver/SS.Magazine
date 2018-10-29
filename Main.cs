using SiteServer.Plugin;
using SS.Magazine.Core;
using SS.Magazine.Pages;
using SS.Magazine.Parse;
using SS.Magazine.Provider;

namespace SS.Magazine
{
    public class Main : PluginBase
    {
        public static string PluginId { get; private set; }

        public override void Startup(IService service)
        {
            PluginId = Id;

            service
                .AddContentModel(ContentTableUtils.ContentTableName, ContentTableUtils.ContentTableColumns)
                .AddContentMenu(contentInfo => new Menu
                {
                    Text = "杂志文章管理",
                    Href = $"{nameof(PageArticles)}.aspx"
                })
                .AddDatabaseTable(ArticleDao.TableName, ArticleDao.Columns)
                .AddStlElementParser(StlMagazineArticles.ElementName, StlMagazineArticles.Parse)
                .AddStlElementParser(StlMagazineArticle.ElementName, StlMagazineArticle.Parse)
                ;

            service.RestApiGet += Service_RestApiGet;
            service.RestApiPost += Service_RestApiPost;
        }

        private object Service_RestApiPost(object sender, RestApiEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.RouteResource))
            {
                if (Utils.EqualsIgnoreCase(args.RouteResource, nameof(StlMagazineArticles.ApiArticles)))
                {
                    return StlMagazineArticles.ApiArticles(args.Request);
                }
                if (Utils.EqualsIgnoreCase(args.RouteResource, WeiXinPayController.RouteResource))
                {
                    //var amount = args.Request.GetPostDecimal("amount");
                    //var detail = args.Request.GetPostString("detail");
                    //var guid = args.Request.GetPostString("guid");

                    return WeiXinPayController.Pay(args.Request);


                    //return Utils.ChargeByWeixin(detail, amount, string.Empty, WeiXinNotifyController.GetNotifyUrl(guid));
                }

                if (Utils.EqualsIgnoreCase(args.RouteResource, WeiXinNotifyController.RouteResource) && !string.IsNullOrEmpty(args.RouteId))
                {
                    return WeiXinNotifyController.Notify(args.RouteId);
                }
            }

            return null;
        }

        private object Service_RestApiGet(object sender, RestApiEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.RouteResource))
            {
                if (Utils.EqualsIgnoreCase(args.RouteResource, WeiXinNotifyController.RouteResource) && !string.IsNullOrEmpty(args.RouteId))
                {
                    return WeiXinNotifyController.Notify(args.RouteId);
                }
            }

            return null;
        }
    }
}