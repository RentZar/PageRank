using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlerSample
{
    class PRComputation
    {
        /// <summary>
        /// Seq Page Rank 
        /// </summary>
        public static double[] PageRank(int[,] a, int n, int[] sumLink)
        {
            double eps;
            const double kZatuhania = 0.85; // Коэффициент затухания
            double[] pagesRankOld = new double[n];
            double[] pagesRank = new double[n];

            for (int i = 0; i < n; i++)
                pagesRankOld[i] = 1;
            do
            {
                eps = 0;
                for (int j = 0; j < n; j++)
                {
                    double sum = 0;
                    pagesRank[j] = (1 - kZatuhania);
                    for (int i = 0; i < n; i++)
                    {
                        if (a[j, i] > 0)
                            sum += pagesRankOld[i] / sumLink[i];
                        else
                            sum += 0;
                    }
                    sum = kZatuhania * sum;
                    pagesRank[j] += sum;
                }

                for (int i = 0; i < pagesRank.Length; i++)
                    eps += (pagesRank[i] - pagesRankOld[i]) * (pagesRank[i] - pagesRankOld[i]);

                pagesRankOld = pagesRank;
            }
            while (eps > 0.000000001);
            return pagesRank;
        }

        /// <summary>
        /// Parallel Page Rank 
        /// </summary>
        public static double[] PageRankPar(int[,] a, int n, int[] sumLink)
        {
            double eps;
            const double kZatuhania = 0.85; // Коэффициент затухания
            double[] pagesRankOld = new double[n];
            double[] pagesRank = new double[n];

            for (int i = 0; i < n; i++)
                pagesRankOld[i] = 1;
            do
            {
                eps = 0;
                Parallel.For(0, n, j =>
                {
                    double sum = 0;
                    pagesRank[j] = (1 - kZatuhania);
                    for (int i = 0; i < n; i++)
                    {
                        if (a[j, i] > 0)
                            sum += pagesRankOld[i] / sumLink[i];
                        else
                            sum += 0;
                    }
                    sum = kZatuhania * sum;
                    pagesRank[j] += sum;
                });

                for (int i = 0; i < pagesRank.Length; i++)
                    eps += (pagesRank[i] - pagesRankOld[i]) * (pagesRank[i] - pagesRankOld[i]);

                pagesRankOld = pagesRank;
            }
            while (eps > 0.0001);
            return pagesRank;
        }
    }
}

