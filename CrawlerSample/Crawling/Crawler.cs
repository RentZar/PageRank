using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abot.Crawler;
using Abot.Poco;
using System.Net;
using System.IO;

namespace CrawlerSample.Crawling
{
    class Crawl
    {
        public String BaseUrl { get; set; }
        public List<simplePage> pages { get; } = new List<simplePage>(0);
        private string dataPath = @"..\..\Data\";

        public struct simplePage{
            public string pageUrl;
            public List<string> linksTo;
            public List<string> linksFrom;
            public string content;
        }

        public List<simplePage> CrawlSite()
        {
            PoliteWebCrawler crawler = new PoliteWebCrawler();

            crawler.PageCrawlCompleted += crawler_ProcessPageCrawlCompleted;
            crawler.PageCrawlDisallowedAsync += crawler_PageCrawlDisallowed;
            crawler.PageLinksCrawlDisallowedAsync += crawler_PageLinksCrawlDisallowed;

            CrawlResult result = crawler.Crawl(new Uri(BaseUrl));
            if (result.ErrorOccurred)
                Console.WriteLine("Crawl of {0} completed with error: {1}", result.RootUri.AbsoluteUri, result.ErrorException.Message);
            else
                Console.WriteLine("Crawl of {0} completed without error.", result.RootUri.AbsoluteUri);

            crossLinksSearch();

            return pages;
        }

        private void crossLinksSearch()
        {
            foreach(var page in pages)
            {
                page.linksTo.ForEach(
                    x => {
                        var pageLinkingTo = pages.Find(y => y.pageUrl == x);
                        if (pageLinkingTo.pageUrl != null && !pageLinkingTo.linksFrom.Contains(page.pageUrl))
                            pageLinkingTo.linksFrom.Add(page.pageUrl);
                    }
                );
            }
        }

        public void SaveData()
        {
            int[,] a = new int[pages.Count, pages.Count];


            using (StreamWriter writetext = new StreamWriter(dataPath + "matrix.dat"))
            {
                //writetext.WriteLine("PAGE URL \t\t\t|\t\t\t links from following pages lead to this page");
                foreach (var page in pages)
                {
                    int pageInd = pages.FindIndex(x => x.pageUrl == page.pageUrl);
                    /*
                    writetext.Write(
                        pageInd + "\t\t|\t\t"
                        + page.pageUrl + "\t\t|\t\t");*/

                    page.linksFrom.ForEach(
                        x => {
                            int refPageInd = pages.FindIndex(y => y.pageUrl == x);
                            a[pageInd, refPageInd] = 1;
                            writetext.Write(refPageInd + "\t");
                        }
                    );
                    writetext.WriteLine("");
                }
            }

            using (StreamWriter writetext = new StreamWriter(dataPath + "adjMatrix.dat"))
            {
                for (int i = 0; i < pages.Count; i++)
                {
                    //writetext.Write(i + "\t");
                    for (int j = 0; j < pages.Count; j++)
                    {
                        writetext.Write(a[i, j] + "\t");
                    }
                    writetext.WriteLine("");
                }
            }
        }

        void crawler_ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;

            var angleSharpHtmlDocument = crawledPage.AngleSharpHtmlDocument; //AngleSharp parser

            simplePage toAdd = new simplePage();
            toAdd.linksTo = new List<string>();
            toAdd.linksFrom = new List<string>();
            toAdd.pageUrl = crawledPage.Uri.AbsoluteUri;
            var links = angleSharpHtmlDocument.Links;
            foreach (var link in links)
            {
                string linkValue = link.Attributes["href"].Value;
                if (linkValue[0] == '/')
                {
                    linkValue = BaseUrl + linkValue;
                }
                if(linkValue.Length > 4 && linkValue.Substring(0, 4) == "http")
                {
                    toAdd.linksTo.Add(linkValue);
                }
            }
            toAdd.content = crawledPage.Content.Text;

            pages.Add(toAdd);


            if (crawledPage.WebException != null || crawledPage.HttpWebResponse.StatusCode != HttpStatusCode.OK)
                Console.WriteLine("Crawl of page failed {0}", crawledPage.Uri.AbsoluteUri);
            else
                Console.WriteLine("Crawl of page succeeded {0}", crawledPage.Uri.AbsoluteUri);

            if (string.IsNullOrEmpty(crawledPage.Content.Text))
                Console.WriteLine("Page had no content {0}", crawledPage.Uri.AbsoluteUri);
        }

        void crawler_PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Console.WriteLine("Did not crawl the links on page {0} due to {1}", crawledPage.Uri.AbsoluteUri, e.DisallowedReason);
        }

        void crawler_PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Console.WriteLine("Did not crawl page {0} due to {1}", pageToCrawl.Uri.AbsoluteUri, e.DisallowedReason);
        }

    }
}
