using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ultimate_Triclustering
{
    public class TriadicContext
    {
        private Dictionary<string, Dictionary<string, List<string>>> context; // тройки контекста
        private List<string> objects, attributes, conditions;
        private int count; // число троек контекста
        private int volume; // объём контекста (|G|*|M|*|B|)
        private Dictionary<string, string> eNames, iNames, mNames; // словарь из номера объекта / признака / условия в его имя

        public TriadicContext()
        {
            context = new Dictionary<string, Dictionary<string, List<string>>>();
        }

        public TriadicContext(Dictionary<string, Dictionary<string, List<string>>> context)
        {
            this.context = context;
            // some method to get extent, intent, and modus from context...
        }

        public TriadicContext(Dictionary<string, Dictionary<string, List<string>>> context, List<string> extent, List<string> intent, List<string> modus)
        {
            this.context = context;
            this.objects = extent;
            this.attributes = intent;
            this.conditions = modus;
        }

        public TriadicContext(Dictionary<string, Dictionary<string, List<string>>> context, List<string> extent, List<string> intent, List<string> modus, Dictionary<string, string> eNames, Dictionary<string, string> iNames, Dictionary<string, string> mNames)
        {
            this.context = context;
            this.objects = extent;
            this.attributes = intent;
            this.conditions = modus;
            this.eNames = eNames;
            this.iNames = iNames;
            this.mNames = mNames;
        }

        public bool Contains(string o, string a, string c) // истина, если тройка принадлежит контексту, иначе ложь
        {
            if (context.Keys.Contains(o) && context[o].Keys.Contains(a) && context[o][a].Contains(c))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<string> Objects
        {
            get { return objects; }
            set { }
        }

        public List<string> Attributes
        {
            get { return attributes; }
            set { }
        }

        public List<string> Conditions
        {
            get { return conditions; }
            set { }
        }

        public int Count
        {
            get { return count; }
            set { count = value; }
        }

        public int Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        #region Prime operators

        public List<KeyValuePair<string, string>> primeO(string o) // штрих оператор от одного объекта (o)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            if (context.Keys.Contains(o))
            {
                foreach (KeyValuePair<string, List<string>> a in context[o])
                {
                    foreach (string c in a.Value)
                    {
                        list.Add(new KeyValuePair<string,string>(a.Key, c));
                    }
                }
            }
            return list;
        }

        public List<KeyValuePair<string, string>> primeA(string a)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                if (o.Value.Keys.Contains(a))
                {
                    foreach (string c in o.Value[a])
                    {
                        list.Add(new KeyValuePair<string, string>(o.Key, c)); 
                    }
                }
            }
            return list;
        }

        public List<KeyValuePair<string, string>> primeC(string c)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                {
                    if(a.Value.Contains(c))
                    {
                        list.Add(new KeyValuePair<string, string>(o.Key, a.Key));
                    }
                }
            }
            return list;
        }

        public KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>>> generalPrimes() // множества штрихов для каждого объекта / признака / условия
        {
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oa = new Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>(conditions.Count);
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oc = new Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>(attributes.Count);
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> ac = new Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>(objects.Count);

            foreach (string o in objects)
            {
                ac.Add(o, new KeyValuePair<SortedSet<string>, SortedSet<string>>(new SortedSet<string>(), new SortedSet<string>()));
            }
            foreach (string a in attributes)
            {
                oc.Add(a, new KeyValuePair<SortedSet<string>, SortedSet<string>>(new SortedSet<string>(), new SortedSet<string>()));
            }
            foreach (string c in conditions)
            {
                oa.Add(c, new KeyValuePair<SortedSet<string>, SortedSet<string>>(new SortedSet<string>(), new SortedSet<string>()));
            }

            foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                {
                    foreach (string c in a.Value)
                    {
                        oa[c].Key.Add(o.Key);
                        oa[c].Value.Add(a.Key);
                        oc[a.Key].Key.Add(o.Key);
                        oc[a.Key].Value.Add(c);
                        ac[o.Key].Key.Add(a.Key);
                        ac[o.Key].Value.Add(c);
                    }
                }
            }

            return new KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>>>(oa, new KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>>(oc, ac));
        }

        public KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>>> generalPrimesParallel() // множества штрихов для каждого объекта / признака / условия с распараллеливанием внешних циклов
        {
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> oa = new ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>();
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> oc = new ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>();
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> ac = new ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>();

            //foreach (string o in extent)
            Parallel.ForEach(objects, o =>
            {
                ac.TryAdd(o, new KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>(new ConcurrentDictionary<string, bool>(), new ConcurrentDictionary<string, bool>()));
            });
            //foreach (string a in intent)
            Parallel.ForEach(attributes, a =>
            {
                oc.TryAdd(a, new KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>(new ConcurrentDictionary<string, bool>(), new ConcurrentDictionary<string, bool>()));
            });
            //foreach (string c in modus)
            Parallel.ForEach(conditions, c =>
            {
                oa.TryAdd(c, new KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>(new ConcurrentDictionary<string, bool>(), new ConcurrentDictionary<string, bool>()));
            });

            //foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            Parallel.ForEach(context, o =>
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                //Parallel.ForEach(o.Value, a =>
                {
                    foreach (string c in a.Value)
                    //Parallel.ForEach(a.Value, c =>
                    {
                        oa[c].Key.TryAdd(o.Key, true);
                        oa[c].Value.TryAdd(a.Key, true);
                        oc[a.Key].Key.TryAdd(o.Key, true);
                        oc[a.Key].Value.TryAdd(c, true);
                        ac[o.Key].Key.TryAdd(a.Key, true);
                        ac[o.Key].Value.TryAdd(c, true);
                    }
                }
            });

            return new KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>>>(oa, new KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>>(oc, ac));
        }

        public KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, Dictionary<KeyValuePair<string, string>, SortedSet<string>>>> generalPrimesPair() // множества штрихов от всех пар (объект, признак) / (объект, условие) / (признак, условие)
        {
            Dictionary<KeyValuePair<string, string>, SortedSet<string>> oa = new Dictionary<KeyValuePair<string, string>, SortedSet<string>>();
            Dictionary<KeyValuePair<string, string>, SortedSet<string>> oc = new Dictionary<KeyValuePair<string, string>, SortedSet<string>>();
            Dictionary<KeyValuePair<string, string>, SortedSet<string>> ac = new Dictionary<KeyValuePair<string, string>, SortedSet<string>>();

            foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                {
                    foreach (string c in a.Value)
                    {
                        if (!oa.Keys.Contains(new KeyValuePair<string, string>(o.Key, a.Key)))
                        {
                            oa.Add(new KeyValuePair<string, string>(o.Key, a.Key), new SortedSet<string>());
                        }
                        if (!oc.Keys.Contains(new KeyValuePair<string, string>(o.Key, c)))
                        {
                            oc.Add(new KeyValuePair<string, string>(o.Key, c), new SortedSet<string>());
                        }
                        if (!ac.Keys.Contains(new KeyValuePair<string, string>(a.Key, c)))
                        {
                            ac.Add(new KeyValuePair<string, string>(a.Key, c), new SortedSet<string>());
                        }

                        oa[new KeyValuePair<string, string>(o.Key, a.Key)].Add(c);
                        oc[new KeyValuePair<string, string>(o.Key, c)].Add(a.Key);
                        ac[new KeyValuePair<string, string>(a.Key, c)].Add(o.Key);
                    }
                }
            }

            return new KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, Dictionary<KeyValuePair<string, string>, SortedSet<string>>>>(oa, new KeyValuePair<Dictionary<KeyValuePair<string, string>, SortedSet<string>>, Dictionary<KeyValuePair<string, string>, SortedSet<string>>>(oc, ac));
        }

        public KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>>> generalPrimesPairParallel() // множества штрихов от всех пар (объект, признак) / (объект, условие) / (признак, условие);  (выигрывает у непараллельной версии только на очень больших контекстах) 
        {
            ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>> oa = new ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>();
            ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>> oc = new ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>();
            ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>> ac = new ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>();

            //foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            Parallel.ForEach(context, o =>
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                //Parallel.ForEach(o.Value, a =>
                {
                    foreach (string c in a.Value)
                    {
                        if (!oa.Keys.Contains(new KeyValuePair<string, string>(o.Key, a.Key)))
                        {
                            oa.TryAdd(new KeyValuePair<string, string>(o.Key, a.Key), new ConcurrentDictionary<string, bool>());
                        }
                        if (!oc.Keys.Contains(new KeyValuePair<string, string>(o.Key, c)))
                        {
                            oc.TryAdd(new KeyValuePair<string, string>(o.Key, c), new ConcurrentDictionary<string, bool>());
                        }
                        if (!ac.Keys.Contains(new KeyValuePair<string, string>(a.Key, c)))
                        {
                            ac.TryAdd(new KeyValuePair<string, string>(a.Key, c), new ConcurrentDictionary<string, bool>());
                        }

                        oa[new KeyValuePair<string, string>(o.Key, a.Key)].TryAdd(c, true);
                        oc[new KeyValuePair<string, string>(o.Key, c)].TryAdd(a.Key, true);
                        ac[new KeyValuePair<string, string>(a.Key, c)].TryAdd(o.Key, true);
                    }
                }
            });

            return new KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>>>(oa, new KeyValuePair<ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, string>, ConcurrentDictionary<string, bool>>>(oc, ac));
        }
        
        public List<string> primeOA(string o, string a) // штрих от пары (объект, признак)
        {
            List<string> list = new List<string>();
            if (context.Keys.Contains(o))
            {
                if (context[o].Keys.Contains(a))
                {
                    foreach (string c in context[o][a])
                    {
                        list.Add(c);
                    }
                }
            }
            return list;
        }

        public List<string> primeOC(string o, string c)
        {
            List<string> list = new List<string>();
            if (context.Keys.Contains(o))
            {
                foreach(KeyValuePair<string, List<String>> a in context[o])
                {
                    if(a.Value.Contains(c))
                    {
                        list.Add(a.Key);
                    }
                }
            }
            return list;
        }

        public List<string> primeAC(string a, string c)
        {
            List<string> list = new List<string>();
            foreach(KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                if (o.Value.Keys.Contains(a))
                {
                    if (o.Value[a].Contains(c))
                    {
                        list.Add(o.Key);
                    }
                }
            }
            return list;
        }

        #endregion

        #region Box operators

        public List<string> boxO(List<KeyValuePair<string, string>> oa, List<KeyValuePair<string, string>> oc) // oa and oc are the results of prime operators application
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> s in oa)
            {
                if (!list.Contains(s.Key))
                {
                    list.Add(s.Key);
                }
            }
            foreach (KeyValuePair<string, string> s in oc)
            {
                if (!list.Contains(s.Key))
                {
                    list.Add(s.Key);
                }
            }
            return list;
        }

        public List<string> boxA(List<KeyValuePair<string, string>> oa, List<KeyValuePair<string, string>> ac) // oa and ac are the results of prime operators application
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> s in oa)
            {
                if (!list.Contains(s.Value))
                {
                    list.Add(s.Value);
                }
            }
            foreach (KeyValuePair<string, string> s in ac)
            {
                if (!list.Contains(s.Key))
                {
                    list.Add(s.Key);
                }
            }
            return list;
        }

        public List<string> boxC(List<KeyValuePair<string, string>> oc, List<KeyValuePair<string, string>> ac) // oc and ac are the results of prime operators application
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, string> s in oc)
            {
                if (!list.Contains(s.Value))
                {
                    list.Add(s.Value);
                }
            }
            foreach (KeyValuePair<string, string> s in ac)
            {
                if (!list.Contains(s.Value))
                {
                    list.Add(s.Value);
                }
            }
            return list;
        }

        public List<string> boxO(List<string> oa, List<string> oc) // oa and oc are the results of prime operators application
        {
            List<string> list = new List<string>();
            /*foreach (KeyValuePair<string, string> s in oa)
            {
                if (!list.Contains(s.Key))
                {
                    list.Add(s.Key);
                }
            }
            foreach (KeyValuePair<string, string> s in oc)
            {
                if (!list.Contains(s.Key))
                {
                    list.Add(s.Key);
                }
            }*/

            list = (oa.Union<string>(oc)).ToList<string>();
            return list;
        }

        public List<string> boxA(List<string> oa, List<string> ac) // oa and ac are the results of prime operators application
        {
            List<string> list = new List<string>();
            list = (oa.Union<string>(ac)).ToList<string>();
            return list;
        }

        public List<string> boxC(List<string> oc, List<string> ac) // oc and ac are the results of prime operators application
        {
            List<string> list = new List<string>();
            list = (oc.Union<string>(ac)).ToList<string>();
            return list;
        }

        #region commented

        /*
        public KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>>> generalBoxes()
        {
            KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, KeyValuePair<Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>, Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>>>> primes = generalPrimes();

            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oa = primes.Key;
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> oc = primes.Value.Key;
            Dictionary<string, KeyValuePair<SortedSet<string>, SortedSet<string>>> ac = primes.Value.Value;

            Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>> oSet = new Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>(ac.Count);
            Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>> aSet = new Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>(oc.Count);
            Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>> cSet = new Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>(oa.Count);

            /*foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                {
                    foreach (string c in a.Value)
                    {
                        KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o.Key, new KeyValuePair<string, string>(a.Key, c));
                        if (!oSet.Keys.Contains(cur))
                        {
                            oSet.Add(cur, new List<string>());
                        }
                        if (!aSet.Keys.Contains(cur))
                        {
                            aSet.Add(cur, new List<string>());
                        }
                        if (!cSet.Keys.Contains(cur))
                        {
                            cSet.Add(cur, new List<string>());
                        }

                        //if (!oa.Contains(new KeyValuePair<string, string>(o.Key, a.Key)))
                        if(!oSet[cur].Contains(o.Key))
                        {
                            //oa.Add(new KeyValuePair<string, string>(o.Key, a.Key)); // для с
                            oSet[cur].Add(o.Key);
                        }
                        if (!aSet[cur].Contains(a.Key))
                        {
                            aSet[cur].Add(a.Key);
                        }
                        if (!cSet[cur].Contains(c))
                        {
                            cSet[cur].Add(c);
                        }
                    }
                }
            }

            foreach (string o in extent)
            {
                foreach (string a in intent)
                {
                    foreach (string c in modus)
                    {
                        if (get(o, a, c))
                        {
                            KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o, new KeyValuePair<string, string>(a, c));
                            if (!oSet.Keys.Contains(cur))
                            {
                                oSet.Add(cur, new List<string>());
                            }
                            if (!aSet.Keys.Contains(cur))
                            {
                                aSet.Add(cur, new List<string>());
                            }
                            if (!cSet.Keys.Contains(cur))
                            {
                                cSet.Add(cur, new List<string>());
                            }

                            //if (!oa.Contains(new KeyValuePair<string, string>(o.Key, a.Key)))
                            if (!oSet[cur].Contains(o))
                            {
                                //oa.Add(new KeyValuePair<string, string>(o.Key, a.Key)); // для с
                                oSet[cur].Add(o);
                            }
                            if (!aSet[cur].Contains(a))
                            {
                                aSet[cur].Add(a);
                            }
                            if (!cSet[cur].Contains(c))
                            {
                                cSet[cur].Add(c);
                            }
                        }
                    }
                }
            }*/

            /*foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            {
                foreach (KeyValuePair<string, List<string>> a in o.Value)
                {
                    foreach (string c in a.Value)
                    {
                        KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o.Key, new KeyValuePair<string, string>(a.Key, c));

                        oSet.Add(cur, new List<string>());
                        aSet.Add(cur, new List<string>());
                        cSet.Add(cur, new List<string>());

                        /*foreach (KeyValuePair<string, string> pOA in oa[c])
                        {
                            oSet[cur].Add(pOA.Key);
                            aSet[cur].Add(pOA.Value);
                        }
                        foreach (KeyValuePair<string, string> pOC in oc[a.Key])
                        {
                            oSet[cur].Add(pOC.Key);
                            cSet[cur].Add(pOC.Value);
                        }
                        foreach (KeyValuePair<string, string> pAC in ac[o.Key])
                        {
                            aSet[cur].Add(pAC.Key);
                            cSet[cur].Add(pAC.Value);
                        }*/

                        /*oSet[cur].AddRange(oa[c].Key.Union<string>(oc[a.Key].Key));
                        aSet[cur].AddRange(oa[c].Value.Union<string>(ac[o.Key].Key));
                        cSet[cur].AddRange(oc[a.Key].Value.Union<string>(ac[o.Key].Value));
                    }
                }
            }

            return new KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>>>(oSet, new KeyValuePair<Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>, Dictionary<KeyValuePair<string, KeyValuePair<string, string>>, List<string>>>(aSet, cSet));
        }*/

        /*public KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>>> generalBoxesParallel()
        {
            KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, KeyValuePair<ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>, ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>>>> primes = generalPrimesParallel();

            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> oa = primes.Key;
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> oc = primes.Value.Key;
            ConcurrentDictionary<string, KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>>> ac = primes.Value.Value;

            ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>> oSet = new ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>();
            ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>> aSet = new ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>();
            ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>> cSet = new ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>();

            //foreach (KeyValuePair<string, Dictionary<string, List<string>>> o in context)
            Parallel.ForEach(context, o =>
            {
                //foreach (KeyValuePair<string, List<string>> a in o.Value)
                Parallel.ForEach(o.Value, a =>
                {
                    foreach (string c in a.Value)
                    {
                        KeyValuePair<string, KeyValuePair<string, string>> cur = new KeyValuePair<string, KeyValuePair<string, string>>(o.Key, new KeyValuePair<string, string>(a.Key, c));

                        oSet.TryAdd(cur, new ConcurrentDictionary<string, bool>());
                        aSet.TryAdd(cur, new ConcurrentDictionary<string, bool>());
                        cSet.TryAdd(cur, new ConcurrentDictionary<string, bool>());
                        
                        /*foreach (KeyValuePair<ConcurrentDictionary<string, bool>, ConcurrentDictionary<string, bool>> pOA in oa[c])
                        {
                            oSet[cur].Add(pOA.Key);
                            aSet[cur].Add(pOA.Value);
                        }
                        foreach (KeyValuePair<string, string> pOC in oc[a.Key])
                        {
                            oSet[cur].Add(pOC.Key);
                            cSet[cur].Add(pOC.Value);
                        }
                        foreach (KeyValuePair<string, string> pAC in ac[o.Key])
                        {
                            aSet[cur].Add(pAC.Key);
                            cSet[cur].Add(pAC.Value);
                        }*/

        /*List<string> temp =  new List<string>((oa[c].Key.Keys.ToList<string>()).Union<string>(oc[a.Key].Key.Keys.ToList<string>()));
        foreach (string s in temp)
        {
            oSet[cur].TryAdd(s, true);
        }
        temp = new List<string>((oa[c].Value.Keys.ToList<string>()).Union<string>(ac[o.Key].Key.Keys.ToList<string>()));
        foreach (string s in temp)
        {
            aSet[cur].TryAdd(s, true);
        }
        temp = new List<string>((oc[a.Key].Value.Keys.ToList<string>()).Union<string>(ac[o.Key].Value.Keys.ToList<string>()));
        foreach (string s in temp)
        {
            cSet[cur].TryAdd(s, true);
        }
        temp.Clear();
    }
});
});

return new KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>>>(oSet, new KeyValuePair<ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>, ConcurrentDictionary<KeyValuePair<string, KeyValuePair<string, string>>, ConcurrentDictionary<string, bool>>>(aSet, cSet));
}
*/

        #endregion

        #endregion

        public KeyValuePair<double, double> CalculateDensityAndCoverage(Tricluster tricluster) // для данного трикластера вовращает пару (плотность, покрытие)
        {
            double count = 0;
            foreach (string o in tricluster.extent)
            {
                foreach (string a in tricluster.intent)
                {
                    foreach (string c in tricluster.modus)
                    {
                        if (Contains(o, a, c))
                        {
                            count++;
                        }
                    }
                }
            }

            return new KeyValuePair<double, double>(count / (tricluster.extent.Count * tricluster.intent.Count * tricluster.modus.Count), count / this.count);
        }

        public void AddTriple(string o, string a, string c) // добавляет тройку к контексту
        {
            if (!context.ContainsKey(o))
            {
                context.Add(o, new Dictionary<string, List<string>>() { { a, new List<string>() { c } } });
            }
            else if (!context[o].ContainsKey(a))
            {
                context[o].Add(a, new List<string>() { c });
            }
            //else if (!context[o][a].Contains(c))
            //{
                context[o][a].Add(c);
            //}
        }

        public void DecypherTricluster(Tricluster tricl) // заменяет индексы в трикластере на имена объектов / признаков / условий
        {
            for (int i = 0; i < tricl.extent.Count; i++)
            {
                if (eNames.ContainsKey(tricl.extent[i]))
                {
                    tricl.extent[i] = eNames[tricl.extent[i]];
                }
            }
            for (int i = 0; i < tricl.intent.Count; i++)
            {
                if (iNames.ContainsKey(tricl.intent[i]))
                {
                tricl.intent[i] = iNames[tricl.intent[i]];
                }
            }
            for (int i = 0; i < tricl.modus.Count; i++)
            {
                if (mNames.ContainsKey(tricl.modus[i]))
                {
                    tricl.modus[i] = mNames[tricl.modus[i]];
                }
            }
        }
    }
}
