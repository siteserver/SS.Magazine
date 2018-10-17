using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using SS.Magazine.Controls;
using SS.Magazine.Core;
using SS.Magazine.Model;
using SS.Magazine.Provider;

namespace SS.Magazine.Pages
{
	public class PageArticles : Page
	{
	    public Literal LtlMessage;

        public Repeater RptArticles;
        public SqlPager SpArticles;

        public Button BtnAdd;
        public Button BtnDelete;
        public Button BtnTaxis;

	    public PlaceHolder PhModalAdd;
        public Literal LtlModalAddTitle;
        public Literal LtlModalAddMessage;
        public TextBox TbTitle;
        public DropDownList DdlIsFree;
        public UEditor TbContent;

	    public PlaceHolder PhModalTaxis;
	    public DropDownList DdlIsTaxisUp;
	    public TextBox TbTaxisCount;

        private int _siteId;
        private int _contentId;
   
        public static string GetRedirectUrl(int siteId, int contentId)
        {
            return $"{nameof(PageArticles)}.aspx?siteId={siteId}&contentId={contentId}";
        }

	    public void Page_Load(object sender, EventArgs e)
	    {
	        _siteId = Convert.ToInt32(Request.QueryString["siteId"]);
	        _contentId = Convert.ToInt32(Request.QueryString["contentId"]);

	        if (!SiteServer.Plugin.Context.Request.AdminPermissions.HasSitePermissions(_siteId, Main.PluginId))
	        {
	            Response.Write("<h1>未授权访问</h1>");
	            Response.End();
	            return;
	        }

	        if (!string.IsNullOrEmpty(Request.QueryString["delete"]) &&
	            !string.IsNullOrEmpty(Request.QueryString["idCollection"]))
	        {
	            var list = Request.QueryString["idCollection"].Split(',').Select(s => Convert.ToInt32(s)).ToList();
	            ArticleDao.Delete(list);
	            LtlMessage.Text = Utils.GetMessageHtml("删除成功！", true);
	        }

	        SpArticles.ControlToPaginate = RptArticles;
	        SpArticles.ItemsPerPage = 30;
	        SpArticles.SelectCommand = ArticleDao.GetSelectString(_siteId, _contentId);
	        SpArticles.SortField = nameof(ArticleInfo.Taxis);
	        SpArticles.SortMode = "ASC";
	        RptArticles.ItemDataBound += RptArticles_ItemDataBound;

	        if (IsPostBack) return;

	        SpArticles.DataBind();

	        BtnAdd.Attributes.Add("onclick",
	            $"location.href = '{GetRedirectUrl(_siteId, _contentId)}&addArticle={true}';return false;");

	        BtnDelete.Attributes.Add("onclick", Utils.ReplaceNewline($@"
var ids = [];
$(""input[name='idCollection']:checked"").each(function () {{
    ids.push($(this).val());}}
);
if (ids.length > 0){{
    {Utils.SwalWarning("删除文章", "此操作将删除所选文章，确定吗？", "取 消", "删 除",
	            $"location.href='{GetRedirectUrl(_siteId, _contentId)}&delete={true}&idCollection=' + ids.join(',')")}
}} else {{
    {Utils.SwalError("请选择需要删除的文章！", string.Empty)}
}}
;return false;", string.Empty));

	        BtnTaxis.Attributes.Add("onclick", Utils.ReplaceNewline($@"
var ids = [];
$(""input[name='idCollection']:checked"").each(function () {{
    ids.push($(this).val());}}
);
if (ids.length > 0){{
    location.href = '{GetRedirectUrl(_siteId, _contentId)}&taxis={true}&idCollection=' + ids.join(',')
}} else {{
    {Utils.SwalError("请选择需要排序的文章！", string.Empty)}
}}
;return false;", string.Empty));

	        if (!string.IsNullOrEmpty(Request.QueryString["addArticle"]))
	        {
	            PhModalAdd.Visible = true;

	            var articleId = Convert.ToInt32(Request.QueryString["articleId"]);

	            var jsUrl = SiteServer.Plugin.Context.PluginApi.GetPluginUrl(Main.PluginId, "assets/script.js");

	            LtlModalAddTitle.Text = articleId > 0 ? "编辑表单内容" : "新增表单内容";
	            LtlModalAddTitle.Text += $@"<script type=""text/javascript"" src=""{jsUrl}""></script>";
	            if (articleId > 0)
	            {
	                var articleInfo = ArticleDao.GetArticleInfo(articleId);
	                TbTitle.Text = articleInfo.Title;
	                Utils.SelectListItems(DdlIsFree, articleInfo.IsFree.ToString());
	                TbContent.Text = articleInfo.Content;
	            }
	        }
	        else if (!string.IsNullOrEmpty(Request.QueryString["taxis"]) &&
                !string.IsNullOrEmpty(Request.QueryString["idCollection"]))
	        {
	            PhModalTaxis.Visible = true;
	        }
	    }

	    private void RptArticles_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var articleId = Utils.EvalInt(e.Item.DataItem, nameof(ArticleInfo.Id));
            var articleInfo = ArticleDao.GetArticleInfo(articleId);

            var ltlTitle = (Literal)e.Item.FindControl("ltlTitle");
            var ltlIsFree = (Literal)e.Item.FindControl("ltlIsFree");
            var ltlActions = (Literal)e.Item.FindControl("ltlActions");

            ltlTitle.Text = articleInfo.Title;
            ltlIsFree.Text = articleInfo.IsFree ? "免费阅读" : "付费阅读";

            ltlActions.Text =
                $@"
<a href=""{GetRedirectUrl(_siteId, _contentId)}&addArticle={true}&articleId={articleId}"">修改</a>
";
        }

        public void BtnAdd_OnClick(object sender, EventArgs e)
        {
            var isChanged = false;

            var articleId = Convert.ToInt32(Request.QueryString["articleId"]);

            if (articleId > 0)
            {
                try
                {
                    var articleInfo = ArticleDao.GetArticleInfo(articleId);

                    articleInfo.Title = TbTitle.Text;
                    articleInfo.IsFree = Convert.ToBoolean(DdlIsFree.SelectedValue);
                    articleInfo.Content = TbContent.Text;

                    ArticleDao.Update(articleInfo);

                    isChanged = true;
                }
                catch (Exception ex)
                {
                    LtlModalAddMessage.Text = Utils.GetMessageHtml("信息修改失败:" + ex.Message, false);
                }
            }
            else
            {
                try
                {
                    var articleInfo = new ArticleInfo
                    {
                        SiteId = _siteId,
                        ContentId = _contentId,
                        Title = TbTitle.Text,
                        IsFree = Convert.ToBoolean(DdlIsFree.SelectedValue),
                        Content = TbContent.Text
                    };

                    ArticleDao.Insert(articleInfo);

                    isChanged = true;
                }
                catch (Exception ex)
                {
                    LtlModalAddMessage.Text = Utils.GetMessageHtml("信息添加失败:" + ex.Message, false);
                }
            }

            if (isChanged)
            {
                Response.Redirect(GetRedirectUrl(_siteId, _contentId));
            }
        }

        public void BtnTaxis_OnClick(object sender, EventArgs e)
        {

            var list = Request.QueryString["idCollection"].Split(',').Select(s => Convert.ToInt32(s)).ToList();
            var isUp = Convert.ToBoolean(DdlIsTaxisUp.SelectedValue);
            var count = Convert.ToInt32(TbTaxisCount.Text);

            if (isUp)
            {
                for (var i = 0; i < count; i++)
                {
                    foreach (var id in list)
                    {
                        ArticleDao.UpdateTaxisToUp(_contentId, id);
                    }
                }
            }
            else
            {
                for (var i = 0; i < count; i++)
                {
                    foreach (var id in list)
                    {
                        ArticleDao.UpdateTaxisToDown(_contentId, id);
                    }
                }
            }
            Response.Redirect(GetRedirectUrl(_siteId, _contentId));
        }
    }
}
