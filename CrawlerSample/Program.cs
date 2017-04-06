using CrawlerSample.Crawling;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrawlerSample
{
    class Program
    {
        static string matPath = @"../../Data/matrix.dat";
        static string dataPath = @"../../Data/";

        static void Main(string[] args)
        {
            
            CrawlMe();
           
            
            var mat = LoadMat();
            var sumLink = sumLinks(mat);

            using (StreamWriter writetext = new StreamWriter(dataPath + "result.out"))
            {
                var sTime = System.DateTime.Now;
                var ans = PRComputation.PageRank(mat, 100, sumLink);
                var fTime = System.DateTime.Now;
                writetext.WriteLine("Sequential: took " + (fTime - sTime).TotalMilliseconds);
                for (int i = 0; i < ans.Length; i++)
                {
                    writetext.Write(ans[i] + " \t ");
                }
                writetext.WriteLine("");
                writetext.WriteLine("");

                sTime = System.DateTime.Now;
                ans = PRComputation.PageRankPar(mat, 100, sumLink);
                fTime = System.DateTime.Now;
                writetext.WriteLine("Parallel: took " + (fTime - sTime).TotalMilliseconds);
                for (int i = 0; i < ans.Length; i++)
                {
                    writetext.Write(ans[i] + " \t ");
                }
            }
        }

        static void CrawlMe()
        {
            XmlConfigurator.Configure();
            Crawling.Crawl crawler = new Crawling.Crawl();
            crawler.BaseUrl = "http://kpfu.ru";

            var ans = crawler.CrawlSite();
			Console.WriteLine(ans);
            crawler.SaveData();
            Console.ReadKey();
        }

        static int[,] LoadMat()
        {
            int[,] ans = new int[100, 100];
            using (TextReader reader = File.OpenText(matPath))
            {
                for (int i = 0; i < 100; i++)
                {
                    string line = reader.ReadLine();
                    line = line.Substring(0, line.Length - 1);

                    new List<string>(line.Split('\t')).ForEach(x => ans[i, int.Parse(x)] = 1 );
                }
            }
            return ans;
        }

        static int[] sumLinks(int[,] mat)
        {
            int[] ans = new int[100];
            for(int i = 0; i < 100; i++)
            {
                for(int j = 0; j < 100; j++)
                {
                    if (mat[i, j] == 1)
                        ans[i]++;
                }
            }
            return ans;
        }
    }
}
