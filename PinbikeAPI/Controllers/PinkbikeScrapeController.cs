using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;

namespace PinbikeAPI.Controllers
{
    public class Article
    {

        public string title { get; set; }
        public string description { get; set; }
        public bool HasVideo { get; set; }
        public string PubDate { get; set; }
        public string ArticleUrl { get; set; }
        public string VideoUrl { get; set; }


        public Article(string title, string description, bool hasVideo, string PubDate, string ArticleUrl, string VideoUrl)
        {
            this.title = cleanData(title);
            this.description = cleanData(description);
            this.HasVideo = hasVideo;
            this.PubDate = cleanData(PubDate);
            this.ArticleUrl = cleanData(ArticleUrl);
            this.VideoUrl = cleanData(VideoUrl);
        }

        public string cleanData(string data)
        {
            return data.Replace("\r\n", "").Trim();
        }

    }

    [ApiController]
    [Route("/pinkbikearticles")]
    public class PinkbikeScrapeController : ControllerBase
    {

        private const string ARTICLE_TAG = "//div[@class='news-style1']";
        private const string TITLE_TAG = "descendant::a[@class='f22 fgrey4 bold']";
        private const string DESCRIPTION_TAG = "descendant::div[@class='news-mt3 fgrey5']";
        private const string PUBDATE_TAG = "descendant::span[@class='fgrey2']";

        [HttpGet]
        public async Task<List<Article>> PinbikeArticles()
        {
            List<Article> Datalist = new List<Article>();

            HttpClient hc = new HttpClient();
            HttpResponseMessage result = await hc.GetAsync($"https://www.pinkbike.com/");
            Stream stream = await result.Content.ReadAsStreamAsync();
            HtmlDocument doc = new HtmlDocument();
            doc.Load(stream);

            var Articles = doc.DocumentNode.SelectNodes(ARTICLE_TAG);


            foreach (var article in Articles)
            {
                HtmlNode TitleNode = article.SelectSingleNode(TITLE_TAG);
                string title = TitleNode.InnerText;
                string description = article.SelectSingleNode(DESCRIPTION_TAG).InnerText;
                bool hasVideo = title.Contains("Video:");
                string PubDate = article.SelectSingleNode(PUBDATE_TAG).InnerText;
                string ArticleUrl = TitleNode.GetAttributeValue("href", "");
                string VideoUrl = "";

                if (hasVideo)
                {
                    result = await hc.GetAsync(ArticleUrl);
                    stream = await result.Content.ReadAsStreamAsync();
                    HtmlDocument Videodoc = new HtmlDocument();
                    Videodoc.Load(stream);

                    if (Videodoc.DocumentNode.SelectSingleNode("//iframe") != null)
                    {
                        VideoUrl = Videodoc.DocumentNode.SelectSingleNode("//iframe").GetAttributeValue("src", "");
                    }
                    else if (Videodoc.DocumentNode.SelectSingleNode("//source") != null)
                    {
                        VideoUrl = Videodoc.DocumentNode.SelectSingleNode("//source").GetAttributeValue("src", "");
                    }
                    else {
                        VideoUrl = "The video url could not be located.";
                    }

                }

                Article articleStruct = new Article(title, description, hasVideo, PubDate, ArticleUrl, VideoUrl);

                Datalist.Add(articleStruct);
            }

            stream.Close();

            return Datalist;

        }
    }
}
