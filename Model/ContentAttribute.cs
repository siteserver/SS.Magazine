namespace SS.Magazine.Model
{
	public class ContentAttribute
	{
        private ContentAttribute()
		{
		}
		
        public const string ImageUrl = "ImageUrl";                 //封面
        public const string FreeCount = "FreeCount";               //免费阅读篇数
        public const string PayCount = "PayCount";               //付费阅读篇数
        public const string ReadingCost = "ReadingCost";             //全刊电子阅读价格
        public const string PaperCost = "PaperCost";               //纸质购买价格
        public const string Content = "Content";                   //商品简介
        public const string PubDate = "PubDate";                   //上架时间
    }
}
