using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Infrastructure;
using projects.Infrastructure.Entities;
using PuppeteerSharp;

namespace projects
{
    class Program
    {
        private static readonly string URL = "https://metruyencv.com";
        private static readonly string UrlWithPage = "https://metruyencv.com/truyen?sort_by=created_at&status=-1&props=-1&limit=30&page=";
        static async Task Main()
        {

            int threads = await ChooseSettings();
            await ExecuteCrawler(threads);
        }

        private static async Task<int> ChooseSettings()
        {
            Console.WriteLine("--------- Start crawler -------");
            Console.WriteLine("Check chronium install !!!! ");
            var browserFetcher = new BrowserFetcher();
            RevisionInfo revisionInfo = await browserFetcher.GetRevisionInfoAsync();
            var isDownload = revisionInfo.Downloaded;

            if (isDownload)
            {
                Console.WriteLine("Chronium is installed");
            }
            else
            {
                Console.WriteLine("Chronium isn't installed");
                Console.WriteLine("Start download and installing chronium");
                await DownloadAndInstallChronium(browserFetcher);
            }
            Console.WriteLine("Many threads ?");
            var threads = short.Parse(Console.ReadLine());
            Console.WriteLine($"Crawling data commics with {threads} threads, please wait for it!");
            return threads;
        }

        private static async Task DownloadAndInstallChronium(BrowserFetcher browserFetcher)
        {
            var process = await browserFetcher.DownloadAsync();
            if (process.Downloaded)
            {
                Console.WriteLine("Install chronium success");
            }
            else
            {
                Console.WriteLine("Install chronium fail");
            }
        }

        private static async Task ExecuteCrawler(int thread)
        {
            string html = await LoadPage(URL + "/truyen");
            var pagesSite = short.Parse(await GetLengthPages(html));
            var loop = pagesSite / thread;
            for (int i = 0; i < loop; i++)
            {
                var series = Enumerable.Range((i * thread) + 1, thread).ToList();
                await Task.WhenAll(series.Select(s => DoWorkAsync(s)));
            }
            if ((pagesSite % thread) > 0)
            {
                var series = Enumerable.Range((loop * thread) + 1, pagesSite % thread).ToList();
                await Task.WhenAll(series.Select(s => DoWorkAsync(s)));
            }
        }

        private static async Task DoWorkAsync(int i)
        {
            Console.WriteLine($"Starting Process {i}: " + DateTime.Now.ToString("yyyy-dd-MM-HH:mm:ss"));
            var htmlContent = await LoadPage(UrlWithPage + i.ToString());
            if (string.IsNullOrEmpty(htmlContent))
            {
                Console.WriteLine("Fail loaded" + i);
                return;
            }
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            var documentDoc = document.DocumentNode;
            var node = documentDoc.SelectNodes(".//div[@class='media border-bottom py-4']");
            var lstCommicsPage = new List<Commic>();
            foreach (var item in node)
            {
                Commic commic = new()
                {
                    Link = item.FirstChild.Attributes["href"].Value,
                    LinkImage = item.FirstChild.ChildNodes[0].Attributes["data-src"].Value,
                    Name = item.ChildNodes[2].FirstChild.InnerText.Trim(),
                    Description = item.ChildNodes[2].ChildNodes[2].InnerText.Trim(),
                    Author = item.ChildNodes[2].ChildNodes[4].FirstChild.ChildNodes[0].InnerText.Trim()
                };
                string urlCommic = URL + commic.Link;
                var htmlCommic = await LoadPage(urlCommic);
                var documentCommic = new HtmlDocument();
                documentCommic.LoadHtml(htmlCommic);
                commic.Status = documentCommic.DocumentNode.SelectSingleNode(".//li[@class='d-inline-block border border-danger px-3 py-1 text-danger rounded-3 mr-2 mb-2']").InnerText;
                commic.Category = documentCommic.DocumentNode.SelectSingleNode(".//li[@class='d-inline-block border border-primary px-3 py-1 text-primary rounded-3 mr-2 mb-2']").InnerText;
                var listMotips = documentCommic.DocumentNode.SelectNodes(".//li[@class='d-inline-block border border-success px-3 py-1 text-success rounded-3 mr-2 mb-2']");
                var valueMotipcs = string.Empty;
                if (listMotips != null)
                {
                    for (int j = 0; j < listMotips.Count; j++)
                    {
                        valueMotipcs += listMotips[j].InnerText;
                    }
                }
                commic.Motips = valueMotipcs;
                var info = documentCommic.DocumentNode.SelectNodes(".//li[@class='mr-5']");
                commic.LengthChapter = info[0].FirstChild.InnerText;
                commic.Performance = info[1].FirstChild.InnerText;
                commic.Reads = info[2].FirstChild.InnerText;
                commic.Rating = documentCommic.DocumentNode.SelectSingleNode(".//span[@class='d-inline-block ml-2']").InnerText;
                lstCommicsPage.Add(commic);
            }

            using var context = new ConsoleContext();
            await context.AddRangeAsync(lstCommicsPage);
            context.SaveChanges();
        }

        private static Task<string> GetLengthPages(string htmlContent)
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);
            return Task.FromResult(document.DocumentNode.SelectNodes(".//ul[@class='pagination pagination-sm']/li")[6].InnerText);
        }

        private static async Task<string> LoadPage(string urlPage)
        {
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new string[] { "--no-sandbox", "--disable-setuid-sandbox" },
                IgnoredDefaultArgs = new string[] { "--disable-extensions" }
            });
            await using var page = await browser.NewPageAsync();

            await page.SetUserAgentAsync("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.108 Safari/537.36");
            await page.GoToAsync(urlPage, timeout: 0);
            await Task.Delay(10000);
            string result = await page.GetContentAsync();
            await page.CloseAsync();
            await browser.CloseAsync();
            return result;
        }
    }
}
