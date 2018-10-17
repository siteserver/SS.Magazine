using System;
using System.Collections.Generic;
using SiteServer.Plugin;
using SS.Magazine.Model;

namespace SS.Magazine.Parse
{
    public class StlMagazineArticles
    {
        private StlMagazineArticles()
        {
        }

        public const string ElementName = "stl:magazineArticles";

        public static object ApiArticles(IRequest context)
        {
            var siteId = context.GetPostInt("siteId");
            var contentId = context.GetPostInt("contentId");
            var articleId = context.GetPostInt("articleId");

            var list = new List<ArticleInfo>();
            if (articleId == 0)
            {
                list = Main.ArticleDao.GetArticleInfoList(siteId, contentId);
            }
            else
            {
                var articleInfo = Main.ArticleDao.GetArticleInfo(articleId);
                if (articleInfo != null)
                {
                    list.Add(articleInfo);
                }
            }

            return new
            {
                Articles = list,
                IsPurchased = Main.Dao.IsPurchased(siteId, contentId, context.UserName)
            };
        }

        public static string Parse(IParseContext context)
        {
            var jqueryUrl = Context.PluginApi.GetPluginUrl(Main.PluginId, "assets/js/jquery-1.9.1.min.js");
            var vueUrl = Context.PluginApi.GetPluginUrl(Main.PluginId, "assets/js/vue-2.4.2.min.js");
            var apiUrl = $"{Context.PluginApi.GetPluginApiUrl(Main.PluginId)}/{nameof(ApiArticles)}";
            var guid = "e" + Guid.NewGuid().ToString().Replace("-", string.Empty);

            var parsedContent = Context.ParseApi.Parse(context.StlInnerHtml, context);
            if (!string.IsNullOrEmpty(parsedContent))
            {
                parsedContent = $@"
<div id=""{guid}"">
    <template v-for=""article in articles"">
        {parsedContent}
    </template>
</div>
";
            }

            parsedContent += $@"
<script type=""text/javascript"" src=""{jqueryUrl}""></script>
<script type=""text/javascript"" src=""{vueUrl}""></script>
<script type=""text/javascript"">
    var v{guid} = new Vue({{
      el: '#{guid}',
      data: {{
        articles: [],
        isPurchased: false
      }}
    }});

    $(document).ready(function(){{
        var result = location.search.match(new RegExp('[\?\&]id=([^\&]+)','i'));
        var articleId = (result == null || result.length < 1) ? 0 : result[1];
        $.ajax({{
            url : ""{apiUrl}"",
            xhrFields: {{
                withCredentials: true
            }},
            type: ""POST"",
            data: JSON.stringify({{
                siteId: '{context.SiteId}',
                contentId: '{context.ContentId}',
                articleId: articleId
            }}),
            contentType: ""application/json; charset=utf-8"",
            dataType: ""json"",
            success: function(data)
            {{
                v{guid}.articles = data.articles;
                v{guid}.isPurchased = data.isPurchased;
            }},
            error: function (xhr, ajaxOptions, thrownError)
            {{
                console.log(xhr.status);
                console.log(thrownError);
            }}
        }});
    }});
</script>
";

            return parsedContent;
        }
    }
}
