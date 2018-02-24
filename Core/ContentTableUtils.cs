using System.Collections.Generic;
using SiteServer.Plugin;
using SS.Magazine.Model;

namespace SS.Magazine.Core
{
    public class ContentTableUtils
    {
        public static string ContentTableName => "ss_magazine";

        public static List<TableColumn> ContentTableColumns => new List<TableColumn>
        {
            new TableColumn
            {
                AttributeName = ContentAttribute.ImageUrl,
                DataType = DataType.VarChar,
                DataLength = 500,
                InputStyle = new InputStyle
                {
                    InputType = InputType.Image,
                    DisplayName = "封面",
                    IsRequired = true
                }
            },
            new TableColumn
            {
                AttributeName = ContentAttribute.FreeCount,
                DataType = DataType.Integer,
                InputStyle = new InputStyle
                {
                    InputType = InputType.Text,
                    DisplayName = "免费阅读篇数",

                    IsRequired = true,
                    ValidateType = ValidateType.Integer
                }

            },
            new TableColumn
            {
                AttributeName = ContentAttribute.PayCount,
                DataType = DataType.Integer,
                InputStyle = new InputStyle
                {
                    InputType = InputType.Text,
                    DisplayName = "付费阅读篇数",
                    IsRequired = true,
                    ValidateType = ValidateType.Integer
                }

            },
            new TableColumn
            {
                AttributeName = ContentAttribute.ReadingCost,
                DataType = DataType.Decimal,
                InputStyle = new InputStyle
                {
                    InputType = InputType.Text,
                    DisplayName = "全刊电子阅读价格",
                    IsRequired = true,
                    ValidateType = ValidateType.Currency,

                }

            },
            new TableColumn
            {
                AttributeName = ContentAttribute.PaperCost,
                DataType = DataType.Decimal,
                InputStyle = new InputStyle
                {
                    InputType = InputType.Text,
                    DisplayName = "纸质购买价格",
                    IsRequired = true,
                    ValidateType = ValidateType.Currency
                }
            },
            new TableColumn
            {
                AttributeName = ContentAttribute.Content,
                DataType = DataType.Text,
                InputStyle = new InputStyle
                {
                    InputType = InputType.TextEditor,
                    DisplayName = "商品简介",
                    IsRequired = true
                }

            },
            new TableColumn
            {
                AttributeName = ContentAttribute.PubDate,
                DataType = DataType.DateTime,
                InputStyle = new InputStyle
                {
                    InputType = InputType.DateTime,
                    DisplayName = "上架时间",
                    IsRequired = true
                }
            }
        };
    }
}
