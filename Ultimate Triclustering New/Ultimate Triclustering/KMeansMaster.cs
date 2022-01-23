using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ultimate_Triclustering
{
    class NumericalTriplet
    {
        public string o, a, c;
        public double val;

        public NumericalTriplet(string obj = "", string attr = "", string cond = "", double value = 0)
        {
            o = obj;
            a = attr;
            c = cond;
            val = value;
        }
    }

    class KMeansMaster
    {
        public bool IsLoaded; // загружен ли контекст
        private NumericalTriadicContext context; // контекст в традиционном представлении
        private List<NumericalTriplet> tlist; // контекст в виде списка триплетов
        private List<int> cluster; // номера кластеров
        private List<int> centroid; // индексы центроидов в списке триплетов
        private int clusterCount;
        private double gamma;

        public KMeansMaster(int k, double gm = 0)
        {
            IsLoaded = false;
            clusterCount = k;
            gamma = gm;
        }

        private double dist(NumericalTriplet p1, NumericalTriplet p2)
        {
            return Math.Abs(p1.val - p2.val) + gamma * (p1.o != p2.o ? 1 : 0) + gamma * (p1.a != p2.a ? 1 : 0) + gamma * (p1.c != p2.c ? 1 : 0);
        }

        public List<Tricluster> Triclustering(out int iterCount, int initStyle = 0, int maxIter = 0)
        {
            cluster = Enumerable.Repeat(0, tlist.Count).ToList();
            centroid = Enumerable.Repeat(0, clusterCount).ToList();
            switch (initStyle) // Select centroids
            {
                case 0:
                    Random rand = new Random();
                    for (int i = 0; i < this.clusterCount; ++i)
                    {
                        centroid[i] = rand.Next(tlist.Count);
                        cluster[centroid[i]] = i;
                    }
                    break;

                case 1:
                    List<int> ls = new List<int>() {0, tlist.Count - 1}; //{0, 2, 6}; //{1, 160, 1343};
                    for (int i = 0; i < this.clusterCount; ++i)
                    {
                        centroid[i] = ls[i];
                        cluster[centroid[i]] = i;
                    }
                    break;

                default: throw new Exception("unknown InitStyle");
            }

            int iterCounter = 0;
            List<List<int>> clusterLists;
            while (true)
            {
                bool clustersChanged = false;
                clusterLists = new List<List<int>> (clusterCount);
                for (int i = 0; i < clusterCount; ++i)
                    clusterLists.Add(new List<int>());

                // Count clasters
                for (int i = 0; i < tlist.Count; ++i)
                {
                    double dmin = dist(tlist[i], tlist[centroid[cluster[i]]]);
                    for (int j = 0; j < centroid.Count; ++j)
                        if (dist(tlist[i], tlist[centroid[j]]) < dmin)
                        {
                            clustersChanged = true;
                            cluster[i] = j;
                            dmin = dist(tlist[i], tlist[centroid[j]]);
                        }
                    clusterLists[cluster[i]].Add(i);
                }
                
                // Redefine centroids
                for (int clusterid = 0; clusterid < clusterCount; ++clusterid)
                {
                    List<double> sumdist = Enumerable.Repeat(0d, clusterLists[clusterid].Count).ToList(); ;
                    for (int i = 0; i < clusterLists[clusterid].Count; ++i)
                    {
                        sumdist[i] = 0;
                        int p = clusterLists[clusterid][i];
                        foreach (int j in clusterLists[clusterid])
                        {
                            sumdist[i] += dist(tlist[p],tlist[j]);
                        }
                    }

                    centroid[clusterid] = clusterLists[clusterid][sumdist.IndexOf(sumdist.Min())];
                }

                // Check exit and iteration maximum
                if (!clustersChanged)
                    break;
                ++iterCounter;
                if (maxIter > 0 && iterCounter > maxIter)
                    break;
            }

            //form answer
            List<Tricluster> ans = new List<Tricluster>();
            foreach (List<int> cl in clusterLists)
            {
                HashSet<string> extent = new HashSet<string>();
                HashSet<string> intent = new HashSet<string>();
                HashSet<string> modus = new HashSet<string>();

                foreach (int i in cl)
                {
                    NumericalTriplet t = tlist[i];
                    extent.Add(t.o);
                    intent.Add(t.a);
                    modus.Add(t.c);
                }

                ans.Add(new Tricluster(extent.ToList(), intent.ToList(), modus.ToList()));
            }

            iterCount = iterCounter;
            return ans;
        }

        private List<NumericalTriplet> _ContextToList(NumericalTriadicContext context)
        {
            List<NumericalTriplet> list = new List<NumericalTriplet>();

            foreach (string o in context.Objects)
                foreach (string a in context.Attributes)
                    foreach (string c in context.Conditions)
                        if (context.Contains(o,a,c))
                            list.Add(new NumericalTriplet(o, a, c, context.Value(o,a,c)));

            return list;
        }

        public void LoadContext(NumericalTriadicContext context)
        {
            IsLoaded = true;
            this.context = context;
            this.tlist = _ContextToList(context);
            this.cluster = new List<int>(context.Count);
        }
    }
}
