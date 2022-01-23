using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ultimate_Triclustering
{
    class Loader
    {
        public Loader()
        {

        }

        public TriadicContext LoadContext(string path, int firstTriples) // Loads context
        {
            Dictionary<string, Dictionary<string, List<string>>> context = new Dictionary<string, Dictionary<string, List<string>>>();
            List<string> extent = new List<string>();
            List<string> intent = new List<string>();
            List<string> modus = new List<string>();
            Dictionary<string, string> eNames = new Dictionary<string, string>();
            Dictionary<string, string> iNames = new Dictionary<string, string>();
            Dictionary<string, string> mNames = new Dictionary<string, string>();
            Dictionary<string, string> eNew = new Dictionary<string, string>();
            Dictionary<string, string> iNew = new Dictionary<string, string>();
            Dictionary<string, string> mNew = new Dictionary<string, string>();
            StreamReader sr = new StreamReader(path, Encoding.UTF8); // Context must be in the list form, i.e. each string is an triple of triadic context: "object'\t'attribute'\t'condition"

            string cur;
            string obj, attr, cond;

            int count = 0;

            int eCount = 0, iCount = 0, mCount = 0;

            while ((((cur = sr.ReadLine()) != null) && (cur != "")) && (count < firstTriples))
            {
                if (cur.Substring(0, 2) == "//")
                    continue;
                
                obj = cur.Substring(0, cur.IndexOf('\t'));
                cur = cur.Substring(cur.IndexOf('\t') + 1);
                attr = cur.Substring(0, cur.IndexOf('\t'));
                cond = cur.Substring(cur.IndexOf('\t') + 1);

                if (!eNew.Keys.Contains(obj))
                {
                    eNew.Add(obj, eCount.ToString());
                    eNames.Add(eCount.ToString(), obj);
                    eCount++;
                }
                if (!iNew.Keys.Contains(attr))
                {
                    iNew.Add(attr, iCount.ToString());
                    iNames.Add(iCount.ToString(), attr);
                    iCount++;
                }
                if (!mNew.Keys.Contains(cond))
                {
                    mNew.Add(cond, mCount.ToString());
                    mNames.Add(mCount.ToString(), cond);
                    mCount++;
                }

                if (!extent.Contains(eNew[obj]))
                {
                    extent.Add(eNew[obj]);
                }
                if (!intent.Contains(iNew[attr]))
                {
                    intent.Add(iNew[attr]);
                }
                if (!modus.Contains(mNew[cond]))
                {
                    modus.Add(mNew[cond]);
                }
                if (!context.Keys.Contains(eNew[obj]))
                {
                    Dictionary<string, List<string>> temp1 = new Dictionary<string, List<string>>();
                    List<string> temp2 = new List<string>();
                    temp2.Add(mNew[cond]);
                    temp1.Add(iNew[attr], temp2);
                    context.Add(eNew[obj], temp1);
                }
                else if (!context[eNew[obj]].Keys.Contains(iNew[attr]))
                {
                    List<string> temp = new List<string>();
                    temp.Add(mNew[cond]);
                    context[eNew[obj]].Add(iNew[attr], temp);
                }
                else if (!context[eNew[obj]][iNew[attr]].Contains(mNew[cond]))
                {
                    context[eNew[obj]][iNew[attr]].Add(mNew[cond]);
                }

                count++;
            }

            TriadicContext tCont = new TriadicContext(context, extent, intent, modus, eNames, iNames, mNames);
            tCont.Count = count;
            tCont.Volume = extent.Count * intent.Count * modus.Count;
            return tCont;
        }

        public TriadicContext LoadContext(string path) // Loads context
        {
            Dictionary<string, Dictionary<string, List<string>>> context = new Dictionary<string, Dictionary<string, List<string>>>();
            List<string> extent = new List<string>();
            List<string> intent = new List<string>();
            List<string> modus = new List<string>(); ;
            Dictionary<string, string> eNames = new Dictionary<string, string>();
            Dictionary<string, string> iNames = new Dictionary<string, string>();
            Dictionary<string, string> mNames = new Dictionary<string, string>();
            Dictionary<string, string> eNew = new Dictionary<string, string>();
            Dictionary<string, string> iNew = new Dictionary<string, string>();
            Dictionary<string, string> mNew = new Dictionary<string, string>();
            StreamReader sr = new StreamReader(path, Encoding.ASCII); // Context must be in the list form, i.e. each string is an triple of triadic context: "object'\t'attribute'\t'condition"

            string cur;
            string obj, attr, cond;

            int count = 0;

            int eCount = 0, iCount = 0, mCount = 0;

            while (((cur = sr.ReadLine()) != null) && (cur != ""))
            {
                if (cur.Substring(0, 2) == "//")
                    continue;
                
                obj = cur.Substring(0, cur.IndexOf('\t'));
                cur = cur.Substring(cur.IndexOf('\t') + 1);
                attr = cur.Substring(0, cur.IndexOf('\t'));
                cond = cur.Substring(cur.IndexOf('\t') + 1);

                if (!eNew.Keys.Contains(obj))
                {
                    eNew.Add(obj, eCount.ToString());
                    eNames.Add(eCount.ToString(), obj);
                    eCount++;
                }
                if (!iNew.Keys.Contains(attr))
                {
                    iNew.Add(attr, iCount.ToString());
                    iNames.Add(iCount.ToString(), attr);
                    iCount++;
                }
                if (!mNew.Keys.Contains(cond))
                {
                    mNew.Add(cond, mCount.ToString());
                    mNames.Add(mCount.ToString(), cond);
                    mCount++;
                }

                if (!extent.Contains(eNew[obj]))
                {
                    extent.Add(eNew[obj]);
                }
                if (!intent.Contains(iNew[attr]))
                {
                    intent.Add(iNew[attr]);
                }
                if (!modus.Contains(mNew[cond]))
                {
                    modus.Add(mNew[cond]);
                }
                if (!context.Keys.Contains(eNew[obj]))
                {
                    Dictionary<string, List<string>> temp1 = new Dictionary<string, List<string>>();
                    List<string> temp2 = new List<string>();
                    temp2.Add(mNew[cond]);
                    temp1.Add(iNew[attr], temp2);
                    context.Add(eNew[obj], temp1);
                }
                else if (!context[eNew[obj]].Keys.Contains(iNew[attr]))
                {
                    List<string> temp = new List<string>();
                    temp.Add(mNew[cond]);
                    context[eNew[obj]].Add(iNew[attr], temp);
                }
                else if (!context[eNew[obj]][iNew[attr]].Contains(mNew[cond]))
                {
                    context[eNew[obj]][iNew[attr]].Add(mNew[cond]);
                }

                count++;
            }

            TriadicContext tCont = new TriadicContext(context, extent, intent, modus, eNames, iNames, mNames);
            tCont.Count = count;
            tCont.Volume = extent.Count * intent.Count * modus.Count;
            return tCont;
        }
        
        public NumericalTriadicContext LoadNumericalContext(string path, int firstTriples = -1) // Loads numerical context
        {
            Dictionary<string, Dictionary<string, Dictionary<string, double>>> context = new Dictionary<string, Dictionary<string, Dictionary<string, double>>>();
            List<string> extent = new List<string>();
            List<string> intent = new List<string>();
            List<string> modus = new List<string>(); ;
            Dictionary<string, string> eNames = new Dictionary<string, string>();
            Dictionary<string, string> iNames = new Dictionary<string, string>();
            Dictionary<string, string> mNames = new Dictionary<string, string>();
            Dictionary<string, string> eNew = new Dictionary<string, string>();
            Dictionary<string, string> iNew = new Dictionary<string, string>();
            Dictionary<string, string> mNew = new Dictionary<string, string>();
            StreamReader sr = new StreamReader(path, Encoding.UTF8); // Context must be in the list form, i.e. each string is an triple of triadic context: "object'\t'attribute'\t'condition"

            string cur;
            string obj, attr, cond;
            double val;

            int count = 0;

            int eCount = 0, iCount = 0, mCount = 0;

            while ((((cur = sr.ReadLine()) != null) && (cur != "")) && ((firstTriples < 0) || (count < firstTriples)))
            {
                if (cur.Substring(0, 2) == "//")
                    continue;
                
                string[] split = cur.Split(new Char[] { '\t' });
                obj = split[0].Trim(); //cur.Substring(0, cur.IndexOf('\t'));
                attr = split[1].Trim(); //cur.Substring(0, cur.IndexOf('\t'));
                cond = split[2].Trim(); //cur.Substring(cur.IndexOf('\t') + 1);
                // globalisation issues;
                char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
                val = Convert.ToDouble(split[3].Replace('.', separator).Trim());

                if (!eNew.Keys.Contains(obj))
                {
                    eNew.Add(obj, eCount.ToString());
                    eNames.Add(eCount.ToString(), obj);
                    eCount++;
                }
                if (!iNew.Keys.Contains(attr))
                {
                    iNew.Add(attr, iCount.ToString());
                    iNames.Add(iCount.ToString(), attr);
                    iCount++;
                }
                if (!mNew.Keys.Contains(cond))
                {
                    mNew.Add(cond, mCount.ToString());
                    mNames.Add(mCount.ToString(), cond);
                    mCount++;
                }

                if (!extent.Contains(eNew[obj]))
                {
                    extent.Add(eNew[obj]);
                }
                if (!intent.Contains(iNew[attr]))
                {
                    intent.Add(iNew[attr]);
                }
                if (!modus.Contains(mNew[cond]))
                {
                    modus.Add(mNew[cond]);
                }
                if (!context.Keys.Contains(eNew[obj]))
                {
                    Dictionary<string, Dictionary<string, double>> temp1 = new Dictionary<string, Dictionary<string, double>>();
                    Dictionary<string, double> temp2 = new Dictionary<string, double>();
                    temp2.Add(mNew[cond], val);
                    temp1.Add(iNew[attr], temp2);
                    context.Add(eNew[obj], temp1);
                }
                else if (!context[eNew[obj]].Keys.Contains(iNew[attr]))
                {
                    Dictionary<string, double> temp = new Dictionary<string, double>();
                    temp.Add(mNew[cond], val);
                    context[eNew[obj]].Add(iNew[attr], temp);
                }
                else if (!context[eNew[obj]][iNew[attr]].Keys.Contains(mNew[cond]))
                {
                    context[eNew[obj]][iNew[attr]].Add(mNew[cond], val);
                }

                count++;
            }

            NumericalTriadicContext tCont = new NumericalTriadicContext(context, extent, intent, modus, eNames, iNames, mNames);
            tCont.Count = count;
            tCont.Volume = extent.Count * intent.Count * modus.Count;
            return tCont;
        }
    }
}
