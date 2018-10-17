using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SiteServer.Plugin;
using WxPayAPI;

namespace SS.Magazine.Core
{
    public static class Utils
    {
        public static string GetMessageHtml(string message, bool isSuccess)
        {
            return isSuccess
                ? $@"<div class=""alert alert-success"" role=""alert"">{message}</div>"
                : $@"<div class=""alert alert-danger"" role=""alert"">{message}</div>";
        }

        public static void SelectListItems(ListControl listControl, params string[] values)
        {
            if (listControl != null)
            {
                foreach (ListItem item in listControl.Items)
                {
                    item.Selected = false;
                }
                foreach (ListItem item in listControl.Items)
                {
                    foreach (var value in values)
                    {
                        if (string.Equals(item.Value, value))
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                }
            }
        }

        public static int ToInt(string str, int defaultVal = 0)
        {
            int i;
            return int.TryParse(str, out i) ? i : defaultVal;
        }

        public static string GetUrlWithoutQueryString(string rawUrl)
        {
            string urlWithoutQueryString;
            if (rawUrl != null && rawUrl.IndexOf("?", StringComparison.Ordinal) != -1)
            {
                var queryString = rawUrl.Substring(rawUrl.IndexOf("?", StringComparison.Ordinal));
                urlWithoutQueryString = rawUrl.Replace(queryString, "");
            }
            else
            {
                urlWithoutQueryString = rawUrl;
            }
            return urlWithoutQueryString;
        }

        public static string AddQueryString(string url, NameValueCollection queryString)
        {
            if (queryString == null || url == null || queryString.Count == 0)
                return url;

            var builder = new StringBuilder();
            foreach (string key in queryString.Keys)
            {
                builder.Append($"&{key}={HttpUtility.UrlEncode(queryString[key])}");
            }
            if (url.IndexOf("?", StringComparison.Ordinal) == -1)
            {
                if (builder.Length > 0) builder.Remove(0, 1);
                return string.Concat(url, "?", builder.ToString());
            }
            if (url.EndsWith("?"))
            {
                if (builder.Length > 0) builder.Remove(0, 1);
            }
            return string.Concat(url, builder.ToString());
        }

        public static bool EqualsIgnoreCase(string a, string b)
        {
            if (a == b) return true;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            return string.Equals(a.Trim().ToLower(), b.Trim().ToLower());
        }

        public static string GetTopSqlString(DatabaseType databaseType, string tableName, string columns, string whereAndOrder, int topN)
        {
            if (topN > 0)
            {
                return databaseType == DatabaseType.MySql ? $"SELECT {columns} FROM {tableName} {whereAndOrder} LIMIT {topN}" : $"SELECT TOP {topN} {columns} FROM {tableName} {whereAndOrder}";
            }
            return $"SELECT {columns} FROM {tableName} {whereAndOrder}";
        }

        public static object Eval(object dataItem, string name)
        {
            object o = null;
            try
            {
                o = DataBinder.Eval(dataItem, name);
            }
            catch
            {
                // ignored
            }
            if (o == DBNull.Value)
            {
                o = null;
            }
            return o;
        }

        public static int EvalInt(object dataItem, string name)
        {
            var o = Eval(dataItem, name);
            return o == null ? 0 : Convert.ToInt32(o);
        }

        public static string ReplaceNewline(string inputString, string replacement)
        {
            if (string.IsNullOrEmpty(inputString)) return string.Empty;
            var retVal = new StringBuilder();
            inputString = inputString.Trim();
            foreach (var t in inputString)
            {
                switch (t)
                {
                    case '\n':
                        retVal.Append(replacement);
                        break;
                    case '\r':
                        break;
                    default:
                        retVal.Append(t);
                        break;
                }
            }
            return retVal.ToString();
        }

        public static string SwalError(string title, string text)
        {
            var script = $@"swal({{
  title: '{title}',
  text: '{ReplaceNewline(text, string.Empty)}',
  icon: 'error',
  button: '关 闭',
}});";

            return script;
        }

        public static string SwalWarning(string title, string text, string btnCancel, string btnSubmit, string scripts)
        {
            var script = $@"swal({{
  title: '{title}',
  text: '{ReplaceNewline(text, string.Empty)}',
  icon: 'warning',
  buttons: {{
    cancel: '{btnCancel}',
    catch: '{btnSubmit}'
  }}
}})
.then(function(willDelete){{
  if (willDelete) {{
    {scripts}
  }}
}});";
            return script;
        }

        public static object ChargeByWeixin(string productName, decimal amount, string guid, string notifyUrl)
        {

            WxPayConfig.APPID = "wx7d306eb1fef3c5eb";
            WxPayConfig.MCHID = "1512892151";
            WxPayConfig.KEY = "qUpaKcqjU5IeMJCF0q1A3AN5DKOaFMyG";

            //=======【支付结果通知url】===================================== 
            /* 支付结果通知回调url，用于商户接收支付结果
            */
            WxPayConfig.NOTIFY_URL = notifyUrl;

            //=======【商户系统后台机器IP】===================================== 
            /* 此参数可手动配置也可在程序中自动获取
            */
            WxPayConfig.IP = "8.8.8.8";


            //=======【代理服务器设置】===================================
            /* 默认IP和端口号分别为0.0.0.0和0，此时不开启代理（如有需要才设置）
            */
            WxPayConfig.PROXY_URL = "http://10.152.18.220:8080";

            //=======【上报信息配置】===================================
            /* 测速上报等级，0.关闭上报; 1.仅错误时上报; 2.全量上报
            */
            WxPayConfig.REPORT_LEVENL = 1;

            //=======【日志级别】===================================
            /* 日志等级，0.不输出日志；1.只输出错误信息; 2.输出错误和正常信息; 3.输出错误信息、正常信息和调试信息
            */
            WxPayConfig.LOG_LEVENL = 0;

            var data = new WxPayData();
            data.SetValue("body", productName);//商品描述
            data.SetValue("attach", string.Empty);//附加数据
            data.SetValue("out_trade_no", WxPayApi.GenerateOutTradeNo());//随机字符串
            data.SetValue("total_fee", Convert.ToInt32(amount * 100));//总金额
            data.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));//交易起始时间
            data.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));//交易结束时间
            data.SetValue("goods_tag", "jjj");//商品标记
            data.SetValue("trade_type", "APP");//交易类型
            data.SetValue("product_id", "productId");//商品ID

            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";
            //检测必填参数
            if (!data.IsSet("out_trade_no"))
            {
                throw new WxPayException("缺少统一支付接口必填参数out_trade_no！");
            }
            if (!data.IsSet("body"))
            {
                throw new WxPayException("缺少统一支付接口必填参数body！");
            }
            if (!data.IsSet("total_fee"))
            {
                throw new WxPayException("缺少统一支付接口必填参数total_fee！");
            }
            if (!data.IsSet("trade_type"))
            {
                throw new WxPayException("缺少统一支付接口必填参数trade_type！");
            }

            //关联参数
            if (data.GetValue("trade_type").ToString() == "JSAPI" && !data.IsSet("openid"))
            {
                throw new WxPayException("统一支付接口中，缺少必填参数openid！trade_type为JSAPI时，openid为必填参数！");
            }
            if (data.GetValue("trade_type").ToString() == "NATIVE" && !data.IsSet("product_id"))
            {
                throw new WxPayException("统一支付接口中，缺少必填参数product_id！trade_type为JSAPI时，product_id为必填参数！");
            }

            //异步通知url未设置，则使用配置文件中的url
            if (!data.IsSet("notify_url"))
            {
                data.SetValue("notify_url", WxPayConfig.NOTIFY_URL);//异步通知url
            }

            data.SetValue("appid", WxPayConfig.APPID);//公众账号ID
            data.SetValue("mch_id", WxPayConfig.MCHID);//商户号
            data.SetValue("spbill_create_ip", WxPayConfig.IP);//终端ip	  	    
            data.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));//随机字符串

            //签名
            data.SetValue("sign", data.MakeSign());
            var xml = data.ToXml();
            var response = HttpService.Post(xml, url, false, 6);
            var result = new WxPayData();
            result.FromXml(response);

            //"request": "<xml><appid><![CDATA[wx7d306eb1fef3c5eb]]></appid><attach><![CDATA[]]></attach><body><![CDATA[producta]]></body><goods_tag><![CDATA[jjj]]></goods_tag><mch_id><![CDATA[1512892151]]></mch_id><nonce_str><![CDATA[152b6698c8f44b9b9aa16f471c3e27b2]]></nonce_str><notify_url><![CDATA[http://cms.chinacampus.org/api/plugins/SS.Magazine/WeiXinNotify]]></notify_url><out_trade_no><![CDATA[151289215120180830085619872]]></out_trade_no><product_id><![CDATA[productId]]></product_id><sign><![CDATA[A4BF095E298130577E98F8CAAEBB496D]]></sign><spbill_create_ip><![CDATA[8.8.8.8]]></spbill_create_ip><time_expire><![CDATA[20180830090619]]></time_expire><time_start><![CDATA[20180830085619]]></time_start><total_fee>100</total_fee><trade_type><![CDATA[APP]]></trade_type></xml>",
            //"response": "<xml><return_code><![CDATA[SUCCESS]]></return_code>\n<return_msg><![CDATA[OK]]></return_msg>\n<appid><![CDATA[wx7d306eb1fef3c5eb]]></appid>\n<mch_id><![CDATA[1512892151]]></mch_id>\n<nonce_str><![CDATA[JL38tGchjgYcDECR]]></nonce_str>\n<sign><![CDATA[BD9DD927EDF0020E519B94FF0ABCD100]]></sign>\n<result_code><![CDATA[SUCCESS]]></result_code>\n<prepay_id><![CDATA[wx300856189223833b9ee467051757080666]]></prepay_id>\n<trade_type><![CDATA[APP]]></trade_type>\n</xml>"

            //Log.Info(GetType().ToString(), "ChargeByWeixin : " + response);
            //Log.Info(GetType().ToString(), "notify_url : " + data.GetValue("notify_url"));

            var appid = result.GetValue("appid");
            var mch_id = result.GetValue("mch_id");
            var nonce_str = result.GetValue("nonce_str");
            var sign = result.GetValue("sign");
            var prepay_id = result.GetValue("prepay_id");

            return new
            {
                appid,
                mch_id,
                nonce_str,
                sign,
                prepay_id,
                guid
            };
        }
    }
}
