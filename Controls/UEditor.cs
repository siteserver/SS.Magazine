using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.UI;

namespace SS.Magazine.Controls
{
    public class UEditor : Control, IPostBackDataHandler
    {
        public string Text
        {
            get
            {
                var state = ViewState["Text"];
                if (state != null)
                {
                    return (string)state;
                }
                return string.Empty;
            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        public string Width
        {
            get
            {
                var state = ViewState["Width"];
                if (state != null)
                {
                    return (string)state;
                }
                return "0";
            }
            set
            {
                ViewState["Width"] = value;
            }
        }

        public string Height
        {
            get
            {
                var state = ViewState["Height"];
                if (state != null)
                {
                    return (string)state;
                }
                return "0";
            }
            set
            {
                ViewState["Height"] = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var controllerUrl = "/api/sys/editors/ueditor/" + HttpContext.Current.Request.QueryString["siteId"];
            var editorUrl = "/sitefiles/assets/ueditor";

            if (string.IsNullOrEmpty(Height) || Height == "0")
            {
                Height = "280";
            }
            if (string.IsNullOrEmpty(Width) || Width == "0")
            {
                Width = "100%";
            }

            var builder = new StringBuilder();
            builder.Append(
                $@"<script type=""text/javascript"">window.UEDITOR_HOME_URL = ""{editorUrl}/"";window.UEDITOR_CONTROLLER_URL = ""{controllerUrl}"";</script><script type=""text/javascript"" src=""{editorUrl}/editor_config.js""></script><script type=""text/javascript"" src=""{editorUrl}/ueditor_all_min.js""></script>");

            builder.Append($@"
<textarea id=""{ClientID}"" name=""{ClientID}"" style=""display:none"">{HttpUtility.HtmlEncode(Text)}</textarea>
<script type=""text/javascript"">
$(function(){{
  UE.getEditor('{ClientID}', {{allowDivTransToP: false}});
  $('#{ClientID}').show();
}});
</script>");

            writer.Write(builder);
        }

        public event EventHandler TextChanged;

        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            var presentValue = Text;
            var postedValue = postCollection[postDataKey];
            if (!presentValue.Equals(postedValue))
            {
                Text = postedValue;
                return true;
            }
            return false;
        }

        public void RaisePostDataChangedEvent()
        {
            OnTextChanged(EventArgs.Empty);
        }

        protected virtual void OnTextChanged(EventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }
    }
}