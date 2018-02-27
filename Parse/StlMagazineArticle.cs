using SiteServer.Plugin;
using SS.Magazine.Core;
using SS.Magazine.Model;

namespace SS.Magazine.Parse
{
    public class StlMagazineArticle
    {
        private StlMagazineArticle()
        {
        }

        public const string ElementName = "stl:magazineArticle";

        public const string AttributeType = "type";

        public static string Parse(IParseContext context)
        {
            var parsedContent = string.Empty;
            var type = string.Empty;

            foreach (var attriName in context.StlAttributes.Keys)
            {
                var value = context.StlAttributes[attriName];
                if (Utils.EqualsIgnoreCase(attriName, AttributeType))
                {
                    type = Main.Instance.ParseApi.ParseAttributeValue(value, context);
                }
            }

            if (Utils.EqualsIgnoreCase(nameof(ArticleInfo.Content), type))
            {
                parsedContent = @"<div v-html=""article.content""></div>";
            }
            else if (type.Length > 1)
            {
                parsedContent = "{{ article." + type.Substring(0, 1).ToLower() + type.Substring(1) + " }}";
            }

            return parsedContent;
        }
    }
}
