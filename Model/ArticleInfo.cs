using Newtonsoft.Json;

namespace SS.Magazine.Model
{
    [JsonObject(MemberSerialization.OptOut)]
    public class ArticleInfo
    {
        public int Id { get; set; }

        public int SiteId { get; set; }

        public int ContentId { get; set; }

        public int Taxis { get; set; }

        public string Title { get; set; }

        public bool IsFree { get; set; }

        public string Content { get; set; }
    }
}