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
            var jqueryUrl = Main.Instance.PluginApi.GetPluginUrl("assets/js/jquery-1.9.1.min.js");
            var vueUrl = Main.Instance.PluginApi.GetPluginUrl("assets/js/vue-2.4.2.min.js");
            var apiUrl = Main.Instance.PluginApi.GetPluginUrl(nameof(ApiArticles));
            var guid = "e" + Guid.NewGuid().ToString().Replace("-", string.Empty);

            var parsedContent = Main.Instance.ParseApi.ParseInnerXml(context.StlElementInnerXml, context);
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
            error: function (err)
            {{
                var err = JSON.parse(err.responseText);
                console.log(err.message);
            }}
        }});
    }});
</script>
";

            return parsedContent;
        }
    }
}
