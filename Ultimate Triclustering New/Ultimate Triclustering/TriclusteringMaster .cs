using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Ultimate_Triclustering
{
    class TriclusteringMaster // содержит все методы трикластеризации
    {
        public bool IsLoaded; // загружен ли контекст
        private TriadicContext context; // контекст
        private MD5 md5Hash;
        private Parsers.CParser parser; // парсер условий для DataPeeler'а

        public TriclusteringMaster(MD5 md5Hash)
        {
            IsLoaded = false;
            this.md5Hash = md5Hash;
            parser = new Parsers.CParser();
        }

        public TriclusteringMaster(TriadicContext context, MD5 md5Hash)
        {
            IsLoaded = true;
            this.context = context;
            this.md5Hash = md5Hash;
            parser = new Parsers.CParser();
        }

        public void LoadContext(TriadicContext context) // загружает контекст
        {
            IsLoaded = true;
            this.context = context;
            
        }


        public void CalculateDensities(List<Tricluster> list) // вычисляет плотности трикластеров
        {
            foreach (Tricluster t in list)
            {
                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(t);
                t.density = denCov.Key;
                t.coverage = denCov.Value;
                t.coverageG = ((double)t.extent.Count) / context.Objects.Count;
                t.coverageM = ((double)t.intent.Count) / context.Attributes.Count;
                t.coverageB = ((double)t.modus.Count) / context.Conditions.Count;
            }
        }

        public void CalculateDensitiesPrime(List<Tricluster> list) // вычисляет плотности трикластеров и их оценки для OAC (prime)
        {
            foreach (Tricluster t in list)
            {
                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(t);
                t.density = denCov.Key;
                t.coverage = denCov.Value;
                t.coverageG = ((double)t.extent.Count) / context.Objects.Count;
                t.coverageM = ((double)t.intent.Count) / context.Attributes.Count;
                t.coverageB = ((double)t.modus.Count) / context.Conditions.Count;
                double sum = t.extent.Count + t.intent.Count + t.modus.Count - 2, prod = t.extent.Count * t.intent.Count * t.modus.Count;
                t.altDensity1 = (double)(sum) / (prod);
                t.altDensity2 = (double)(sum + (prod - sum) * (context.Count - sum) / (context.Volume - sum)) / (prod);
                t.altDensity3 = (double)(sum + (prod - sum) * (context.Count) / (context.Volume)) / (prod);
            }
        }

        public void DecypherNames(List<Tricluster> list) // заменяет индексы в каждом трикластере на имена объектов / признаков / условий
        {
            foreach (Tricluster t in list)
            {
                context.DecypherTricluster(t);
            }
        }

        public List<int> ContextCounts() // возвращает число объектов / признаков / условий
        {
            List<int> list = new List<int>();
            list.Add(context.Objects.Count);
            list.Add(context.Attributes.Count);
            list.Add(context.Conditions.Count);
            return list;
        }

        public double TotalCoverage(List<Tricluster> list) // для множества вычисляет его общее покрытие
        {
            ConcurrentDictionary<string, Dictionary<string, Dictionary<string, bool>>> dict = new ConcurrentDictionary<string, Dictionary<string, Dictionary<string, bool>>>();
            Parallel.ForEach(context.Objects, o =>
            {
                dict.TryAdd(o, new Dictionary<string, Dictionary<string, bool>>());
                foreach (string a in context.Attributes)
                {
                    dict[o].Add(a, new Dictionary<string, bool>());
                    foreach (string c in context.Conditions)
                    {
                        dict[o][a].Add(c, false);
                    }
                }
            });

            double count = 0;

            foreach(Tricluster t in list)
            {
                foreach (string o in t.extent)
                {
                    foreach (string a in t.intent)
                    {
                        foreach (string c in t.modus)
                        {
                            if(context.Contains(o,a,c))
                            {
                                if (!dict[o][a][c])
                                {
                                    dict[o][a][c] = true;
                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            return (count / context.Count);
        }

        public double CoverageExtent(List<Tricluster> list) // для множества трикластеров вычисляет покрытие множества объектов
        {
            List<string> objects = new List<string>();

            foreach (Tricluster t in list)
            {
                objects = (objects.Union(t.extent)).ToList<string>();
            }

            return ((double)objects.Count / context.Objects.Count);
        }

        public double CoverageIntent(List<Tricluster> list) // для множества трикластеров вычисляет покрытие множества признаков
        {
            List<string> attributes = new List<string>();

            foreach (Tricluster t in list)
            {
                attributes = (attributes.Union(t.intent)).ToList<string>();
            }

            return ((double)attributes.Count / context.Attributes.Count);
        }

        public double CoverageModus(List<Tricluster> list) // для множества трикластеров вычисляет покрытие множества условий
        {
            List<string> conditions = new List<string>();

            foreach (Tricluster t in list)
            {
                conditions = (conditions.Union(t.modus)).ToList<string>();
            }

            return ((double)conditions.Count / context.Conditions.Count);
        }

        public double TotalDiversity(List<Tricluster> list) // для множества трикластеров вычисляет общее разнообразие
        {
            double intersections = 0;

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].extent.Intersect(list[j].extent).Count() != 0 && list[i].intent.Intersect(list[j].intent).Count() != 0 && list[i].modus.Intersect(list[j].modus).Count() != 0)
                    {
                        intersections++;
                    }
                }
            }

            return (1.0 - intersections / (list.Count * (list.Count - 1.0) / 2.0));
        }

        public double DiversityExtent(List<Tricluster> list) // для множества трикластеров вычисляет разнообразие по множеству объектов
        {
            double intersections = 0;

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].extent.Intersect(list[j].extent).Count() != 0)
                    {
                        intersections++;
                    }
                }
            }

            return (1.0 - intersections / (list.Count * (list.Count - 1.0) / 2.0));
        }

        public double DiversityIntent(List<Tricluster> list) // для множества трикластеров вычисляет разнообразие по множеству признаков
        {
            double intersections = 0;

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].intent.Intersect(list[j].intent).Count() != 0)
                    {
                        intersections++;
                    }
                }
            }

            return (1.0 - intersections / (list.Count * (list.Count - 1.0) / 2.0));
        }

        public double DiversityModus(List<Tricluster> list) // для множества трикластеров вычисляет разнообразие по множеству условий
        {
            double intersections = 0;

            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[i].modus.Intersect(list[j].modus).Count() != 0)
                    {
                        intersections++;
                    }
                }
            }

            return (1.0 - intersections / (list.Count * (list.Count - 1.0) / 2.0));
        }

        public List<KeyValuePair<Tricluster, double>> TestDiagContSimilarity(List<Tricluster> list) // для теста на шумоустойчивость для каждого трикластера вычисляет его лучшее сходство с одним из 3 кубоидов на диагонали размера 10х10х10
        {
            List<Tricluster> real = new List<Tricluster>();
            List<Tricluster> curBestTri = new List<Tricluster>();
            List<double> curBestSims = new List<double>();
            double cur;
            int cubSize = 10;
            for (int i = 0; i < 3; i++)
            {
                curBestSims.Add(double.MinValue);
                curBestTri.Add(new Tricluster());
                Tricluster temp = new Tricluster();
                for (int j = 0; j < cubSize; j++)
                {
                    temp.extent.Add((i * cubSize + j).ToString());
                    temp.intent.Add((i * cubSize + j).ToString());
                    temp.modus.Add((i * cubSize + j).ToString());
                }
                real.Add(temp);
            }

            foreach (Tricluster t in list)
            {
                for (int i = 0; i < real.Count; i++)
                {
                    cur = (((double)t.extent.Intersect(real[i].extent).Count()) / t.extent.Union(real[i].extent).Count()) * (((double)t.intent.Intersect(real[i].intent).Count()) / t.intent.Union(real[i].intent).Count()) *
                        (((double)t.modus.Intersect(real[i].modus).Count()) / t.modus.Union(real[i].modus).Count());

                    if (cur > curBestSims[i])
                    {
                        curBestSims[i] = cur;
                        curBestTri[i] = t;
                    }
                }
            }

            List<KeyValuePair<Tricluster, double>> result = new List<KeyValuePair<Tricluster,double>>();

            for (int i = 0; i < real.Count; i++)
            {
                result.Add(new KeyValuePair<Tricluster, double>(curBestTri[i], curBestSims[i]));
            }

            return result;
        }

        public List<Tricluster> Triclustering(string method, bool parallel, List<string> constraints,  double opt1, double opt2 = 0, double opt3 = 0) // General method for triclustering. Runs corresponding triclustering method with given options and returns the resulting tricluster set.
        {
            if (parallel)
            {
                switch (method)
                {
                    case "OAC (box)":
                        /*if (isFast)
                        {
                            return Triclustering_OACBox_parallel_mc(options);
                        }
                        else
                        {
                            return Triclustering_OACBox_parallel_tc(options);
                        }*/
                        return Triclustering_OACBox_parallel(opt1);
                    case "OAC (prime)":
                        return Triclustering_OACPrime_parallel(opt1);
                    /*case "Spectral":
                        return Triclustering_Spectral_parallel(options);*/
                    case "TBox":
                        return Triclustering_TBox_parallel(opt1);
                    case "Trias":
                        return Triclustering_Trias_parallel(opt1, opt2, opt3);
                    default:
                        return new List<Tricluster>();
                }
            }
            else
            {
                switch (method)
                {
                    case "OAC (box)":
                        /*if (isFast)
                        {
                            return Triclustering_OACBox_norm_mc(options);
                        }
                        else
                        {
                            return Triclustering_OACBox_norm_tc(options);
                        }*/
                        return Triclustering_OACBox_norm(opt1);
                    case "OAC (prime)":
                        return Triclustering_OACPrime_norm(opt1);
                    case "Spectral":
                        return Triclustering_Spectral_norm(opt1);
                    case "TBox":
                        return Triclustering_TBox_norm(opt1);
                    case "Trias":
                        return Triclustering_Trias_norm(opt1, opt2, opt3);
                    case "DataPeeler":
                        return Triclustering_DataPeeler_norm(constraints, new Tricluster(), new Tricluster(context.Objects, context.Attributes, context.Conditions).Copy(), new Tricluster(), new List<Tricluster>());
                    default:
                        return new List<Tricluster>();
                }
            }
        }

        private List<Tricluster> Triclustering_Trias_parallel(double opt1, double opt2, double opt3)
        {
            throw new NotImplementedException();
        }


        #region Normal algorithms

        #region commented

        /*private List<Tricluster> Triclustering_OACBox_norm_mc(double minDensity) // OAC-triclustering based on box-operators
        {
            SortedSet<Tricluster> triclusterSet = new SortedSet<Tricluster>();
            //double countStep = 0;
            //double totalSteps = context.Extent.Count * context.Intent.Count * context.Modus.Count;*/

            /*Dictionary<string, List<KeyValuePair<string, string>>> oc = new Dictionary<string, List<KeyValuePair<string, string>>>();
            foreach(string a in context.Intent)
            {
                oc.Add(a, context.primeA(a));
                //countStep++;
                //pbValue = Convert.ToInt32(countStep / context.Intent.Count * 5);
            }

            //countStep = 0;
            Dictionary<string, List<KeyValuePair<string, string>>> oa = new Dictionary<string, List<KeyValuePair<string, string>>>();
            foreach(string c in context.Modus)
            {
                oa.Add(c, context.primeC(c));
                //countStep++;
                //pbValue = Convert.ToInt32(countStep / context.Intent.Count * 5 + 5);
            }

            //countStep = 0;
            Dictionary<string, List<KeyValuePair<string, string>>> ac = new Dictionary<string, List<KeyValuePair<string, string>>>();
            foreach (string o in context.Extent)
            {
                oa.Add(o, context.primeO(o));
                //countStep++;
                //pbValue = Convert.ToInt32(countStep / context.Intent.Count * 5 + 5);
            }

            Dictionary<KeyValuePair<string, string>, List<string>> oSet = new Dictionary<KeyValuePair<string, string>, List<string>>();
            foreach (string a in context.Intent)
            {
                foreach (string c in context.Modus)
                {
                    oSet.Add(new KeyValuePair<string, string>(a, c), context.boxO(oa[c], oc[a]));
                }
            }

            Dictionary<KeyValuePair<string, string>, List<string>> aSet = new Dictionary<KeyValuePair<string, string>, List<string>>();
            foreach (string o in context.Extent)
            {
                foreach (string c in context.Modus)
                {
                    aSet.Add(new KeyValuePair<string, string>(o, c), context.boxA(oa[c], ac[o]));
                }
            }

            Dictionary<KeyValuePair<string, string>, List<string>> cSet = new Dictionary<KeyValuePair<string, string>, List<string>>();
            foreach (string o in context.Extent)
            {
                foreach (string a in context.Intent)
                {
                    cSet.Add(new KeyValuePair<string, string>(o, a), context.boxC(oc[a], ac[o]));
                }
            }

            foreach (string o in context.Extent)
            {
                foreach (string a in context.Intent)
                {
                    foreach (string c in context.Modus)
                    {
                        if (context.get(o, a, c))
                        {
                            Tricluster temp = new Tricluster(oSet[new KeyValuePair<string,string>(a,c)], aSet[new KeyValuePair<string,string>(o,c)], cSet[new KeyValuePair<string,string>(o,a)]);
                            if (minDensity > 0)
                            {
                                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp);
                                if (denCov.Key >= minDensity)
                                {
                                    //temp.extent.Sort();
                                    //temp.intent.Sort();
                                    //temp.modus.Sort();
                                    temp.CalcHash(md5Hash);
                                    temp.density = denCov.Key;
                                    temp.coverage = denCov.Value;
                                    bool flag = false;
                                    foreach (Tricluster t in triclusterSet)
                                    {
                                        if (temp == t)
                                        {
                                            flag = true;
                                            break;
                                        }
                                        if (!flag)
                                        {
                                            triclusterSet.Add(temp);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool flag = false;
                                foreach (Tricluster t in triclusterSet)
                                {
                                    if (temp == t)
                                    {
                                        flag = true;
                                        break;
                                    }
                                    if (!flag)
                                    {
                                        triclusterSet.Add(temp);
                                    }
                                }
                            }

                            //countStep++;
                            //pbValue = Convert.ToInt32(countStep / totalSteps * 80 + 10);
                        }
                    }
                }
            }*/

            /*KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>>> boxes = context.generalBoxes();
            Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>> oSet = boxes.Key;
            Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>> aSet = boxes.Value.Key;
            Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>> cSet = boxes.Value.Value;*/

            /*KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>>> primes = context.generalPrimes();
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oa = primes.Key;
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oc = primes.Value.Key;
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> ac = primes.Value.Value;*/

            /*foreach (string o in context.Extent)
            {
                foreach (string a in context.Intent)
                {
                    foreach (string c in context.Modus)
                    {
                        if (context.get(o, a, c))
                        {
                            KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o, new KeyValuePair<string, string>(a, c));
                            Tricluster temp = new Tricluster(oSet[cur], aSet[cur], cSet[cur]);
                            //Tricluster temp = new Tricluster(context.boxO(oa[c].Key.ToList<string>(), oc[a].Key.ToList<string>()), context.boxA(oa[c].Value.ToList<string>(), ac[o].Key.ToList<string>()), context.boxC(oc[a].Value.ToList<string>(), ac[o].Value.ToList<string>()));
                            if (minDensity > 0)
                            {
                                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp);
                                if (denCov.Key >= minDensity)
                                {
                                    //temp.extent.Sort();
                                    //temp.intent.Sort();
                                    //temp.modus.Sort();
                                    //temp.CalcIHash();
                                    temp.density = denCov.Key;
                                    temp.coverage = denCov.Value;
                                    /*bool flag = false;
                                    foreach (Tricluster t in triclusterSet)
                                    {
                                        if (temp == t)
                                        {
                                            flag = true;
                                            break;
                                        }
                                        if (!flag)
                                        {
                                            triclusterSet.Add(temp);
                                        }
                                    }*/
                                    /*triclusterSet.Add(temp);
                                }
                            }
                            else
                            {
                                //temp.CalcIHash();
                                /*bool flag = false;
                                foreach (Tricluster t in triclusterSet)
                                {
                                    if (temp == t)
                                    {
                                        flag = true;
                                        break;
                                    }
                                    if (!flag)
                                    {
                                        triclusterSet.Add(temp);
                                    }
                                }*/
                                /*triclusterSet.Add(temp);
                            }
                        }
                    }
                }
            }*/

            /*//countStep = 0;
            Stack<int> todel = new Stack<int>();
            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusterSet[i] == triclusterSet[j])
                    {
                        todel.Push(i);
                        break;
                    }
                }
                //countStep++;
                //pbValue = Convert.ToInt32(countStep / triclusterSet.Count * 5 + 90);
            }

            //countStep = 0;
            totalSteps = todel.Count;
            while (todel.Count != 0)
            {
                triclusterSet.RemoveAt(todel.Pop());
                //countStep++;
                //pbValue = Convert.ToInt32(countStep / totalSteps * 5 + 95);
            }*/

        /*return triclusterSet.ToList<Tricluster>();
    }*/

        #endregion

        private List<Tricluster> Triclustering_OACBox_norm(double minDensity) // OAC-triclustering based on box-operators
        {
            HashSet<Tricluster> triclusterSet = new HashSet<Tricluster>(); // будущее множество трикластеров

            KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>>> primes = context.generalPrimes(); // получаем все возможные множества штрихов для пар
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oa = primes.Key; // множество пар вида (объект, признак)
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oc = primes.Value.Key; // множество пар вида (объект, условие)
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> ac = primes.Value.Value; // множество пар вида (признак, условие)

            foreach (string o in context.Objects) // в идеале можно задать перечисление троек контекста и идти по нему, должно быть быстрее
            {
                foreach (string a in context.Attributes)
                {
                    foreach (string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            // KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o, new KeyValuePair<string, string>(a, c));
                            Tricluster temp = new Tricluster(context.boxO(oa[c].Key.ToList<string>(), oc[a].Key.ToList<string>()), context.boxA(oa[c].Value.ToList<string>(), ac[o].Key.ToList<string>()), context.boxC(oc[a].Value.ToList<string>(), ac[o].Value.ToList<string>()));
                            if (minDensity > 0) // если есть ненулеое условие на минимальную плотность
                            {
                                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp); // вычисляем плотность возможного трикластера (а заодно и покрытие)
                                if (denCov.Key >= minDensity) // если условие на минимальную плотность выполнено
                                {
                                    temp.density = denCov.Key; // добавляем в трикластер посчитанные данные, немного досчитываем ещё несколько тривиальных мер (покрытия множеств)
                                    temp.coverage = denCov.Value;
                                    temp.coverageG = ((double)temp.extent.Count) / context.Objects.Count;
                                    temp.coverageM = ((double)temp.intent.Count) / context.Attributes.Count;
                                    temp.coverageB = ((double)temp.modus.Count) / context.Conditions.Count;
                                    temp.CalcIHash(); // вычисляем хеш-код
                                    triclusterSet.Add(temp); // добавляем трикластер ко множеству, если такого ещё нет
                                }
                            }
                            else
                            {
                                temp.CalcIHash();
                                triclusterSet.Add(temp); // просто пытаемся добавить трикластер ко множеству (меры будут досчитаны после)
                            }
                        }
                    }
                }
            }

            return triclusterSet.ToList<Tricluster>();
        }

        private List<Tricluster> Triclustering_OACPrime_norm(double minDensity) // OAC-triclustering based on prime-operators
        {
            HashSet<Tricluster> triclusterSet = new HashSet<Tricluster>();

            KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, Dictionary<KeyValuePair<string, string>, SortedSet<string>>>> primes = context.generalPrimesPair(); // получаем все возможные множества штрихов для пар
            Dictionary<KeyValuePair<string, string>, SortedSet<string>> cSet = primes.Key; // разделяем полученный словарь на нужные множества
            Dictionary<KeyValuePair<string, string>, SortedSet<string>> aSet = primes.Value.Key;
            Dictionary<KeyValuePair<string, string>, SortedSet<string>> oSet = primes.Value.Value;

            foreach (string o in context.Objects) // в идеале можно задать перечисление троек контекста и идти по нему, должно быть быстрее
            {
                foreach (string a in context.Attributes)
                {
                    foreach (string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            Tricluster temp = new Tricluster(oSet[new KeyValuePair<string, string>(a, c)].ToList<string>(), aSet[new KeyValuePair<string, string>(o, c)].ToList<string>(), cSet[new KeyValuePair<string, string>(o, a)].ToList<string>());
                            if (minDensity > 0) // если есть ненулеое условие на минимальную плотность
                            {
                                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp); // вычисляем плотность возможного трикластера (а заодно и покрытие)
                                if (denCov.Key >= minDensity) // если условие на минимальную плотность выполнено
                                {
                                    //temp.CalcHash(md5Hash);
                                    temp.density = denCov.Key; // добавляем в трикластер посчитанные данные, немного досчитываем ещё несколько тривиальных мер (покрытия множеств и оценки плотности)
                                    temp.coverage = denCov.Value;
                                    double sum = temp.extent.Count + temp.intent.Count + temp.modus.Count - 2, prod = temp.extent.Count * temp.intent.Count * temp.modus.Count;
                                    temp.altDensity1 = (double)(sum) / (prod);
                                    temp.altDensity2 = (double)(sum + (prod - sum) * (context.Count - sum) / (context.Volume)) / (prod);
                                    temp.altDensity3 = (double)(sum + (prod - sum) * (context.Count) / (context.Volume)) / (prod);
                                    temp.coverageG = ((double)temp.extent.Count) / context.Objects.Count;
                                    temp.coverageM = ((double)temp.intent.Count) / context.Attributes.Count;
                                    temp.coverageB = ((double)temp.modus.Count) / context.Conditions.Count;
                                    temp.CalcIHash(); // вычисляем хеш-код
                                    triclusterSet.Add(temp); // добавляем трикластер ко множеству, если такого ещё нет
                                }
                            }
                            else
                            {
                                temp.CalcIHash();
                                triclusterSet.Add(temp); // просто пытаемся добавить трикластер ко множеству (меры будут досчитаны после)
                            }
                        }
                    }
                }
            }

            return triclusterSet.ToList<Tricluster>();
        }

        private List<Tricluster> Triclustering_Spectral_norm(double minSize) // Spectral triclustering
        {
            List<Tricluster> triclusterSet = new List<Tricluster>();

            // проверка вычисления собственных чисел и свойств матрицы Лапласа
            /*double[,] matrix = new double[6, 6] {{2,-1,0,0,0,-1},{-1,3,-1,0,0,-1},{0,-1,3,-1,-1,0},{0,0,-1,2,-1,0},{0,0,-1,-1,2,0},{-1,-1,0,0,0,2}};

            double[] values = new double[6];
            double[,] vectors = new double[6, 6];

            alglib.smatrixevd(matrix, 6, 1, true, out values, out vectors);*/

            List<string> extent = context.Objects, intent = context.Attributes, modus = context.Conditions; // исходный "трикластер"
            int dim = extent.Count + intent.Count + modus.Count; // число строк / столбцов матрицы (вершин графа)
            double[,] matrix = new double[dim, dim]; // сама матрица - !!!ОЧЕНЬ ПЛОХО, ЧТО НЕ РАЗРЕЖЕННАЯ, НУЖНО ИСПРАВИТЬ С ИСПОЛЬЗОВАНИЕМ СООТВЕТСТВУЮЩИХ АЛГОРИТМОВ!!!
            int[] indicator = new int[dim]; // обозначает, какая вершина принадлежит какому трикластеру
            //Dictionary<int, int[]> inds = new Dictionary<int, int[]>();
            Dictionary<int, int> countTri = new Dictionary<int, int>(); // число вершин в соответствующем трикластере
            List<double[,]> matrices = new List<double[,]>(); // список подматриц - !!!НУЖНО ПЕРЕДЕЛАТЬ НА РАЗРЕЖЕННЫЕ!!!
            int curInd = 0;

            for (int i = 0; i < dim; i++)
            {
                countTri.Add(i, 0);
            }
            countTri[curInd] = dim;

            // creation of the matrix of the triadic context

            for (int i = 0; i < dim; i++) // изначально матрица - нулевая
            {
                for (int j = 0; j < dim; j++)
                {
                    matrix[i, j] = 0;
                }
            }

            for (int i = 0; i < extent.Count; i++) // минус матрица смежности
            {
                for (int j = 0; j < intent.Count; j++)
                {
                    for (int k = 0; k < modus.Count; k++)
                    {
                        if (context.Contains(extent[i], intent[j], modus[k]))
                        {
                            matrix[i, extent.Count + j] = -1;
                            matrix[i, extent.Count + intent.Count + k] = -1;
                            matrix[extent.Count + j, extent.Count + intent.Count + k] = -1;

                            matrix[extent.Count + j, i] = -1;
                            matrix[extent.Count + intent.Count + k, i] = -1;
                            matrix[extent.Count + intent.Count + k, extent.Count + j] = -1;
                        }
                    }
                }
            }

            double count;

            for (int i = 0; i < dim; i++) // модифицируем до матрицы Лапласа
            {
                count = 0;
                for (int j = 0; j < dim; j++)
                {
                    count -= matrix[i, j];
                }
                matrix[i, i] = count;
            }

            bool flag = true; // показывает, подходит ли данное дробление (содержит ли трикластер по крайней мере по одной вершине из каждого множества)
            //int iteration = 0;

            while ( ( ((double)(countTri.Values.Max())/dim) > minSize ) && flag ) // iteratively runs eigenvalues decomposition on all submatrices
            {
                flag = false;
                matrices = new List<double[,]>();
                foreach (int i in countTri.Keys) // преобразовываем множество трикластеров в матрицу с помощью сглаживающего преобразования
                {
                    if (countTri[i] > 0)
                    {
                        double[,] invD = new double[countTri[i], countTri[i]];
                        double[,] tempM = new double[countTri[i], countTri[i]];
                        int countRows = 0, countColumns;
                        for (int j = 0; j < dim; j++)
                        {
                            countColumns = 0;
                            if (indicator[j] == i)
                            {
                                for (int k = 0; k < dim; k++)
                                {
                                    if (indicator[k] == i)
                                    {
                                        tempM[countRows, countColumns] = matrix[j, k];
                                        tempM[countColumns, countRows] = matrix[k, j];
                                        countColumns++;
                                    }
                                }
                                countRows++;
                            }
                        }

                        for (int j = 0; j < countTri[i]; j++)
                        {
                            count = 0;
                            for (int k = 0; k < countTri[i]; k++)
                            {
                                if (k != j)
                                {
                                    count -= tempM[j, k];
                                    invD[j, k] = 0;
                                }
                            }
                            tempM[j, j] = count;
                            invD[j, j] = 1.0 / count;
                        }
                        double[,] tempMatrix = new double[countTri[i], countTri[i]];
                        alglib.rmatrixgemm(countTri[i], countTri[i], countTri[i], 1, invD, 0, 0, 0, tempM, 0, 0, 0, 0, ref tempMatrix, 0, 0); // преобразовываем исходную подматрицу, чтобы "подстроиться" под обобщённую задачу поиска собственных векторов
                        matrices.Add(tempMatrix);
                    }
                }

                for (int i = 0; i < matrices.Count; i++)
                {
                    if (((double)countTri[i]) / dim <= minSize) // если трикластер достаточно мал - не дробим его
                    {
                        continue;
                    }
                    double[] tempVal;
                    double[,] tempVec;
                    int ind;

                    alglib.smatrixevd(matrices[i], countTri[i], 1, true, out tempVal, out tempVec); // считаем собственные вектора

                    int cpos = 0, cneg = 0; // число положительных и отрицательных элементов второго наименьшего собственного вектора

                    for(int j = 0; j < countTri[i]; j++)
                    {
                        if(Math.Round(tempVec[j,1],6) >= 0)
                        {
                            cpos++;
                        }
                        else
                        {
                            cneg++;
                        }
                    }

                    int[] tempIndicator = new int[dim]; // индикаторный вектор, если мы разобьём контекст, как указывает вектор

                    for (int j = 0; j < dim; j++)
                    {
                        tempIndicator[j] = indicator[j];
                    }

                    if (cpos >= 3 && cneg >= 3) // если достаточно много положительных и отрицательных объектов (иначе не может быть по крайней мере по 1 вершине из каждого из 3 множеств)
                    {
                        ind = 0;

                        curInd++;

                        for (int j = 0; j < extent.Count; j++)
                        {
                            if (tempIndicator[j] == i)
                            {
                                if (Math.Round(tempVec[ind, 1], 6) >= 0)
                                {
                                    tempIndicator[j] = curInd;
                                }

                                ind++;
                            }
                        }
                        for (int j = extent.Count; j < extent.Count + intent.Count; j++)
                        {
                            if (tempIndicator[j] == i)
                            {
                                if (Math.Round(tempVec[ind, 1], 6) >= 0)
                                {
                                    tempIndicator[j] = curInd;
                                }

                                ind++;
                            }
                        }
                        for (int j = extent.Count + intent.Count; j < dim; j++)
                        {
                            if (tempIndicator[j] == i)
                            {
                                if (Math.Round(tempVec[ind,1], 6) >= 0)
                                {
                                    tempIndicator[j] = curInd;
                                }

                                ind++;
                            }
                        }

                        List<bool> indicators = new List<bool>();
                        bool tempFlag = true;
                        for (int j = 0; j <= tempIndicator.Max(); j++) // проверяем, все ли трикластеры имеют вершины из множества объектов
                        {
                            indicators.Add(false);
                        }
                        for (int j = 0; j < extent.Count; j++)
                        {
                            indicators[tempIndicator[j]] = true;
                        }
                        for (int j = 0; j <= tempIndicator.Max(); j++)
                        {
                            tempFlag = tempFlag && indicators[j];
                        }
                        if (!tempFlag)
                        {
                            curInd--;
                            continue;
                        }

                        indicators = new List<bool>();
                        tempFlag = true;
                        for (int j = 0; j <= tempIndicator.Max(); j++)  // проверяем, все ли трикластеры имеют вершины из множества признаков
                        {
                            indicators.Add(false);
                        }
                        for (int j = extent.Count; j < extent.Count + intent.Count; j++)
                        {
                            indicators[tempIndicator[j]] = true;
                        }
                        for (int j = 0; j <= tempIndicator.Max(); j++)
                        {
                            tempFlag = tempFlag && indicators[j];
                        }
                        if (!tempFlag)
                        {
                            curInd--;
                            continue;
                        }

                        indicators = new List<bool>();
                        tempFlag = true;
                        for (int j = 0; j <= tempIndicator.Max(); j++) // проверяем, все ли трикластеры имеют вершины из множества условий
                        {
                            indicators.Add(false);
                        }
                        for (int j = extent.Count + intent.Count; j < dim; j++)
                        {
                            indicators[tempIndicator[j]] = true;
                        }
                        for (int j = 0; j <= tempIndicator.Max(); j++)
                        {
                            tempFlag = tempFlag && indicators[j];
                        }
                        if (!tempFlag)
                        {
                            curInd--;
                            continue;
                        }

                        flag = true; // если мы дошли сюда, значит каждый трикластер имеет по крайней мере по 1 вершине из каждого из 3 множеств
                        indicator = tempIndicator; // принимаем изменения
                    }
                }

                for (int i = 0; i < dim; i++) // пересчитываем размеры трикластеров
                {
                    countTri[i] = 0;
                }

                for (int i = 0; i < dim; i++)
                {
                    countTri[indicator[i]]++;
                }

                /*if (flag)
                {
                    int[] tempInd = new int[dim];

                    for (int i = 0; i < dim; i++)
                    {
                        tempInd[i] = indicator[i];
                    }

                    inds.Add(iteration, tempInd);
                }*/
            }

            // transforming matrix into the set of triclusters

            for (int i = 0; i <= countTri.Keys.Max(); i++)
            {
                if (countTri[i] == 0)
                {
                    break;
                }
                triclusterSet.Add(new Tricluster());
            }

            for (int i = 0; i < extent.Count; i++)
            {
                triclusterSet[indicator[i]].extent.Add(extent[i]);
            }
            for (int i = extent.Count; i < extent.Count + intent.Count; i++)
            {
                triclusterSet[indicator[i]].intent.Add(intent[i - extent.Count]);
            }
            for (int i = extent.Count + intent.Count; i < dim; i++)
            {
                triclusterSet[indicator[i]].modus.Add(modus[i - extent.Count - intent.Count]);
            }

            /*KeyValuePair<double, double> denCov;
            for (int i = 0; i < triclusterSet.Count; i++)
            {
                denCov = context.CalculateDensityAndCoverage(triclusterSet[i]);
                triclusterSet[i].density = denCov.Key;
                triclusterSet[i].coverage = denCov.Value;
            }*/

            return triclusterSet;
        }

        private List<Tricluster> Triclustering_TBox_norm(double options) // TriBox
        {
            HashSet<Tricluster> triclusterSet = new HashSet<Tricluster>();

            foreach (string o in context.Objects) // в идеале можно задать перечисление троек контекста и идти по нему, должно быть быстрее
            {
                foreach(string a in context.Attributes)
                {
                    foreach(string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            Tricluster curTri = new Tricluster(); // начинаем строить новый трикластер с текущей тройки
                            curTri.extent.Add(o);
                            curTri.intent.Add(a);
                            curTri.modus.Add(c);
                            Tricluster temp = TBoxBody_norm(curTri); // достраиваем трикластер
                            if (temp.extent.Count > 0 && temp.intent.Count > 0 && temp.modus.Count > 0) // если все множества непусты - считаем хеш-код и добавляем
                            {
                                temp.CalcIHash();
                                triclusterSet.Add(temp);
                            }
                        }
                    }
                }
            }

            return triclusterSet.ToList<Tricluster>();
        }

        private double CalcD_norm(Tricluster tri, string star, char set) // вычисление функции D
        {
            double sum;
            double z = -1;
            double n = 0, m = 0, l = 0;
            List<string> starList = new List<string>();
            starList.Add(star);
            Tricluster temp = new Tricluster();
            switch (set)
            {
                case 'e':
                    /*if (tri.extent.Count == 1 && star == tri.extent[0])
                    {
                        return double.MinValue;
                    }*/
                    temp.extent = starList;
                    temp.intent = tri.intent;
                    temp.modus = tri.modus;
                    if(!tri.extent.Contains(star))
                    {
                        z = 1;
                    }
                    n = tri.extent.Count;
                    m = tri.intent.Count;
                    l = tri.modus.Count;
                    break;
                case 'i':
                    /*if (tri.intent.Count == 1 && star == tri.intent[0])
                    {
                        return double.MinValue;
                    }*/
                    temp.extent = tri.extent;
                    temp.intent = starList;
                    temp.modus = tri.modus;
                    if (!tri.intent.Contains(star))
                    {
                        z = 1;
                    }
                    n = tri.intent.Count;
                    m = tri.extent.Count;
                    l = tri.modus.Count;
                    break;
                case 'm':
                    /*if (tri.modus.Count == 1 && star == tri.modus[0])
                    {
                        return double.MinValue;
                    }*/
                    temp.extent = tri.extent;
                    temp.intent = tri.intent;
                    temp.modus = starList;
                    if (!tri.modus.Contains(star))
                    {
                        z = 1;
                    }
                    n = tri.modus.Count;
                    m=tri.extent.Count;
                    l=tri.intent.Count;
                    break;
                default:
                    return double.MinValue;
            }

            double r = 0, rStar = 0;

            foreach (string o in tri.extent)
            {
                foreach (string a in tri.intent)
                {
                    foreach (string c in tri.modus)
                    {
                        r += Convert.ToDouble(context.Contains(o, a, c)) - ((double)context.Count) / context.Volume;
                    }
                }
            }

            foreach (string o in temp.extent)
            {
                foreach (string a in temp.intent)
                {
                    foreach (string c in temp.modus)
                    {
                        rStar += Convert.ToInt32(context.Contains(o, a, c)) - ((double)context.Count) / context.Volume;
                    }
                }
            }

            sum = (rStar*rStar + 2*z*r*rStar - z*r*r/n) / ((n + z)*m*l);

            return sum;
        }

        private Tricluster TBoxBody_norm(Tricluster curTri) // Основная часть TriBox'а
        {
            double dStar;
            double D;
            KeyValuePair<string, char> best = new KeyValuePair<string, char>(); // лучший элемент по значению функции D (имя (индекс) и метка множества, к которому этот элемент принадлежит)

            while (true)
            {
                dStar = double.MinValue;
                for (int j = 0; j < context.Objects.Count; j++) // перебираем все объекты
                {
                    D = CalcD_norm(curTri, context.Objects[j], 'e'); // считаем значение функции D для текущего объекта
                    if (D > dStar) // если текущее значение - лучшее, запоминаем его
                    {
                        dStar = D;
                        best = new KeyValuePair<string, char>(context.Objects[j], 'e');
                    }
                }

                for (int j = 0; j < context.Attributes.Count; j++) // перебираем все признаки
                {
                    D = CalcD_norm(curTri, context.Attributes[j], 'i');
                    if (D > dStar)
                    {
                        dStar = D;
                        best = new KeyValuePair<string, char>(context.Attributes[j], 'i');
                    }
                }

                for (int j = 0; j < context.Conditions.Count; j++) // перебираем все условия
                {
                    D = CalcD_norm(curTri, context.Conditions[j], 'm');
                    if (D > dStar)
                    {
                        dStar = D;
                        best = new KeyValuePair<string, char>(context.Conditions[j], 'm');
                    }
                }

                if (dStar < 0) // если лучшее значение D - отрицательное, т.е. не выгодно отклониться от текущего трикластера
                {
                    KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(curTri); // считаем плотность и покрытие
                    curTri.density = denCov.Key;
                    curTri.coverage = denCov.Value;
                    curTri.extent.Sort();
                    curTri.intent.Sort();
                    curTri.modus.Sort();
                    //curTri.CalcHash(md5Hash);
                    return curTri;
                }
                else
                {
                    switch (best.Value) // иначе, в зависимости от того, к какому множеству принадлежит текущий элемент, добавляем или удаляем его к текущеу трикластеру
                    {
                        case 'e':
                            if (curTri.extent.Contains(best.Key))
                            {
                                curTri.extent.Remove(best.Key);
                            }
                            else
                            {
                                curTri.extent.Add(best.Key);
                            }
                            break;
                        case 'i':
                            if (curTri.intent.Contains(best.Key))
                            {
                                curTri.intent.Remove(best.Key);
                            }
                            else
                            {
                                curTri.intent.Add(best.Key);
                            }
                            break;
                        case 'm':
                            if (curTri.modus.Contains(best.Key))
                            {
                                curTri.modus.Remove(best.Key);
                            }
                            else
                            {
                                curTri.modus.Add(best.Key);
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        private List<Tricluster> Triclustering_Trias_norm(double tauG, double tauM, double tauB) // Trias
        {
            HashSet<Tricluster> triclusterSet = new HashSet<Tricluster>();

            //List<KeyValuePair<string, KeyValuePair<string, string>>> dContext = new List<KeyValuePair<string,KeyValuePair<string,string>>>();
            HashSet<KeyValuePair<string, string>> intent = new HashSet<KeyValuePair<string, string>>(); // множетво признаков внешнего контекста (пары вида (признак, условие))
            HashSet<string> extent = new HashSet<string>(); //  множество объектов внешнего контекста 
            Dictionary<string, List<KeyValuePair<string, string>>> utrCont = new Dictionary<string, List<KeyValuePair<string, string>>>(); // отношение внешнего контекста

            foreach (string o in context.Objects)
            {
                utrCont.Add(o, new List<KeyValuePair<string, string>>());
            }

            foreach (string o in context.Objects) // строим отношение внешнего контекста
            {
                foreach (string a in context.Attributes)
                {
                    foreach (string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            extent.Add(o);
                            intent.Add(new KeyValuePair<string, string>(a, c));
                            if (!utrCont[o].Contains(new KeyValuePair<string, string>(a, c)))
                            {
                                utrCont[o].Add(new KeyValuePair<string, string>(a, c));
                            }
                        }
                    }
                }
            }

            DyadicContext<string, KeyValuePair<string, string>> UTRContext = new DyadicContext<string, KeyValuePair<string, string>>(utrCont, extent.ToList<string>(), intent.ToList<KeyValuePair<string, string>>()); // строим внешний контекст

            KeyValuePair<List<string>, List<KeyValuePair<string, string>>> AI = FirstFrequentConcept(UTRContext, tauG); // в AI будем записывать понятия внешнего контекста; получаем первое понятие контекста
            KeyValuePair<List<string>, List<string>> BC; // в BC будем записывать понятия внутреннего контекста

            do
            {
                if (AI.Value.Count >= tauM * tauB) // если содержание AI достаточно большое
                {
                    DyadicContext<string, string> TRContext = new DyadicContext<string, string>(AI.Value); // строим на содержании AI внутренний контекст
                    BC = FirstFrequentConcept(TRContext, tauM); // получаем первое понятие внутреннего контекста
                    do
                    {
                        if (BC.Value.Count >= tauB) // если содержание BC достаточно большое
                        {
                            List<KeyValuePair<string, string>> cartProd = new List<KeyValuePair<string, string>>();
                            foreach (string b in BC.Key) // преобразовываем BC к виду множества пар (как признаки во внешнем контексте)
                            {
                                foreach (string c in BC.Value)
                                {
                                    cartProd.Add(new KeyValuePair<string, string>(b, c));
                                }
                            }
                            List<string> BCp = UTRContext.primeA(cartProd); // берём штрих от полученного множества

                            bool flag = (AI.Key.Count == BCp.Count); // проверяем равенство объёма AI и полученного множества
                            if (flag)
                            {
                                foreach (string str in AI.Key)
                                {
                                    if(!BCp.Contains(str))
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                            if (flag) // если равны - "сливаем" понятия в трикластер и добавляем в множество
                            {
                                Tricluster temp = new Tricluster();
                                foreach (string s in AI.Key)
                                {
                                    temp.extent.Add(s);
                                }
                                foreach (string s in BC.Key)
                                {
                                    temp.intent.Add(s);
                                }
                                foreach (string s in BC.Value)
                                {
                                    temp.modus.Add(s);
                                }
                                temp.CalcIHash();
                                triclusterSet.Add(temp);
                            }
                        }
                    }
                    while (NextFrequentConcept(TRContext, tauM, BC, out BC)); // строим следующее понятие внутреннего контекста, пока не построим все
                }
            }
            while (NextFrequentConcept(UTRContext, tauG, AI, out AI)); // строим следующее понятие внешнего контекста, пока не построим все

            return triclusterSet.ToList<Tricluster>();
        }

        private KeyValuePair<List<string>, List<KeyValuePair<string, string>>> FirstFrequentConcept(DyadicContext<string, KeyValuePair<string, string>> cont, double tau) // FFC для диадического контекста одиночный объект - пара (признак, условие)
        {
            List<KeyValuePair<string, string>> B = cont.primeO(new List<string>());
            List<string> A = cont.primeA(B);

            KeyValuePair<List<string>, List<KeyValuePair<string, string>>> AB = new KeyValuePair<List<string>, List<KeyValuePair<string, string>>>(A, B);

            if (A.Count < tau)
            {
                NextFrequentConcept(cont, tau, AB, out AB);
            }

            return AB;
        }

        private KeyValuePair<List<string>, List<string>> FirstFrequentConcept(DyadicContext<string, string> cont, double tau) // FFC для обычного диадического контекста
        {
            List<string> B = cont.primeO(new List<string>());
            List<string> A = cont.primeA(B);

            KeyValuePair<List<string>, List<string>> AB = new KeyValuePair<List<string>, List<string>>(A, B);

            if (A.Count < tau)
            {
                NextFrequentConcept(cont, tau, AB, out AB);
            }

            return AB;
        }

        private bool NextFrequentConcept(DyadicContext<string, KeyValuePair<string, string>> cont, double tau, KeyValuePair<List<string>, List<KeyValuePair<string, string>>> AB, out KeyValuePair<List<string>, List<KeyValuePair<string, string>>> outAB)
        {
            int i = cont.Objects.Count - 1;
            string g;
            List<string> G = new List<string>();
            foreach (string s in cont.Objects)
            {
                G.Add(s);
            }
            outAB = AB;

            List<string> tempA = new List<string>();
            List<KeyValuePair<string, string>> tempB = new List<KeyValuePair<string, string>>();
            foreach (string s in AB.Key)
            {
                tempA.Add(s);
            }

            while (i >= 0)
            {
                g = cont.Objects[i];
                G.Remove(g);
                if (!tempA.Contains(g))
                {
                    tempA = (tempA.Intersect(G)).ToList<string>();
                    tempA.Add(g);
                    tempB = cont.primeO(tempA);
                    List<string> D = cont.primeA(tempB);
                    List<string> DwA = (D.Except(tempA)).ToList<string>();
                    if ((DwA.Intersect(G)).Count() == 0)
                    {
                        outAB = new KeyValuePair<List<string>, List<KeyValuePair<string, string>>>(D, tempB);
                        if (D.Count < tau)
                        {
                            return NextFrequentConcept(cont, tau, outAB, out outAB);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                i--;
            }
            return false;
        }

        private bool NextFrequentConcept(DyadicContext<string, string> cont, double tau, KeyValuePair<List<string>, List<string>> AB, out KeyValuePair<List<string>, List<string>> outAB)
        {
            int i = cont.Objects.Count - 1;
            string g;
            List<string> G = new List<string>();
            foreach (string s in cont.Objects)
            {
                G.Add(s);
            }
            outAB = AB;

            List<string> tempA = new List<string>();
            List<string> tempB = new List<string>();
            foreach (string s in AB.Key)
            {
                tempA.Add(s);
            }

            while (i >= 0)
            {
                g = cont.Objects[i];
                G.Remove(g);
                if (!tempA.Contains(g))
                {
                    tempA = (tempA.Intersect(G)).ToList<string>();
                    tempA.Add(g);
                    tempB = cont.primeO(tempA);
                    List<string> D = cont.primeA(tempB);
                    List<string> DwA = (D.Except(tempA)).ToList<string>();
                    if ((DwA.Intersect(G)).Count() == 0)
                    {
                        outAB = new KeyValuePair<List<string>, List<string>>(D, tempB);
                        if (D.Count < tau)
                        {
                            return NextFrequentConcept(cont, tau, outAB, out outAB);
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
                i--;
            }
            return false;
        }

        List<Tricluster> Triclustering_DataPeeler_norm(List<string> constraints, Tricluster U, Tricluster V, Tricluster S, List<Tricluster> TriclusterSet) // Data Peeler
        {
            KeyValuePair<bool, string> pair;
            if (closed(U.Union(V), S))
            {
                foreach (string c in constraints)
                {
                    pair = parser.CalcConstraint(c, U, V, context);
                    if (pair.Value != null)
                    {
                        throw new FormatException(pair.Value);
                    }
                    else if (!pair.Key)
                    {
                        return TriclusterSet;
                    }
                }

                bool empty = true;
                for (int i = 0; i < 3; i++)
                {
                    empty = V.extent.Count == 0 && V.intent.Count == 0 && V.modus.Count == 0;
                }

                if (empty)
                {
                    TriclusterSet.Add(U.Copy());
                    return TriclusterSet;
                }
                else
                {
                    KeyValuePair<int, string> p = selectP(U, V);
                    switch (p.Key)
                    {
                        case 0:
                            V.extent.Remove(p.Value);
                            break;
                        case 1:
                            V.intent.Remove(p.Value);
                            break;
                        case 2:
                            V.modus.Remove(p.Value);
                            break;
                    }
                    Tricluster VNew = new Tricluster();
                    Tricluster tempClus;
                    for (int i = 0; i < 3; i++)
                    {
                        tempClus = U.Copy();
                        if (i == p.Key)
                        {
                            switch (i)
                            {
                                case 0:
                                    VNew.extent = V.extent;
                                    break;
                                case 1:
                                    VNew.intent = V.intent;
                                    break;
                                case 2:
                                    VNew.modus = V.modus;
                                    break;
                            }
                        }
                        else
                        {
                            switch (p.Key)
                            {
                                case 0:
                                    tempClus.extent = new List<string>() {p.Value};
                                    break;
                                case 1:
                                    tempClus.intent = new List<string>() { p.Value };
                                    break;
                                case 2:
                                    tempClus.modus = new List<string>() { p.Value };
                                    break;
                            }

                            switch (i)
                            {
                                case 0:
                                    foreach (string v in V.extent)
                                    {
                                        tempClus.extent = new List<string>() { v };
                                        if (connected(tempClus))
                                        {
                                            VNew.extent.Add(v);
                                        }
                                    }
                                    break;
                                case 1:
                                    foreach (string v in V.intent)
                                    {
                                        tempClus.intent = new List<string>() { v };
                                        if (connected(tempClus))
                                        {
                                            VNew.intent.Add(v);
                                        }
                                    }
                                    break;
                                case 2:
                                    foreach (string v in V.modus)
                                    {
                                        tempClus.modus = new List<string>() { v };
                                        if (connected(tempClus))
                                        {
                                            VNew.modus.Add(v);
                                        }
                                    }
                                    break;
                            }
                        }
                    }

                    switch (p.Key)
                    {
                        case 0:
                            U.extent.Add(p.Value);
                            break;
                        case 1:
                            U.intent.Add(p.Value);
                            break;
                        case 2:
                            U.modus.Add(p.Value);
                            break;
                    }
                    TriclusterSet = Triclustering_DataPeeler_norm(constraints, U, VNew, S, TriclusterSet);
                    switch (p.Key)
                    {
                        case 0:
                            U.extent.Remove(p.Value);
                            break;
                        case 1:
                            U.intent.Remove(p.Value);
                            break;
                        case 2:
                            U.modus.Remove(p.Value);
                            break;
                    }

                    switch (p.Key)
                    {
                        case 0:
                            S.extent.Add(p.Value);
                            break;
                        case 1:
                            S.intent.Add(p.Value);
                            break;
                        case 2:
                            S.modus.Add(p.Value);
                            break;
                    }
                    TriclusterSet = Triclustering_DataPeeler_norm(constraints, U, V.Copy(), S, TriclusterSet);
                    switch (p.Key)
                    {
                        case 0:
                            S.extent.Remove(p.Value);
                            break;
                        case 1:
                            S.intent.Remove(p.Value);
                            break;
                        case 2:
                            S.modus.Remove(p.Value);
                            break;
                    }
                }
            }
            return TriclusterSet;
        }

        bool connected(Tricluster cluster) // проверка условия связности трикластера
        {
            if (cluster.extent.Count == 0 || cluster.intent.Count == 0 || cluster.modus.Count == 0)
            {
                return true;
            }
            
            List<string> tuple = new List<string>();
            int[] indeces = new int[3];
            for (int i = 0; i < 3; i++)
            {
                indeces[i] = 0;
            }

            for (int i = 0; i < cluster.extent.Count; i++)
            {
                for (int j = 0; j < cluster.intent.Count; j++)
                {
                    for (int k = 0; k < cluster.modus.Count; k++)
                    {
                        if (!context.Contains(cluster.extent[i], cluster.intent[j], cluster.modus[k]))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        bool closed(Tricluster cluster, Tricluster S) // проверка условия замкнутости трикластера
        {
            Tricluster tempClus;

            for (Int32 i = 0; i < 3; i++)
            {
                switch (i)
                {
                    case 0:
                        foreach (string s in S.extent)
                        {
                            tempClus = cluster.Copy();
                            tempClus.extent = new List<string>() { s };
                            if (connected(tempClus))
                            {
                                return false;
                            }
                        }
                        break;
                    case 1:
                        foreach (string s in S.intent)
                        {
                            tempClus = cluster.Copy();
                            tempClus.intent = new List<string>() { s };
                            if (connected(tempClus))
                            {
                                return false;
                            }
                        }
                        break;
                    case 2:
                        foreach (string s in S.modus)
                        {
                            tempClus = cluster.Copy();
                            tempClus.modus = new List<string>() { s };
                            if (connected(tempClus))
                            {
                                return false;
                            }
                        }
                        break;
                }
            }

            return true;
        }

        KeyValuePair<int, string> selectP(Tricluster U, Tricluster V) // определяет оптимальный элемент для деления области значений
        {
            int optCat = -1;
            string optEl = "";
            double optValue = double.NegativeInfinity;
            double sum;
            double curValue;
            Tricluster tempClus;

            for (int d = 0; d < 3; d++)
            {
                switch (d)
                {
                    case 0:
                        if (V.extent.Count > 0)
                        {
                            sum = 0;
                            for (int k = 0; k < 3; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.extent.Count;
                                        }
                                        break;
                                    case 1:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.intent.Count;
                                        }
                                        break;
                                    case 2:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.modus.Count;
                                        }
                                        break;
                                }
                            }
                            if (sum > optValue)
                            {
                                optCat = d;
                                optValue = sum;
                            }
                        }
                        break;
                    case 1:
                        if (V.intent.Count > 0)
                        {
                            sum = 0;
                            for (int k = 0; k < 3; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.extent.Count;
                                        }
                                        break;
                                    case 1:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.intent.Count;
                                        }
                                        break;
                                    case 2:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.modus.Count;
                                        }
                                        break;
                                }
                            }
                            if (sum > optValue)
                            {
                                optCat = d;
                                optValue = sum;
                            }
                        }
                        break;
                    case 2:
                        if (V.modus.Count > 0)
                        {
                            sum = 0;
                            for (int k = 0; k < 3; k++)
                            {
                                switch (k)
                                {
                                    case 0:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.extent.Count;
                                        }
                                        break;
                                    case 1:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.intent.Count;
                                        }
                                        break;
                                    case 2:
                                        curValue = 1;
                                        if (k != d)
                                        {
                                            for (int l = 0; l < 3; l++)
                                            {
                                                switch (l)
                                                {
                                                    case 0:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.extent.Count;
                                                        }
                                                        break;
                                                    case 1:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.intent.Count;
                                                        }
                                                        break;
                                                    case 2:
                                                        if (l != d && l != k)
                                                        {
                                                            curValue *= U.modus.Count;
                                                        }
                                                        break;
                                                }
                                            }
                                            sum += curValue * V.modus.Count;
                                        }
                                        break;
                                }
                            }
                            if (sum > optValue)
                            {
                                optCat = d;
                                optValue = sum;
                            }
                        }
                        break;
                }
            }

            optValue = double.PositiveInfinity;
            tempClus = new Tricluster(context.Objects, context.Attributes, context.Conditions);
            tempClus = tempClus.Copy();
            switch(optCat)
            {
                case 0:
                    for (int i = 0; i < V.extent.Count; i++)
                    {
                        tempClus.extent = new List<string>() { V.extent[i] };
                        curValue = context.CalculateDensityAndCoverage(tempClus).Key;
                        if (curValue < optValue)
                        {
                            optEl = V.extent[i];
                            optValue = curValue;
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i < V.intent.Count; i++)
                    {
                        tempClus.intent = new List<string>() { V.intent[i] };
                        curValue = context.CalculateDensityAndCoverage(tempClus).Key;
                        if (curValue < optValue)
                        {
                            optEl = V.intent[i];
                            optValue = curValue;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < V.modus.Count; i++)
                    {
                        tempClus.modus = new List<string>() { V.modus[i] };
                        curValue = context.CalculateDensityAndCoverage(tempClus).Key;
                        if (curValue < optValue)
                        {
                            optEl = V.modus[i];
                            optValue = curValue;
                        }
                    }
                    break;
            }

            return new KeyValuePair<int, string>(optCat, optEl);
        }

        #endregion

        #region Parallel algorithms

        /*
        private List<Tricluster> Triclustering_OACBox_parallel_mc(double minDensity) // OAC-triclustering based on box-operators
        {
            ConcurrentBag<Tricluster> triclusterSet = new ConcurrentBag<Tricluster>();
            
            {
                KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>>> boxes = context.generalBoxesParallel();

                ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>> oSet = boxes.Key;
                ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>> aSet = boxes.Value.Key;
                ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>> cSet = boxes.Value.Value;



                //foreach (string o in context.Extent)
                Parallel.ForEach(context.Extent, o =>
                {
                    //foreach (string a in context.Intent)
                    Parallel.ForEach(context.Intent, a =>
                    {
                        foreach (string c in context.Modus)
                        //Parallel.ForEach(context.Modus, c =>
                        {
                            if (context.get(o, a, c))
                            {
                                KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o, new KeyValuePair<string, string>(a, c));
                                Tricluster temp = new Tricluster(oSet[cur].Keys.ToList<string>(), aSet[cur].Keys.ToList<string>(), cSet[cur].Keys.ToList<string>(), false);
                                if (minDensity > 0)
                                {
                                    KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp);
                                    if (denCov.Key >= minDensity)
                                    {
                                        temp.density = denCov.Key;
                                        temp.coverage = denCov.Value;
                                        triclusterSet.Add(temp);
                                    }
                                }
                                else
                                {
                                    triclusterSet.Add(temp);
                                }
                            }
                        }
                    });
                });
            }

            Parallel.ForEach(triclusterSet, t =>
            {
                t.extent.Sort();
                t.intent.Sort();
                t.modus.Sort();
                t.CalcIHash();
            });

            List<Tricluster> triclusters = triclusterSet.ToList<Tricluster>();

            Stack<int> todel = new Stack<int>();

            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusters[i] == triclusters[j])
                    {
                        todel.Push(i);
                        break;
                    }
                }
            }

            
            while (todel.Count != 0)
            {
                triclusters.RemoveAt(todel.Pop());
            }

            return triclusters;
        }
        */

        private List<Tricluster> Triclustering_OACBox_parallel(double minDensity) // OAC-triclustering based on box-operators
        {
            ConcurrentBag<Tricluster> triclusterSet = new ConcurrentBag<Tricluster>();
            
            KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>>> primes = context.generalPrimesParallel();

            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> oa = primes.Key;
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> oc = primes.Value.Key;
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> ac = primes.Value.Value;



            //foreach (string o in context.Extent)
            Parallel.ForEach(context.Objects, o =>
            {
                foreach (string a in context.Attributes)
                //Parallel.ForEach(context.Intent, a =>
                {
                    foreach (string c in context.Conditions)
                    //Parallel.ForEach(context.Modus, c =>
                    {
                        if (context.Contains(o, a, c))
                        {
                            KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o, new KeyValuePair<string, string>(a, c));
                            Tricluster temp = new Tricluster(context.boxO(oa[c].Key.Keys.ToList<string>(), oc[a].Key.Keys.ToList<string>()), context.boxA(oa[c].Value.Keys.ToList<string>(), ac[o].Key.Keys.ToList<string>()), context.boxC(oc[a].Value.Keys.ToList<string>(), ac[o].Value.Keys.ToList<string>()), false);
                            if (minDensity > 0)
                            {
                                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp);
                                if (denCov.Key >= minDensity)
                                {
                                    /*temp.extent.Sort();
                                    temp.intent.Sort();
                                    temp.modus.Sort();
                                    temp.CalcIHash();*/
                                    temp.density = denCov.Key;
                                    temp.coverage = denCov.Value;
                                    temp.coverageG = ((double)temp.extent.Count) / context.Objects.Count;
                                    temp.coverageM = ((double)temp.intent.Count) / context.Attributes.Count;
                                    temp.coverageB = ((double)temp.modus.Count) / context.Conditions.Count;
                                    /*bool flag = false;
                                    foreach (Tricluster t in triclusterSet)
                                    {
                                        if (temp == t)
                                        {
                                            flag = true;
                                            break;
                                        }
                                    }
                                    if (!flag)
                                    {
                                        triclusterSet.Add(temp);
                                    }*/
                                    triclusterSet.Add(temp);
                                }
                            }
                            else
                            {
                                //temp.CalcIHash();
                                /*temp.extent.Sort();
                                temp.intent.Sort();
                                temp.modus.Sort();
                                temp.CalcIHash();*/
                                /*bool flag = false;
                                foreach (Tricluster t in triclusterSet)
                                {
                                    if (temp == t)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    triclusterSet.Add(temp);
                                }*/
                                triclusterSet.Add(temp);
                            }
                        }
                    }
                }
            });


            Parallel.ForEach(triclusterSet, t =>
            {
                //t.extent.Sort();
                //t.intent.Sort();
                //t.modus.Sort();
                //t.CalcHash(md5Hash);
                t.CalcIHash();
            });

            List<Tricluster> triclusters = triclusterSet.ToList<Tricluster>();

            removeEqual(triclusters);

            return triclusters;
        }

        private List<Tricluster> Triclustering_OACPrime_parallel(double minDensity) // OAC-triclustering based on prime-operators
        {
            ConcurrentBag<Tricluster> triclusterSet = new ConcurrentBag<Tricluster>();

            KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>>> primes = context.generalPrimesPairParallel();

            ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>> oSet = primes.Value.Value;
            ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>> aSet = primes.Value.Key;
            ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>> cSet = primes.Key;

            //foreach (string o in context.Extent)
            Parallel.ForEach(context.Objects, o =>
            {
                foreach (string a in context.Attributes)
                {
                    foreach (string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            Tricluster temp = new Tricluster(oSet[new KeyValuePair<string, string>(a, c)].Keys.ToList<string>(), aSet[new KeyValuePair<string, string>(o, c)].Keys.ToList<string>(), cSet[new KeyValuePair<string, string>(o, a)].Keys.ToList<string>(), false);
                            if (minDensity > 0)
                            {
                                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp);
                                if (denCov.Key >= minDensity)
                                {
                                    //temp.CalcHash(md5Hash);
                                    temp.density = denCov.Key;
                                    temp.coverage = denCov.Value;
                                    temp.coverageG = ((double)temp.extent.Count) / context.Objects.Count;
                                    temp.coverageM = ((double)temp.intent.Count) / context.Attributes.Count;
                                    temp.coverageB = ((double)temp.modus.Count) / context.Conditions.Count;
                                    //temp.altDensity = (double)(temp.extent.Count + temp.intent.Count + temp.modus.Count - 2) / (temp.extent.Count * temp.intent.Count * temp.modus.Count);
                                    double sum = temp.extent.Count + temp.intent.Count + temp.modus.Count - 2, prod = temp.extent.Count * temp.intent.Count * temp.modus.Count;
                                    temp.altDensity1 = (double)(sum) / (prod);
                                    temp.altDensity2 = (double)(sum + (prod - sum) * (context.Count - sum) / (context.Volume)) / (prod);
                                    temp.altDensity3 = (double)(sum + (prod - sum) * (context.Count) / (context.Volume)) / (prod);
                                    triclusterSet.Add(temp);
                                }
                            }
                            else
                            {
                                triclusterSet.Add(temp);
                            }
                        }
                    }
                }
            });

            Parallel.ForEach(triclusterSet, t =>
            {
                t.extent.Sort();
                t.intent.Sort();
                t.modus.Sort();
                //t.CalcHash(md5Hash);
                t.CalcIHash();
            });

            List<Tricluster> triclusters = triclusterSet.ToList<Tricluster>();

            removeEqual(triclusters);

            return triclusters;
        }

        /*
        private List<Tricluster> Triclustering_Spectral_parallel(double minSize) // Spectral triclustering
        {
            List<Tricluster> triclusterSet = new List<Tricluster>();

            List<string> extent = context.Extent, intent = context.Intent, modus = context.Modus;
            int dim = extent.Count + intent.Count + modus.Count;
            double[,] matrix = new double[dim, dim];
            int[] indicator = new int[dim];
            Dictionary<int, int> countTri = new Dictionary<int, int>();
            ConcurrentDictionary<int, double[,]> matrices = new ConcurrentDictionary<int, double[,]>();
            int curInd = 0;

            for (int i = 0; i < dim; i++)
            {
                countTri.Add(i, 0);
            }
            countTri[curInd] = dim;

            // creation of the matrix of the triadic context

            //for (int i = 0; i < dim; i++)
            Parallel.For(0, dim, i =>
            {
                //for (int j = 0; j < dim; j++)
                Parallel.For(0, dim, j =>
                {
                    matrix[i, j] = 0;
                });
            });

            //for (int i = 0; i < extent.Count; i++)
            Parallel.For(0, extent.Count, i =>
            {
                //for (int j = 0; j < intent.Count; j++)
                Parallel.For(0, intent.Count, j =>
                {
                    //for (int k = 0; k < modus.Count; k++)
                    Parallel.For(0, modus.Count, k =>
                    {
                        if (context.get(extent[i], intent[j], modus[k]))
                        {
                            matrix[i, extent.Count + j] = -1;
                            matrix[i, extent.Count + intent.Count + k] = -1;
                            matrix[extent.Count + j, extent.Count + intent.Count + k] = -1;

                            matrix[extent.Count + j, i] = -1;
                            matrix[extent.Count + intent.Count + k, i] = -1;
                            matrix[extent.Count + intent.Count + k, extent.Count + j] = -1;
                        }
                    });
                });
            });

            double count;

            //for (int i = 0; i < dim; i++)
            Parallel.For(0, dim, i =>
            {
                count = 0;
                //for (int j = 0; j < dim; j++)
                Parallel.For(0, dim, j =>
                {
                    count -= matrix[i, j];
                });
                matrix[i, i] = count;
            });

            bool flag = true;

            while ((((double)(countTri.Values.Max()) / dim) > minSize) && flag) // iteratively running eigenvalues decomposition on all submatrices
            {
                flag = false;
                matrices = new ConcurrentDictionary<int, double[,]>();
                //foreach (int i in countTri.Keys)
                Parallel.For(0, countTri.Keys.Count, i =>
                {
                    if (countTri[i] > 0)
                    {
                        double[,] tempM = new double[countTri[i], countTri[i]];
                        int countRows = 0, countColumns;
                        for (int j = 0; j < dim; j++)
                        {
                            countColumns = 0;
                            if (indicator[j] == i)
                            {
                                for (int k = 0; k < dim; k++)
                                {
                                    if (indicator[k] == i)
                                    {
                                        tempM[countRows, countColumns] = matrix[j, k];
                                        tempM[countColumns, countRows] = matrix[k, j];
                                        countColumns++;
                                    }
                                }
                                countRows++;
                            }
                        }

                        //for (int j = 0; j < countTri[i]; j++)
                        Parallel.For(0, countTri[i], j =>
                        {
                            count = 0;
                            //for (int k = 0; k < countTri[i]; k++)
                            Parallel.For(0, countTri[i], k =>
                            {
                                if (k != j)
                                {
                                    count -= tempM[j, k];
                                }
                            });
                            tempM[j, j] = count;
                        });
                        matrices.TryAdd(i, tempM);
                    }
                });

                //for (int i = 0; i < matrices.Count; i++)
                Parallel.For(0, matrices.Count, i =>
                {
                    double[] tempVal;
                    double[,] tempVec;
                    //int[] tempInd = new int[countTri[i]];
                    int ind;

                    alglib.smatrixevd(matrices[i], countTri[i], 1, true, out tempVal, out tempVec);

                    int cpos = 0, cneg = 0;

                    //for (int j = 0; j < countTri[i]; j++)
                    Parallel.For(0, countTri[i], j =>
                    {
                        if (tempVec[j, 1] >= 0)
                        {
                            cpos++;
                        }
                        else
                        {
                            cneg++;
                        }
                    });

                    int[] tempIndicator = new int[dim];

                    //for (int j = 0; j < dim; j++)
                    Parallel.For(0, dim, j =>
                    {
                        tempIndicator[j] = indicator[j];
                    });

                    if (cpos >= 3 && cneg >= 3)
                    {
                        ind = 0;

                        curInd++;

                        //for (int j = 0; j < extent.Count; j++)
                        Parallel.For(0, extent.Count, j =>
                        {
                            if (tempIndicator[j] == i)
                            {
                                if (tempVec[ind, 1] >= 0)
                                {
                                    tempIndicator[j] = curInd;
                                }

                                ind++;
                            }
                        });
                        //for (int j = extent.Count; j < extent.Count + intent.Count; j++)
                        Parallel.For(extent.Count, extent.Count + intent.Count, j =>
                        {
                            if (tempIndicator[j] == i)
                            {
                                if (tempVec[ind, 1] >= 0)
                                {
                                    tempIndicator[j] = curInd;
                                }

                                ind++;
                            }
                        });
                        //for (int j = extent.Count + intent.Count; j < dim; j++)
                        Parallel.For(extent.Count + intent.Count, dim, j =>
                        {
                            if (tempIndicator[j] == i)
                            {
                                if (tempVec[ind, 1] >= 0)
                                {
                                    tempIndicator[j] = curInd;
                                }

                                ind++;
                            }
                        });

                        ConcurrentDictionary<int, bool> indicators = new ConcurrentDictionary<int, bool>();
                        bool tempFlag = true;
                        //for (int j = 0; j <= tempIndicator.Max(); j++)
                        Parallel.For(0, tempIndicator.Max(), j =>
                        {
                            indicators.TryAdd(j, false);
                        });
                        //for (int j = 0; j < extent.Count; j++)
                        Parallel.For(0, extent.Count, j =>
                        {
                            indicators[tempIndicator[j]] = true;
                        });
                        //for (int j = 0; j < tempIndicator.Max(); j++)
                        Parallel.For(0, tempIndicator.Max(), j =>
                        {
                            tempFlag = tempFlag && indicators[j];
                        });
                        if (!tempFlag)
                        {
                            curInd--;
                        }
                        else
                        {

                            indicators = new ConcurrentDictionary<int,bool>();
                            tempFlag = true;
                            //for (int j = 0; j <= tempIndicator.Max(); j++)
                            Parallel.For(0, tempIndicator.Max(), j =>
                            {
                                indicators.TryAdd(j, false);
                            });
                            //for (int j = extent.Count; j < extent.Count + intent.Count; j++)
                            Parallel.For(extent.Count, extent.Count + intent.Count, j =>
                            {
                                indicators[tempIndicator[j]] = true;
                            });
                            //for (int j = 0; j < tempIndicator.Max(); j++)
                            Parallel.For(0, tempIndicator.Max(), j =>
                            {
                                tempFlag = tempFlag && indicators[j];
                            });
                            if (!tempFlag)
                            {
                                curInd--;
                            }
                            else
                            {

                                indicators = new ConcurrentDictionary<int,bool>();
                                tempFlag = true;
                                //for (int j = 0; j <= tempIndicator.Max(); j++)
                                Parallel.For(0, tempIndicator.Max(), j =>
                                {
                                    indicators.TryAdd(j, false);
                                });
                                //for (int j = extent.Count + intent.Count; j < dim; j++)
                                Parallel.For(extent.Count + intent.Count, dim, j =>
                                {
                                    indicators[tempIndicator[j]] = true;
                                });
                                //for (int j = 0; j < tempIndicator.Max(); j++)
                                Parallel.For(0, tempIndicator.Max(), j =>
                                {
                                    tempFlag = tempFlag && indicators[j];
                                });
                                if (!tempFlag)
                                {
                                    curInd--;
                                }
                                else
                                {

                                    flag = true;
                                    indicator = tempIndicator;
                                }
                            }
                        }
                    }
                });

                //for (int i = 0; i < dim; i++)
                Parallel.For(0, dim, i =>
                {
                    countTri[i] = 0;
                });

                //for (int i = 0; i < dim; i++)
                Parallel.For(0, dim, i =>
                {
                    countTri[indicator[i]]++;
                });
            }

            // transforming matrix into the set of triclusters

            for (int i = 0; i <= countTri.Keys.Max(); i++)
            {
                if (countTri[i] == 0)
                {
                    break;
                }
                triclusterSet.Add(new Tricluster());
            }

            //for (int i = 0; i < extent.Count; i++)
            Parallel.For(0, extent.Count, i =>
            {
                triclusterSet[indicator[i]].extent.Add(extent[i]);
            });
            //for (int i = extent.Count; i < extent.Count + intent.Count; i++)
            Parallel.For(extent.Count, extent.Count + intent.Count, i =>
            {
                triclusterSet[indicator[i]].intent.Add(intent[i - extent.Count]);
            });
            //for (int i = extent.Count + intent.Count; i < dim; i++)
            Parallel.For(extent.Count + intent.Count, dim, i =>
            {
                triclusterSet[indicator[i]].modus.Add(modus[i - extent.Count - intent.Count]);
            });

            //for (int i = 0; i < triclusterSet.Count; i++)
            Parallel.For(0, triclusterSet.Count, i =>
            {
                KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(triclusterSet[i]);
                triclusterSet[i].density = denCov.Key;
                triclusterSet[i].coverage = denCov.Value;
            });

            return triclusterSet;
        }
        */

        private List<Tricluster> Triclustering_TBox_parallel(double options) // TBox
        {
            ConcurrentBag<Tricluster> triclusterSet = new ConcurrentBag<Tricluster>();

            Parallel.ForEach(context.Objects, o =>
            {
                
                foreach (string a in context.Attributes)
                {
                    foreach (string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            Tricluster curTri = new Tricluster();
                            curTri.extent.Add(o);
                            curTri.intent.Add(a);
                            curTri.modus.Add(c);
                            Tricluster temp = TBoxBody_parallel(curTri);
                            if (temp.extent.Count > 0 && temp.intent.Count > 0 && temp.modus.Count > 0)
                            {
                                triclusterSet.Add(temp);
                            }
                        }
                    }
                }
            });

            List<Tricluster> triclusters = triclusterSet.ToList<Tricluster>();
            foreach (Tricluster t in triclusters)
            {
                t.CalcHash(md5Hash);
            }

            removeEqual(triclusters);

            return triclusters;
        }

        /*
        private double CalcD_parallel(Tricluster tri, string star, char set)
        {
            double sum;
            double z = -1;
            double n = 0, m = 0, l = 0;
            List<string> starList = new List<string>();
            starList.Add(star);
            Tricluster temp = new Tricluster();
            starList.Add(star);
            switch (set)
            {
                case 'e':
                    if (tri.extent.Count == 1 && star == tri.extent[0])
                    {
                        return double.MinValue;
                    }
                    temp.extent = starList;
                    temp.intent = tri.intent;
                    temp.modus = tri.modus;
                    if (!tri.extent.Contains(star))
                    {
                        z = 1;
                    }
                    n = tri.extent.Count;
                    m = tri.intent.Count;
                    l = tri.modus.Count;
                    break;
                case 'i':
                    if (tri.intent.Count == 1 && star == tri.intent[0])
                    {
                        return double.MinValue;
                    }
                    temp.extent = tri.extent;
                    temp.intent = starList;
                    temp.modus = tri.modus;
                    if (!tri.intent.Contains(star))
                    {
                        z = 1;
                    }
                    n = tri.intent.Count;
                    m = tri.extent.Count;
                    l = tri.modus.Count;
                    break;
                case 'm':
                    if (tri.modus.Count == 1 && star == tri.modus[0])
                    {
                        return double.MinValue;
                    }
                    temp.extent = tri.extent;
                    temp.intent = tri.intent;
                    temp.modus = starList;
                    if (!tri.modus.Contains(star))
                    {
                        z = 1;
                    }
                    n = tri.modus.Count;
                    m = tri.extent.Count;
                    l = tri.intent.Count;
                    break;
                default:
                    return double.MinValue;
            }

            double r = 0, rStar = 0;

            //foreach (string o in tri.extent)
            Parallel.ForEach(tri.extent, o =>
            {
                //foreach (string a in tri.intent)
                Parallel.ForEach(tri.intent, a =>
                {
                    //foreach (string c in tri.modus)
                    Parallel.ForEach(tri.modus, c =>
                    {
                        r += Convert.ToInt32(context.get(o, a, c));
                    });
                });
            });

            //foreach (string o in temp.extent)
            Parallel.ForEach(temp.extent, o =>
            {
                //foreach (string a in temp.intent)
                Parallel.ForEach(temp.intent, a =>
                {
                    //foreach (string c in temp.modus)
                    Parallel.ForEach(temp.modus, c =>
                    {
                        rStar += Convert.ToInt32(context.get(o, a, c));
                    });
                });
            });

            sum = (rStar * rStar + 2 * z * r * rStar - z * r * r / n) / ((n + z) * m * l);

            return sum;
        }
        */

        private Tricluster TBoxBody_parallel(Tricluster curTri)
        {
            double dStar;
            double D;
            KeyValuePair<string, char> best = new KeyValuePair<string, char>();

            while (true)
            {
                dStar = double.MinValue;
                for (int j = 0; j < context.Objects.Count; j++)
                {
                    D = CalcD_norm(curTri, context.Objects[j], 'e');
                    if (D > dStar)
                    {
                        dStar = D;
                        best = new KeyValuePair<string, char>(context.Objects[j], 'e');
                    }
                }

                for (int j = 0; j < context.Attributes.Count; j++)
                {
                    D = CalcD_norm(curTri, context.Attributes[j], 'i');
                    if (D > dStar)
                    {
                        dStar = D;
                        best = new KeyValuePair<string, char>(context.Attributes[j], 'i');
                    }
                }

                for (int j = 0; j < context.Conditions.Count; j++)
                {
                    D = CalcD_norm(curTri, context.Conditions[j], 'm');
                    if (D > dStar)
                    {
                        dStar = D;
                        best = new KeyValuePair<string, char>(context.Conditions[j], 'm');
                    }
                }

                if (dStar < 0)
                {
                    KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(curTri);
                    curTri.density = denCov.Key;
                    curTri.coverage = denCov.Value;
                    curTri.extent.Sort();
                    curTri.intent.Sort();
                    curTri.modus.Sort();
                    //curTri.CalcHash(md5Hash);
                    return curTri;
                }
                else
                {
                    switch (best.Value)
                    {
                        case 'e':
                            if (curTri.extent.Contains(best.Key))
                            {
                                curTri.extent.Remove(best.Key);
                            }
                            else
                            {
                                curTri.extent.Add(best.Key);
                            }
                            break;
                        case 'i':
                            if (curTri.intent.Contains(best.Key))
                            {
                                curTri.intent.Remove(best.Key);
                            }
                            else
                            {
                                curTri.intent.Add(best.Key);
                            }
                            break;
                        case 'm':
                            if (curTri.modus.Contains(best.Key))
                            {
                                curTri.modus.Remove(best.Key);
                            }
                            else
                            {
                                curTri.modus.Add(best.Key);
                            }
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        /*
        private List<Tricluster> Triclustering_Trias_parallel(double tauG, double tauM, double tauB) // Trias
        {
            ConcurrentBag<Tricluster> triclusterSet = new ConcurrentBag<Tricluster>();

            //List<KeyValuePair<string, KeyValuePair<string, string>>> dContext = new List<KeyValuePair<string,KeyValuePair<string,string>>>();
            HashSet<KeyValuePair<string, string>> intent = new HashSet<KeyValuePair<string, string>>();
            HashSet<string> extent = new HashSet<string>();
            Dictionary<string, List<KeyValuePair<string, string>>> utrCont = new Dictionary<string, List<KeyValuePair<string, string>>>();

            foreach (string o in context.Objects)
            {
                utrCont.Add(o, new List<KeyValuePair<string, string>>());
            }

            foreach (string o in context.Objects)
            {
                foreach (string a in context.Attributes)
                {
                    foreach (string c in context.Conditions)
                    {
                        if (context.Contains(o, a, c))
                        {
                            extent.Add(o);
                            intent.Add(new KeyValuePair<string, string>(a, c));
                            if (!utrCont[o].Contains(new KeyValuePair<string, string>(a, c)))
                            {
                                utrCont[o].Add(new KeyValuePair<string, string>(a, c));
                            }
                        }
                    }
                }
            }

            DyadicContext<string, KeyValuePair<string, string>> UTRContext = new DyadicContext<string, KeyValuePair<string, string>>(utrCont, extent.ToList<string>(), intent.ToList<KeyValuePair<string, string>>());

            KeyValuePair<List<string>, List<KeyValuePair<string, string>>> AI = FirstFrequentConcept(UTRContext, tauG);
            KeyValuePair<List<string>, List<string>> BC;

            do
            {
                if (AI.Value.Count >= tauM * tauB)
                {
                    DyadicContext<string, string> TRContext = new DyadicContext<string, string>(AI.Value);
                    BC = FirstFrequentConcept(TRContext, tauM);
                    do
                    {
                        if (BC.Value.Count >= tauB)
                        {
                            List<KeyValuePair<string, string>> cartProd = new List<KeyValuePair<string, string>>();
                            foreach (string b in BC.Key)
                            {
                                foreach (string c in BC.Value)
                                {
                                    cartProd.Add(new KeyValuePair<string, string>(b, c));
                                }
                            }
                            List<string> BCp = UTRContext.primeA(cartProd);
                            bool flag = (AI.Key.Count == BCp.Count);
                            if (flag)
                            {
                                foreach (string str in AI.Key)
                                {
                                    if (!BCp.Contains(str))
                                    {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                            if (flag)
                            {
                                Tricluster temp = new Tricluster(); //new Tricluster(AI.Key, BC.Key, BC.Value, false);
                                foreach (string s in AI.Key)
                                {
                                    temp.extent.Add(s);
                                }
                                foreach (string s in BC.Key)
                                {
                                    temp.intent.Add(s);
                                }
                                foreach (string s in BC.Value)
                                {
                                    temp.modus.Add(s);
                                }
                                //KeyValuePair<double, double> denCov = context.CalculateDensityAndCoverage(temp);
                                //temp.density = denCov.Key;
                                //temp.coverage = denCov.Value;
                                temp.CalcIHash();
                                triclusterSet.Add(temp);
                            }
                        }
                    }
                    while (NextFrequentConcept(TRContext, tauM, BC, out BC));
                }
            }
            while (NextFrequentConcept(UTRContext, tauG, AI, out AI));

            return triclusterSet.ToList<Tricluster>();
        }

        private KeyValuePair<List<string>, List<KeyValuePair<string, string>>> FirstFrequentConcept_parallel(DyadicContext<string, KeyValuePair<string, string>> cont, double tau)
        {
            List<KeyValuePair<string, string>> B = cont.primeO(new List<string>());
            List<string> A = cont.primeA(B);

            KeyValuePair<List<string>, List<KeyValuePair<string, string>>> AB = new KeyValuePair<List<string>, List<KeyValuePair<string, string>>>(A, B);

            if (A.Count < tau)
            {
                NextFrequentConcept(cont, tau, AB, out AB);
            }

            return AB;
        }

        private KeyValuePair<List<string>, List<string>> FirstFrequentConcept_parallel(DyadicContext<string, string> cont, double tau)
        {
            List<string> B = cont.primeO(new List<string>());
            List<string> A = cont.primeA(B);

            KeyValuePair<List<string>, List<string>> AB = new KeyValuePair<List<string>, List<string>>(A, B);

            if (A.Count < tau)
            {
                NextFrequentConcept(cont, tau, AB, out AB);
            }

            return AB;
        }

        private bool NextFrequentConcept_parallel(DyadicContext<string, KeyValuePair<string, string>> cont, double tau, KeyValuePair<List<string>, List<KeyValuePair<string, string>>> AB, out KeyValuePair<List<string>, List<KeyValuePair<string, string>>> outAB)
        {
            int i = cont.Objects.Count - 1;
            string g;
            List<string> G = new List<string>();
            //Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (string s in cont.Objects)
            {
                G.Add(s);
                //dict.Add(s, cont.CountO(s));
            }
            //List<KeyValuePair<string, int>> tempList = (dict.OrderBy(p => p.Value)).ToList<KeyValuePair<string, int>>();//.ToDictionary<string, int>(p => p.Key);
            //List<string> map = cont.Sort();
            //foreach (KeyValuePair<string, int> p in tempList)
            //{
            //    map.Add(p.Key);
            //}

            outAB = AB;

            List<string> tempA = new List<string>(); //AB.Key;
            List<KeyValuePair<string, string>> tempB = new List<KeyValuePair<string, string>>();
            foreach (string s in AB.Key)
            {
                tempA.Add(s);
            }

            while (i >= 0)
            {
                //outAB = AB;
                g = cont.Objects[i];
                G.Remove(g);
                if (!tempA.Contains(g))
                {
                    tempA = (tempA.Intersect(G)).ToList<string>();
                    tempA.Add(g);
                    //outAB = new KeyValuePair<List<string>,List<KeyValuePair<string,string>>>(outAB.Key, cont.primeO(tempA));
                    tempB = cont.primeO(tempA);
                    List<string> D = cont.primeA(tempB);
                    //if (D.Count >= tau)
                    {
                        List<string> DwA = (D.Except(tempA)).ToList<string>();
                        if ((DwA.Intersect(G)).Count() == 0)
                        {
                            outAB = new KeyValuePair<List<string>, List<KeyValuePair<string, string>>>(D, tempB);
                            if (D.Count < tau)
                            {
                                return NextFrequentConcept(cont, tau, outAB, out outAB);
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
                i--;
                //G.Add(g);
            }
            return false;
        }

        private bool NextFrequentConcept_parallel(DyadicContext<string, string> cont, double tau, KeyValuePair<List<string>, List<string>> AB, out KeyValuePair<List<string>, List<string>> outAB)
        {
            int i = cont.Objects.Count - 1;
            string g;
            List<string> G = new List<string>();
            //Dictionary<string, int> dict = new Dictionary<string, int>();
            foreach (string s in cont.Objects)
            {
                G.Add(s);
                //dict.Add(s, cont.CountO(s));
            }
            //List<KeyValuePair<string, int>> tempList = (dict.OrderBy(p => p.Value)).ToList<KeyValuePair<string, int>>();//.ToDictionary<string, int>(p => p.Key);
            //List<string> map = cont.Sort();
            //foreach (KeyValuePair<string, int> p in tempList)
            //{
            //    map.Add(p.Key);
            //}
            outAB = AB;

            List<string> tempA = new List<string>(); //AB.Key;
            List<string> tempB = new List<string>();
            foreach (string s in AB.Key)
            {
                tempA.Add(s);
            }

            while (i >= 0)
            {
                //outAB = AB;
                g = cont.Objects[i];
                G.Remove(g);
                if (!tempA.Contains(g))
                {
                    tempA = (tempA.Intersect(G)).ToList<string>();
                    tempA.Add(g);
                    //outAB = new KeyValuePair<List<string>,List<string>>(outAB.Key, cont.primeO(tempA));
                    tempB = cont.primeO(tempA);
                    List<string> D = cont.primeA(tempB);
                    //if (D.Count >= tau)
                    {
                        List<string> DwA = (D.Except(tempA)).ToList<string>();
                        if ((DwA.Intersect(G)).Count() == 0)
                        {
                            outAB = new KeyValuePair<List<string>, List<string>>(D, tempB);
                            if (D.Count < tau)
                            {
                                return NextFrequentConcept(cont, tau, outAB, out outAB);
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }

                }
                i--;
                //G.Add(g);
            }
            return false;
        }
        */

        #endregion


        private void removeEqual(List<Tricluster> triclusterSet) // удаляет повторы трикластеров из множества
        {
            Stack<int> todel = new Stack<int>();

            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusterSet[i] == triclusterSet[j])
                    {
                        todel.Push(i);
                        break;
                    }
                }
            }
            while (todel.Count != 0)
            {
                triclusterSet.RemoveAt(todel.Pop());
            }
        }

        private void removeSimilar(List<Tricluster> triclusterSet, double similarity) // удаляет похожие трикластеры // not finished
        {
            Stack<int> todel = new Stack<int>();

            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusterSet[i] == triclusterSet[j])
                    {
                        todel.Push(i);
                        break;
                    }
                }
            }
            while (todel.Count != 0)
            {
                triclusterSet.RemoveAt(todel.Pop());
            }
        }

        public void removeInjections(List<Tricluster> triclusterSet, double diff) // удаляет вкладывающийся трикластер (если у большего трикластера плотность выше - автоматически, если меньше, если разница плотности достаточно мала)
        {
            List<int> todel = new List<int>();

            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusterSet[i].extent.Count == triclusterSet[j].extent.Count && triclusterSet[i].intent.Count == triclusterSet[j].intent.Count)
                    {
                        bool flag = true;
                        for (int k = 0; k < triclusterSet[i].extent.Count; k++)
                        {
                            if (triclusterSet[i].extent[k] != triclusterSet[j].extent[k])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            for (int k = 0; k < triclusterSet[i].intent.Count; k++)
                            {
                                if (triclusterSet[i].intent[k] != triclusterSet[j].intent[k])
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            if (triclusterSet[i].modus.Except(triclusterSet[j].modus).Count() == 0)
                            {
                                if ((triclusterSet[i].density - triclusterSet[j].density) <= diff)
                                {
                                    todel.Add(i);
                                }
                            }
                            else if (triclusterSet[j].modus.Except(triclusterSet[i].modus).Count() == 0)
                            {
                                if ((triclusterSet[j].density - triclusterSet[i].density) <= diff)
                                {
                                    todel.Add(j);
                                }
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusterSet[i].extent.Count == triclusterSet[j].extent.Count && triclusterSet[i].modus.Count == triclusterSet[j].modus.Count)
                    {
                        bool flag = true;
                        for (int k = 0; k < triclusterSet[i].extent.Count; k++)
                        {
                            if (triclusterSet[i].extent[k] != triclusterSet[j].extent[k])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            for (int k = 0; k < triclusterSet[i].modus.Count; k++)
                            {
                                if (triclusterSet[i].modus[k] != triclusterSet[j].modus[k])
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            if (triclusterSet[i].intent.Except(triclusterSet[j].intent).Count() == 0)
                            {
                                if ((triclusterSet[i].density - triclusterSet[j].density) <= diff)
                                {
                                    todel.Add(i);
                                }
                            }
                            else if (triclusterSet[j].intent.Except(triclusterSet[i].intent).Count() == 0)
                            {
                                if ((triclusterSet[j].density - triclusterSet[i].density) <= diff)
                                {
                                    todel.Add(j);
                                }
                            }
                        }
                    }
                }
            }
            
            for (int i = 0; i < triclusterSet.Count; i++)
            {
                for (int j = triclusterSet.Count - 1; j > i; j--)
                {
                    if (triclusterSet[i].intent.Count == triclusterSet[j].intent.Count && triclusterSet[i].modus.Count == triclusterSet[j].modus.Count)
                    {
                        bool flag = true;
                        for (int k = 0; k < triclusterSet[i].intent.Count; k++)
                        {
                            if (triclusterSet[i].intent[k] != triclusterSet[j].intent[k])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            for (int k = 0; k < triclusterSet[i].modus.Count; k++)
                            {
                                if (triclusterSet[i].modus[k] != triclusterSet[j].modus[k])
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        if (flag)
                        {
                            if (triclusterSet[i].extent.Except(triclusterSet[j].extent).Count() == 0)
                            {
                                if ((triclusterSet[i].density - triclusterSet[j].density) <= diff)
                                {
                                    todel.Add(i);
                                }
                            }
                            else if (triclusterSet[j].extent.Except(triclusterSet[i].extent).Count() == 0)
                            {
                                if ((triclusterSet[j].density - triclusterSet[i].density) <= diff)
                                {
                                    todel.Add(j);
                                }
                            }
                        }
                    }
                }
            }
            todel = todel.Distinct().ToList<int>();
            todel.Sort();
            for (int i = todel.Count - 1; i >= 0; i--)
            {
                triclusterSet.RemoveAt(todel[i]);
            }
        }
    }
}
