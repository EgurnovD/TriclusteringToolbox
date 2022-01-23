using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Ultimate_Triclustering
{
    public class Tricluster : IComparable<Tricluster>
    {
        public List<string> extent;
        public List<string> intent;
        public List<string> modus;
        public double variance;
        public double average;
        public double density;
        public double altDensity1; // для оценки плотности (используется для OAC (prime))
        public double altDensity2; // для оценки плотности (используется для OAC (prime))
        public double altDensity3; // для оценки плотности (используется для OAC (prime))
        public double coverage; // покрытие троек контекста
        public double coverageG; // покрытие множества объектов
        public double coverageM; // покрытие множества признаков
        public double coverageB; // покрытие множества условий
        public string hash; // хеш-код (строка)
        public int iHash; // хеш-код (int)
        public bool hashCalculated; // был ли вычислен хеш-код
        public int step; // шаг дробления контекста, на котором был получен данный трикластер (для спектральной кластеризации) (пока не используется, нужно модифицировать метод)

        public Tricluster()
        {
            extent = new List<string>();
            intent = new List<string>();
            modus = new List<string>();
            variance = -1;
            average = -1;
            density = -1;
            altDensity1 = -1;
            altDensity2 = -1;
            altDensity3 = -1;
            coverage = -1;
            hash = "";
            iHash = 0;
            hashCalculated = false;
            step = -1;
        }

        public Tricluster(List<string> content, List<string> intent, List<string> modus)
        {
            this.extent = content;
            this.intent = intent;
            this.modus = modus;
            variance = -1;
            average = -1;
            density = -1;
            altDensity1 = -1;
            altDensity2 = -1;
            altDensity3 = -1;
            coverage = -1;
            hash = "";
            hashCalculated = false;
            CalcIHash();
            step = -1;
        }

        public Tricluster(List<string> content, List<string> intent, List<string> modus, bool calc) // нужно ли вычислять хеш-код сразу
        {
            this.extent = content;
            this.intent = intent;
            this.modus = modus;
            variance = -1;
            average = -1;
            density = -1;
            altDensity1 = -1;
            altDensity2 = -1;
            altDensity3 = -1;
            coverage = -1;
            hash = "";
            if (calc)
            {
                hashCalculated = false;
                CalcIHash();
            }
            else
            {
                iHash = 0;
                hashCalculated = false;
            }
            step = -1;
        }

        public Tricluster(List<string> content, List<string> intent, List<string> modus, double density)
        {
            this.extent = content;
            this.intent = intent;
            this.modus = modus;
            this.density = density;
            variance = -1;
            average = -1;
            altDensity1 = -1;
            altDensity2 = -1;
            altDensity3 = -1;
            coverage = -1;
            hash = "";
            hashCalculated = false;
            CalcIHash();
            step = -1;
        }

        public int Volume
        {
            get { return extent.Count * intent.Count * modus.Count; }
            set { }

        }

        public static bool operator ==(Tricluster a, Tricluster b) // проверяет равенство трикластеров по целочисленному хеш-коду
        {
            if (!a.hashCalculated)
            {
                a.hashCalculated = false;
                a.CalcIHash();
            }
            if (!b.hashCalculated)
            {
                b.hashCalculated = false;
                b.CalcIHash();
            }
            try
            {
                return a.iHash == b.iHash;
            }
            catch (NullReferenceException)
            {
                return false;
            }
        }

        public static bool operator !=(Tricluster a, Tricluster b)
        {
            return !(a == b);
        }

        public void CalcHash(MD5 md5Hash) // вычисляет хеш-код (строка) (больше не используется)
        {
            string temp = "";
            foreach (string s in extent)
            {
                temp += s + ",";
            }
            temp += ";";
            foreach (string s in intent)
            {
                temp += s + ","; 
            }
            temp += ";";
            foreach (string s in modus)
            {
                temp += s + ";";
            }
            hash = GetMd5Hash(md5Hash, temp);
        }

        public void CalcIHash() // вычисляет целочисленный хеш-код
        {
            if (!hashCalculated)
            {
                StringBuilder temp = new StringBuilder();
                extent.Sort();
                foreach (string s in extent)
                {
                    temp.Append(s).Append("\n");
                }
                temp.Append("\n\n");
                intent.Sort();
                foreach (string s in intent)
                {
                    temp.Append(s).Append("\n");
                }
                temp.Append("\n\n");
                modus.Sort();
                foreach (string s in modus)
                {
                    temp.Append(s).Append("\n");
                }
               // iHash = temp.GetHashCode();
                string hashstring = temp.ToString();
                iHash = 0;
                int hashbase = 257;
                for (int i = 0; i < hashstring.Length; ++i)
                {
                    iHash *= hashbase;
                    iHash += (int)hashstring[i];
                }
                //
                hashCalculated = true;
            }
        }

        public static bool operator >=(Tricluster a, Tricluster b) // используется для сортировки трикластеров по плотности
        {
            return a.density >= b.density;
        }

        public static bool operator <=(Tricluster a, Tricluster b) // используется для сортировки трикластеров по плотности
        {
            return a.density <= b.density;
        }

        public static bool operator >(Tricluster a, Tricluster b) // используется для сортировки трикластеров по плотности
        {
            return a.density > b.density;
        }

        public static bool operator <(Tricluster a, Tricluster b) // используется для сортировки трикластеров по плотности
        {
            return a.density < b.density;
        }

        public string Descr() // формирует из трикластера текстовую строку
        {
            string ext = "";
            string inten = "";
            string mod = "";

            for (int k = 0; k < extent.Count; k++)
            {
                ext += extent[k] + ", ";
            }
            ext = ext.Substring(0, ext.Length - 2);

            for (int k = 0; k < intent.Count; k++)
            {
                inten += intent[k] + ", ";
            }
            inten = inten.Substring(0, inten.Length - 2);

            for (int k = 0; k < modus.Count; k++)
            {
                mod += modus[k] + ", ";
            }
            mod = mod.Substring(0, mod.Length - 2);

            string temp = "";

            if (step != -1)
            {
                temp = "\t{" + step + "}";
            }

            if (altDensity1 == -1)
            {
                return ("Density: " + Math.Round(density * 100, 2) + "%\tCoverage: " + Math.Round(coverage * 100, 2) + 
                    "%\tExtent: {" + ext + "}\tIntent: {" + inten + "}\tModus: {" + mod + "}" + temp);
            }
            else
            {
                return ("Density: " + Math.Round(density * 100, 2) + "%\tEstimate 1: " + Math.Round(altDensity1 * 100, 2) + "%\tEstimate 2: " +
                    Math.Round(altDensity2 * 100, 2) + "%\tEstimate 3: " + Math.Round(altDensity3 * 100, 2) + "%\tCoverage: " + 
                    Math.Round(coverage * 100, 2) + "%\tExtent: {" + ext + "}\tIntent: {" + inten + "}\tModus: {" + mod + "}" + temp);
            }
        }

        public string DescrShort() // формирует более короткую строку-описание трикластера (без пояснений, которые будут включены в шапку файла)
        {
            string ext = "";
            string inten = "";
            string mod = "";

            for (int k = 0; k < extent.Count; k++)
            {
                ext += extent[k] + ", ";
            }
            if (ext != "")
            {
                ext = ext.Substring(0, ext.Length - 2);
            }

            for (int k = 0; k < intent.Count; k++)
            {
                inten += intent[k] + ", ";
            }
            if (inten != "")
            {
                inten = inten.Substring(0, inten.Length - 2);
            }

            for (int k = 0; k < modus.Count; k++)
            {
                mod += modus[k] + ", ";
            }
            if (mod != "")
            {
                mod = mod.Substring(0, mod.Length - 2);
            }

            string temp = "";

            if (step != -1)
            {
                temp = "\t{" + step + "}";
            }

            if (altDensity1 == -1)
            {
                return (Math.Round(density * 100, 2) + "\t" + Math.Round(coverage * 100, 2) + "\t" + Math.Round(coverageG * 100, 2) + "\t" + Math.Round(coverageM * 100, 2) + "\t" + Math.Round(coverageB * 100, 2) +
                    "\t<" + ext + ">\t<" + inten + ">\t<" + mod + ">" + temp);
            }
            else
            {
                return (Math.Round(density * 100, 2) + "\t" + Math.Round(altDensity1 * 100, 2) + "\t" + Math.Round(altDensity2 * 100, 2) + "\t" + Math.Round(altDensity3 * 100, 2) + "\t" + Math.Round(coverage * 100, 2) +
                    "\t" + Math.Round(coverageG * 100, 2) + "\t" + Math.Round(coverageM * 100, 2) + "\t" + Math.Round(coverageB * 100, 2) + "\t<" + ext + ">\t<" + inten + ">\t<" + mod + ">" + temp);
            }
        }

        public string DescrNumerical() // формирует короткую строку-описание трикластера (без пояснений, которые будут включены в шапку файла)
        {
            string ext = "";
            string inten = "";
            string mod = "";

            for (int k = 0; k < extent.Count; k++)
            {
                ext += extent[k] + ", ";
            }
            if (ext != "")
            {
                ext = ext.Substring(0, ext.Length - 2);
            }

            for (int k = 0; k < intent.Count; k++)
            {
                inten += intent[k] + ", ";
            }
            if (inten != "")
            {
                inten = inten.Substring(0, inten.Length - 2);
            }

            for (int k = 0; k < modus.Count; k++)
            {
                mod += modus[k] + ", ";
            }
            if (mod != "")
            {
                mod = mod.Substring(0, mod.Length - 2);
            }

            string temp = "";

            if (step != -1)
            {
                temp = "\t{" + step + "}";
            }

            if (variance == -1)
            {
                return (Math.Round(density * 100, 2) + "\t" + Math.Round(coverage * 100, 2) + "\t" + Math.Round(coverageG * 100, 2) + "\t" + Math.Round(coverageM * 100, 2) + "\t" + Math.Round(coverageB * 100, 2) +
                    "\t<" + ext + ">\t<" + inten + ">\t<" + mod + ">" + temp);
            }
            else
            {
                return (Math.Round(density * 100, 2) + "\t" + Math.Round(variance, 2) + "\t" + Math.Round(average, 2) + "\t" + Math.Round(coverage * 100, 2) +
                    "\t" + Math.Round(coverageG * 100, 2) + "\t" + Math.Round(coverageM * 100, 2) + "\t" + Math.Round(coverageB * 100, 2) + "\t<" + ext + ">\t<" + inten + ">\t<" + mod + ">" + temp);
            }
        }

        public int CompareTo(Tricluster b) // сравнение трикластеров по плотности
        {
            if (b.IsNull())
                return 1;

            return density.CompareTo(b.density);
        }

        public bool IsNull()
        {
            try
            {
                if (density == 0) { }
            }
            catch (NullReferenceException)
            { return true; }
            return false;
        }

        public string GetMd5Hash(MD5 md5Hash, string input) // непосредсвенно вычисление хеш-кода на основе любой строки
        {

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public override int GetHashCode() // возвращает хеш-код
        {
            if (!hashCalculated)
            {
                CalcIHash();
                //hashCalculated = true;
            }
            return iHash;
        }
        public override bool Equals(object obj) // аналог ==
        {
            return this == (Tricluster)(obj);
        }

        public double Similarity(Tricluster t) // вычисляет сходство трикластера t с данным
        {
            return (((double)(extent.Intersect(t.extent)).Count()) * (intent.Intersect(t.intent)).Count() * (modus.Intersect(t.modus)).Count() / ((extent.Union(t.extent)).Count() * (intent.Union(t.intent)).Count() * (modus.Union(t.modus)).Count()));
        }

        public Tricluster Copy() // возвращает копию трикластера (для DataPeeler'а)
        {
            Tricluster tricluster = new Tricluster();
            tricluster.extent = extent.ToList<string>();
            tricluster.intent = intent.ToList<string>();
            tricluster.modus = modus.ToList<string>();

            tricluster.density = density;
            tricluster.altDensity1 = altDensity1;
            tricluster.altDensity2 = altDensity2;
            tricluster.altDensity3 = altDensity3;
            tricluster.coverage = coverage;
            tricluster.hash = hash;
            tricluster.iHash = iHash;
            tricluster.hashCalculated = hashCalculated;
            tricluster.step = step;

            return tricluster;
        }

        public Tricluster Union(Tricluster clus) // объединение трикластеров
        {
            Tricluster tricluster = new Tricluster();

            tricluster.extent = (extent.Union(clus.extent)).ToList<string>();
            tricluster.intent = (intent.Union(clus.intent)).ToList<string>();
            tricluster.modus = (modus.Union(clus.modus)).ToList<string>();

            return tricluster;
        }
    }
}
