using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ultimate_Triclustering
{
    class DyadicContext<oT, aT> // диадический контекст (для триаса)
    {
        private List<oT> objs;
        private List<aT> attrs;

        private Dictionary<oT, List<aT>> context;
        private Dictionary<aT, List<oT>> invContext; // формально, не обязательно, но если память позволяет, значительно ускоряет работу

        public DyadicContext(Dictionary<oT, List<aT>> context, List<oT> extent, List<aT> intent)
        {
            this.context = context;
            this.objs = extent;
            this.attrs = intent;

            invContext = new Dictionary<aT, List<oT>>(intent.Count);

            foreach (aT a in intent)
            {
                invContext.Add(a, new List<oT>());
            }

            foreach (oT o in context.Keys)
            {
                foreach (aT a in context[o])
                {
                    if(!invContext[a].Contains(o))
                    invContext[a].Add(o);
                }
            }

            //this.intent = Sort();
        }

        public DyadicContext(List<KeyValuePair<oT, aT>> cont)
        {
            context = new Dictionary<oT, List<aT>>();
            invContext = new Dictionary<aT, List<oT>>();
            HashSet<oT> objects = new HashSet<oT>();
            HashSet<aT> attributes = new HashSet<aT>();
            foreach (KeyValuePair<oT, aT> p in cont)
            {
                objects.Add(p.Key);
                attributes.Add(p.Value);
            }
            objs = objects.ToList<oT>();
            attrs = attributes.ToList<aT>();

            foreach (oT o in objects)
            {
                context.Add(o, new List<aT>());
            }
            foreach (aT a in attributes)
            {
                invContext.Add(a, new List<oT>());
            }

            foreach (KeyValuePair<oT, aT> p in cont)
            {
                context[p.Key].Add(p.Value);
                invContext[p.Value].Add(p.Key);
            }

            //this.intent = Sort();
        }

        public bool Contains(oT o, aT a)
        {
            if (context.Keys.Contains(o) && context[o].Contains(a))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<oT> Objects
        {
            get { return objs; }
            set { }
        }

        public List<aT> Attributes
        {
            get { return attrs; }
            set { }
        }

        public List<aT> primeO(List<oT> objects) // штрих оператор G -> M
        {
            if(objects.Count == 0)
            {
                List<aT> inten = new List<aT>();
                foreach (aT a in attrs)
                {
                    inten.Add(a);
                }
                return inten;
            }

            if (!context.ContainsKey(objects[0]))
            {
                return new List<aT>();
            }

            List<aT> list = context[objects[0]];;

            for(int i = 1; i < objects.Count; i++)
            {
                if (!context.ContainsKey(objects[i]) || list.Count == 0)
                {
                    return new List<aT>();
                }
                list = (list.Intersect(context[objects[i]])).ToList<aT>();
            }

            return list;
        }

        public List<oT> primeA(List<aT> attributes) // штрих-оператор M -> G
        {
            if (attributes.Count == 0)
            {
                List<oT> ext = new List<oT>();
                foreach (oT o in objs)
                {
                    ext.Add(o);
                }
                return ext;
            }

            if (!invContext.ContainsKey(attributes[0]))
            {
                return new List<oT>();
            }

            List<oT> list = invContext[attributes[0]]; ;

            for (int i = 1; i < attributes.Count; i++)
            {
                if(!invContext.ContainsKey(attributes[i]) || list.Count == 0)
                {
                    return new List<oT>();
                }
                list = (list.Intersect(invContext[attributes[i]])).ToList<oT>();
            }

            return list;
        }

        public int CountO(oT o) // число признаков, соответствующих объекту o
        {
            return context[o].Count();
        }

        public int CountA(aT a) // число объектов, соответствующих признаку a
        {
            return invContext[a].Count();
        }

        public oT maxE() // возвращает объект с максимальным числом признаков
        {
            oT cur = objs[0];
            int max = CountO(cur);
            int curInt;

            for(int i = 1; i < objs.Count; i++)
            {
                curInt = CountO(objs[i]);
                if (curInt > max)
                {
                    cur = objs[i];
                    max = curInt;
                }
            }
            return cur;
        }

        public oT maxE(List<oT> A) // возвращает объект с максимальным числом признаков (исключая объекты из A)
        {
            List<oT> list = (objs.Except(A)).ToList<oT>();
            if (list.Count == 0)
            {
                return default(oT);
            }
            else
            {
                oT cur = list[0];
                int max = CountO(cur);
                int curInt;

                for (int i = 1; i < list.Count; i++)
                {
                    curInt = CountO(list[i]);
                    if (curInt > max)
                    {
                        cur = list[i];
                        max = curInt;
                    }
                }
                return cur;
            }
        }

        public oT maxE(List<oT> A, oT g) // возвращает объект с максимальным числом признаков (исключая объекты из A) такой, что число его признаков меньше числа признаков g
        {
            List<oT> list = (objs.Except(A)).ToList<oT>();
            if (list.Count == 0)
            {
                return default(oT);
            }
            else
            {
                oT cur = default(oT);
                int max = 0;
                int curInt;

                for (int i = 1; i < list.Count; i++)
                {
                    curInt = CountO(list[i]);
                    if (curInt > max && curInt < CountO(g))
                    {
                        cur = list[i];
                        max = curInt;
                    }
                }
                return cur;
            }
        }

        /*public List<eT> SortAlt()
        {
            //Dictionary<int, int> map = new Dictionary<int, int>();

            List<eT> list = new List<eT>();
            if (extent.Count == 1)
            {
                list.Add(extent[0]);
                return list;
            }

            //int count;
            List<KeyValuePair<eT, int>> tempList = new List<KeyValuePair<eT, int>>();
            List<KeyValuePair<eT, int>> smallList = new List<KeyValuePair<eT, int>>();

            foreach (eT o in extent)
            {
                tempList.Add(new KeyValuePair<eT, int>(o, CountO(o)));
            }

            //tempList = (tempList.OrderByDescending(p => p.Value)).ToList<KeyValuePair<eT, int>>();
            tempList = (tempList.OrderBy(p => p.Value)).ToList<KeyValuePair<eT, int>>();

            //list.Add(tempList[0].Key);
            smallList.Add(tempList[0]);
            
            for (int i = 1; i < tempList.Count; i++)
            {
                if (tempList[i].Value == tempList[i - 1].Value && i != (tempList.Count - 1))
                {
                    smallList.Add(tempList[i]);
                }
                else
                {
                    if (tempList[i].Value == tempList[i - 1].Value && i == (tempList.Count - 1))
                    {
                        smallList.Add(tempList[i]);
                    }

                    /*while (smallList.Count != 0)
                    {
                        int max = -1;
                        KeyValuePair<eT, int> cur = new KeyValuePair<eT, int>();
                        int inter;
                        for (int j = 0; j < smallList.Count; j++)
                        {
                            inter = (context[smallList[j].Key].Intersect(context[list[0]])).Count();
                            if (inter > max)
                            {
                                cur = smallList[j];
                                max = inter;
                            }
                        }
                        list.Insert(0, cur.Key);
                        smallList.Remove(cur);                        
                    }*/

                    /*smallList = (smallList.OrderBy(p => sVal(p.Key))).ToList<KeyValuePair<eT, int>>();

                    for (int j = 0; j < smallList.Count; j++)
                    {
                        list.Add(smallList[j].Key);
                    }

                    smallList = new List<KeyValuePair<eT, int>>();
                    smallList.Add(tempList[i]);

                    if (tempList[i].Value != tempList[i - 1].Value && i == (tempList.Count - 1))
                    {
                        list.Add(tempList[i].Key);
                    }
                }
            }

            return list;
        }*/

        public double sVal(oT o) // относительное число единиц в замыкании объекта o
        {
            return (((double)(primeA(context[o])).Count()) / objs.Count);
        }

        /*public List<eT> Sort()
        {
            //Dictionary<int, int> map = new Dictionary<int, int>();

            List<eT> list = new List<eT>();
            if (extent.Count == 1)
            {
                list.Add(extent[0]);
                return list;
            }

            //int count;
            List<KeyValuePair<eT, int>> tempList = new List<KeyValuePair<eT, int>>();

            foreach (eT o in extent)
            {
                tempList.Add(new KeyValuePair<eT, int>(o, CountO(o)));
            }

            tempList = (tempList.OrderByDescending(p => p.Value)).ToList<KeyValuePair<eT, int>>();

            for (int i = 0; i < tempList.Count; i++)
            {
                list.Add(tempList[i].Key);
            }

            return list;
        }*/

        /*public List<eT> Sort()
        {
            //Dictionary<int, int> map = new Dictionary<int, int>();

            List<eT> list = new List<eT>();
            if (extent.Count == 1)
            {
                list.Add(extent[0]);
                return list;
            }

            //int count;
            List<KeyValuePair<eT, double>> tempList = new List<KeyValuePair<eT, double>>();
            List<KeyValuePair<eT, double>> smallList = new List<KeyValuePair<eT, double>>();

            foreach (eT o in extent)
            {
                tempList.Add(new KeyValuePair<eT, double>(o, sVal(o)));
            }

            tempList = (tempList.OrderByDescending(p => p.Value)).ToList<KeyValuePair<eT, double>>();

            //list.Add(tempList[0].Key);
            smallList.Add(tempList[0]);

            for (int i = 1; i < tempList.Count; i++)
            {
                if (tempList[i].Value == tempList[i - 1].Value && i != (tempList.Count - 1))
                {
                    smallList.Add(tempList[i]);
                }
                else
                {
                    if (tempList[i].Value == tempList[i - 1].Value && i == (tempList.Count - 1))
                    {
                        smallList.Add(tempList[i]);
                    }

                    smallList = (smallList.OrderBy(p => CountO(p.Key))).ToList<KeyValuePair<eT, double>>();

                    for (int j = 0; j < smallList.Count; j++)
                    {
                        list.Add(smallList[j].Key);
                    }

                    smallList = new List<KeyValuePair<eT, double>>();
                    smallList.Add(tempList[i]);

                    if (tempList[i].Value != tempList[i - 1].Value && i == (tempList.Count - 1))
                    {
                        list.Add(tempList[i].Key);
                    }
                }
            }

            return list;
        }*/

        /*public List<eT> Sort()
        {
            List<eT> list = new List<eT>();
            List<iT> diff = new List<iT>();
            List<iT> diffInv = new List<iT>();

            list.Add(extent[0]);
            iT min, minInv;
            int res;
            int j;

            for(int i = 1; i < extent.Count; i++)
            {
                for (j = 0; j < list.Count; j++)
                {
                    diff = (context[extent[i]].Except(context[list[j]])).ToList<iT>();
                    diffInv = (context[list[j]].Except(context[extent[i]])).ToList<iT>();
                    if (diff.Count == 0 && diffInv.Count == 0)
                    {
                        list.Insert(j, extent[i]);
                        break;
                    }
                    else if (diff.Count == 0)
                    {
                        list.Insert(j, extent[i]);
                        break;
                    }
                    else if (diffInv.Count == 0)
                    { }
                    else
                    {
                        //diff.Sort();
                        //diffInv.Sort();
                        min = diff.Min();
                        minInv = diffInv.Min();
                        res = min.ToString().CompareTo(minInv.ToString());
                        if (res == 0)
                        {
                            list.Insert(j, extent[i]);
                            break;
                        }
                        else if (res == 1)
                        {
                            list.Insert(j, extent[i]);
                            break;
                        }
                        else
                        { }
                    }
                }
                if (j == list.Count)
                {
                    list.Add(extent[i]);
                }
            }

            return list;
        }*/

        public List<aT> Sort() // сортировка признаков (для триаса)
        {
            List<aT> list = new List<aT>();
            List<oT> diff = new List<oT>();
            List<oT> diffInv = new List<oT>();

            list.Add(attrs[0]);
            oT min, minInv;
            int res;
            int j;

            for (int i = 1; i < attrs.Count; i++)
            {
                for (j = 0; j < list.Count; j++)
                {
                    diff = (invContext[attrs[i]].Except(invContext[list[j]])).ToList<oT>();
                    diffInv = (invContext[list[j]].Except(invContext[attrs[i]])).ToList<oT>();
                    if (diff.Count == 0 && diffInv.Count == 0)
                    {
                        list.Insert(j, attrs[i]);
                        break;
                    }
                    else if (diff.Count == 0)
                    {
                        list.Insert(j, attrs[i]);
                        break;
                    }
                    else if (diffInv.Count == 0)
                    { }
                    else
                    {
                        //diff.Sort();
                        //diffInv.Sort();
                        min = diff.Min();
                        minInv = diffInv.Min();
                        res = min.ToString().CompareTo(minInv.ToString());
                        if (res == 0)
                        {
                            list.Insert(j, attrs[i]);
                            break;
                        }
                        else if (res == 1)
                        {
                            list.Insert(j, attrs[i]);
                            break;
                        }
                        else
                        { }
                    }
                }
                if (j == list.Count)
                {
                    list.Add(attrs[i]);
                }
            }

            return list;
        }
    }
}
