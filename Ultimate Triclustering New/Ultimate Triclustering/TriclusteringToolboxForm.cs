using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Globalization;

namespace Ultimate_Triclustering
{
    public partial class TriclustringToolboxForm : Form
    {
        TriclusteringMaster Triclusterer; // Содержит методы трикластеризации и контекст
        Loader ContextLoader; // Читает набор троек из файла и загружает их в контекст
        bool prevCheckParallel, prevForbParallel, prevCheckInjections, prevForbInjections;
        BindingList<ConstraintClass> constraints; // Набор ограничений

        public TriclustringToolboxForm()
        {
            InitializeComponent();
            openFileDialog.InitialDirectory = "D:\\Dropbox\\Diploma\\utils\\ContextGenerator\\ContextGenerator"; // \\MovieLensPrepare\\MovieLensPrepare"; 
            //"D:\\YandexDisk\\HSE\\Диплом\\results"; // папка по умолчанию для контекста
            folderBrowserDialog.SelectedPath = "D:\\Dropbox\\Diploma\\results\\"; //"D:\\YandexDisk\\HSE\\Диплом\\results\\"; // папка по умолчанию для результатов
            tbOutput.Text = folderBrowserDialog.SelectedPath;
            cbMethod.SelectedIndex = 0;
            //lblProc.Text = progressBar.Value + "%";
            prevForbParallel = false;
            prevForbInjections = true;

            MD5 md5Hash = MD5.Create(); // хеш-функция (на данный момент не используется, ибо можно проще)
            Triclusterer = new TriclusteringMaster(md5Hash);
            ContextLoader = new Loader();

            constraints = new BindingList<ConstraintClass>();
            tbContext.Text = openFileDialog.InitialDirectory + "\\" + openFileDialog.FileName + "." + openFileDialog.DefaultExt;
           
            constraints.AllowNew = true;
            constraints.AllowRemove = true;
            constraints.AllowEdit = true;
            dgConstraints.DataSource = constraints;
        }

        private void btContext_Click(object sender, EventArgs e) // диалог выбора файла контекста
        {
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != null)
            {
                tbContext.Text = openFileDialog.FileName;
            }
        }

        private void btOutput_Click(object sender, EventArgs e) // диалог выбора папки для результатов
        {
            folderBrowserDialog.ShowDialog();
            if (folderBrowserDialog.SelectedPath != null)
            {
                tbOutput.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void btStart_Click(object sender, EventArgs e) // Launches triclustering with given options
        {
            StreamWriter swLog = null;
            StreamWriter sw = null;
            try
            {
                bool specFlag = false; // запускать ли расчёт сходства для тестов на шумоустойчивость
                if (folderBrowserDialog.SelectedPath[folderBrowserDialog.SelectedPath.Length - 1] != '\\')
                {
                    //folderBrowserDialog.SelectedPath += "\\";
                    tbOutput.Text = folderBrowserDialog.SelectedPath + "\\";
                }

                string par = "";
                if (ckbParallel.Checked) // если выбран параллельный алгоритм, нужно к названию файла дописывать соответствующую метку
                {
                    par = " (parallel)";
                }
                string contextFileName = Path.GetFileNameWithoutExtension(tbContext.Text);
                swLog = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + "_" + contextFileName + " " + par + "- log.txt", true, Encoding.Unicode);

                Stopwatch stopwatch = new Stopwatch(); // инициализация таймера

                List<Tricluster> triclusterSet;

                List<KeyValuePair<Tricluster, double>> best;

                StreamWriter swTemp;

                //double totalCoverage;

                swLog.WriteLine("Parameters\tLimit\tTime\tCount\tAverage Variance\tAverage Density\tTotal coverage\tObjects coverage\tAttributes coverage\tModus coverage\tTotal diversity\tObjects diversity\tAttributes diversity\tModus diversity"); // "Шапка" лога

                string limit = "no";

                if (!Triclusterer.IsLoaded) // загружает контекст, если не загружен и/или изменился
                {
                    if (ckbFirstTriples.Checked)
                    {
                        limit = tbFirstTriples.Text;
                        Triclusterer.LoadContext(ContextLoader.LoadContext(tbContext.Text, Convert.ToInt32(tbFirstTriples.Text)));
                    }
                    else
                    {
                        Triclusterer.LoadContext(ContextLoader.LoadContext(tbContext.Text));
                    }
                }
                switch (cbMethod.SelectedItem.ToString()) // запускает метод с необходимыми предварительными процедурами и процедурами постобработки
                {
                    case "Trias": // возможно, из-за большого пересечения, стоит попытаться вынести большую часть кода в отдельную функцию
                        string[] paramSet = tbOptions.Text.Split('|'); // условия минимальных поддержек для объёма содержания и модуса разделяются '|'
                        List<List<double>> parameters = new List<List<double>>();  // в условиях для любого из множеств может быть по несколько значений. Тогда алгоритм запустится для всех возможных комбинаций
                        for (int i = 0; i < 3; i++) // для каждого из 3 множеств считываем значения параметров
                        {
                            parameters.Add(new List<double>());
                            foreach (string p in paramSet[i].Split(';'))
                            {
                                if (!p.Contains(':'))
                                {
                                    if (!p.Contains('*'))
                                    {
                                        parameters[i].Add(Convert.ToDouble(p));
                                    }
                                    else
                                    {
                                        parameters[i].Add(Convert.ToDouble(p.Substring(1)) * Triclusterer.ContextCounts()[i]);
                                    }
                                }
                                else
                                {
                                    int coef;
                                    string pp = p;
                                    if (!p.Contains('*')) // минимульную поддержку можно также задать в виде части от размера соответствующего множества
                                    {
                                        coef = 1;
                                    }
                                    else
                                    {
                                        coef = Triclusterer.ContextCounts()[i];
                                        pp = p.Substring(1);
                                    }
                                    string str1 = pp.Substring(0, pp.IndexOf(':'));
                                    string str2 = pp.Substring(pp.IndexOf(':') + 1);
                                    double start, interval, end;
                                    start = Convert.ToDouble(str1) * coef;
                                    end = Convert.ToDouble(str2.Substring(str2.IndexOf(':') + 1)) * coef;
                                    interval = Convert.ToDouble(str2.Substring(0, str2.IndexOf(':'))) * coef;
                                    for (int j = 0; j <= Convert.ToInt32(end / interval); j++)
                                    {
                                        parameters[i].Add(Math.Round(start + i * interval, 6));
                                    }
                                }
                            }
                        }
                        foreach (double p1 in parameters[0])
                        {
                            foreach (double p2 in parameters[1])
                            {
                                foreach (double p3 in parameters[2])
                                {
                                    //progressBar.Value = 0;
                                    sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - (" + p1 + ", " + p2 + ", " + p3 + ")" + par + ".txt", false, Encoding.Unicode); // запускаем запись в файл для соответствующих значений параметров
                                    stopwatch.Start(); // запускаем таймер
                                    triclusterSet = Triclusterer.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, new List<string>(), p1, p2, p3); // запускаем трикластеризацию (1 аргумент - метод, 2 - паралельная ли версия нужна, 3 - список условий (нужно для DataPeeler'а, 4+ - значения параметров))
                                    stopwatch.Stop(); // останавливаем таймер
                                    Triclusterer.CalculateDensities(triclusterSet); // вычисляем плотности трикластеров
                                    //totalCoverage = Triclusterer.TotalCoverage(triclusterSet);
                                    swLog.WriteLine("<" + p1 + ", " + p2 + ", " + p3 + ">" + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + Math.Round(Triclusterer.TotalCoverage(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.CoverageIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.TotalDiversity(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.DiversityIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityModus(triclusterSet) * 100, 3));
                                    if (specFlag) // если нужно посчитать сходства для тестов на шумоустойчивость - запускаем соответствующие процедуры
                                    {
                                        best = Triclusterer.TestDiagContSimilarity(triclusterSet);
                                        swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - (" + p1 + ", " + p2 + ", " + p3 + ")" + par + " - sim.txt", false, Encoding.Unicode);
                                        foreach (KeyValuePair<Tricluster, double> t in best)
                                        {
                                            swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrShort());
                                        }
                                        swTemp.Close();
                                    }
                                    Triclusterer.DecypherNames(triclusterSet); // заменяем имена объектов, признаков и условий на настоящие (вообще говоря, не нужно делать такие преобрахования, из-за того, что C# по умолчанию работает со ссылками)
                                    writer(sw, triclusterSet, false); // записываем множество трикластеров в файл
                                    stopwatch.Reset(); // сбрасываем таймер
                                    swLog.Flush(); // явно записываем текущие результаты в лог
                                }
                            }
                        }
                        break;
                    case "TBox":
                        sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + par + ".txt", false, Encoding.Unicode);
                        stopwatch.Start();
                        triclusterSet = Triclusterer.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, new List<string>(), 0);
                        stopwatch.Stop();
                        Triclusterer.CalculateDensities(triclusterSet);
                        //totalCoverage = Triclusterer.TotalCoverage(triclusterSet);
                        swLog.WriteLine("-" + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + Math.Round(Triclusterer.TotalCoverage(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.CoverageIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.TotalDiversity(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.DiversityIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityModus(triclusterSet) * 100, 3));
                        if (specFlag)
                        {
                            best = Triclusterer.TestDiagContSimilarity(triclusterSet);
                            swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + par + " - sim.txt", false, Encoding.Unicode);
                            foreach (KeyValuePair<Tricluster, double> t in best)
                            {
                                swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrShort());
                            }
                            swTemp.Close();
                        }
                        Triclusterer.DecypherNames(triclusterSet);
                        writer(sw, triclusterSet, false);
                        stopwatch.Reset();
                        swLog.Flush();
                        break;
                    case "DataPeeler": // реализация DataPeeler'а в трёхмерном случае (пока не очень хорошо тестировалась, возможны ошибки, а также, вероятно, можно улучшить производительность)
                        sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + par + ".txt", false, Encoding.Unicode);

                        List<string> cons = new List<string>(); // читаем условия условия
                        for (int i = 0; i < constraints.Count; i++)
                        {
                            cons.Add(constraints[i].Constraint);
                        }

                        stopwatch.Start();
                        triclusterSet = Triclusterer.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, cons, 0);
                        stopwatch.Stop();
                        Triclusterer.CalculateDensities(triclusterSet);
                        //totalCoverage = Triclusterer.TotalCoverage(triclusterSet);
                        swLog.WriteLine("-" + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + Math.Round(Triclusterer.TotalCoverage(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.CoverageIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.TotalDiversity(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.DiversityIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityModus(triclusterSet) * 100, 3));
                        if (specFlag)
                        {
                            best = Triclusterer.TestDiagContSimilarity(triclusterSet);
                            swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + par + " - sim.txt", false, Encoding.Unicode);
                            foreach (KeyValuePair<Tricluster, double> t in best)
                            {
                                swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrShort());
                            }
                            swTemp.Close();
                        }
                        Triclusterer.DecypherNames(triclusterSet);
                        writer(sw, triclusterSet, false);
                        stopwatch.Reset();
                        swLog.Flush();
                        break;
                    case "NOAC":
                        MD5 md5Hash = MD5.Create(); // хеш-функция (на данный момент не используется, ибо можно проще)
                        NumericalTriclusteringMaster Triclusterer1 = new NumericalTriclusteringMaster(md5Hash);
                        
                        if (!Triclusterer1.IsLoaded) // загружает контекст, если не загружен и/или изменился
                        {
                            if (ckbFirstTriples.Checked)
                            {
                                Triclusterer1.LoadContext(ContextLoader.LoadNumericalContext(tbContext.Text, Convert.ToInt32(tbFirstTriples.Text)));
                            }
                            else
                            {
                                Triclusterer1.LoadContext(ContextLoader.LoadNumericalContext(tbContext.Text));
                            }
                        }

                        string[] pars = tbOptions.Text.Split(';');
                        foreach (string p in pars)
                        {
                            // Данные формата ввода параметров
                            NumberFormatInfo provider = new NumberFormatInfo();
                            provider.NumberDecimalSeparator = ".";
                            
                            string[] split = p.Split(',');
                            double delta = Convert.ToDouble(split[0],provider);
                            double opt1 = 0, opt2 = 0;
                            if (split.Length > 1)
                            {
                                opt1 = Convert.ToDouble(split[1],provider);
                            }
                            if (split.Length > 2)
                            {
                                opt2 = Convert.ToDouble(split[2],provider);
                            }

                          //  if (!opt1.Contains(':')) // если задано перечисление параметров
                            {
                                sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + "_" + contextFileName + " - " + p + par + "_" + limit + ".txt", false, Encoding.Unicode);
                                stopwatch.Start();
                                triclusterSet = Triclusterer1.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, new List<string>(), delta, opt1, opt2);
                                stopwatch.Stop();

                                double avgVariance = Triclusterer1.CalculateNumericalParameters(triclusterSet);
                                double avgDensity = Triclusterer1.CalculateDensities(triclusterSet);
                                //if (opt1 == 0) // если не было условия на минимальную плотность, плотности нужно вычислить
                                //{
                                //    Triclusterer1.CalculateDensities(triclusterSet);
                                //}

                                Triclusterer1.removeBadTriclusters(triclusterSet, delta);

                                if (ckbInjections.Checked)
                                {
                                    Triclusterer1.removeInjections(triclusterSet, Convert.ToDouble(tbDifference.Text,provider));
                                }

                                //totalCoverage = Triclusterer1.TotalCoverage(triclusterSet);
                                swLog.WriteLine(p + "\t" + limit + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + avgVariance + "\t" + avgDensity + "\t" + Math.Round(Triclusterer1.TotalCoverage(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer1.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer1.CoverageIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer1.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer1.TotalDiversity(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer1.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer1.DiversityIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer1.DiversityModus(triclusterSet) * 100, 3));
                                if (specFlag)
                                {
                                    best = Triclusterer1.TestDiagContSimilarity(triclusterSet);
                                    swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + p + par + " - sim.txt", false, Encoding.Unicode);
                                    foreach (KeyValuePair<Tricluster, double> t in best)
                                    {
                                        swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrNumerical());
                                    }
                                    swTemp.Close();
                                }
                                Triclusterer1.DecypherNames(triclusterSet);
                                writer_numerical(sw, triclusterSet);
                                stopwatch.Reset();
                                swLog.Flush();
                            }
                            //else // если задан интервал значения параметров
                            //{
                            //    string str1 = p.Substring(0, p.IndexOf(':'));
                            //    string str2 = p.Substring(p.IndexOf(':') + 1);
                            //    double start, interval, end;
                            //    start = Convert.ToDouble(str1);
                            //    end = Convert.ToDouble(str2.Substring(str2.IndexOf(':') + 1));
                            //    interval = Convert.ToDouble(str2.Substring(0, str2.IndexOf(':')));
                            //    for (int i = 0; i <= Convert.ToInt32(end / interval); i++) // если задать "for(double p = start; p <= end; p += interval)" будет быстро копиться погрешность
                            //    {
                            //        sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + (Math.Round(start + i * interval, 6)).ToString() + par + ".txt", false, Encoding.Unicode);
                            //        stopwatch.Start();
                            //        triclusterSet = Triclusterer1.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, new List<string>(), Math.Round(start + i * interval, 6));
                            //        stopwatch.Stop();
                            //        if (Math.Round(start + i * interval, 6) == 0)
                            //        {
                            //            Triclusterer1.CalculateDensities(triclusterSet);
                            //        }
                            //        //totalCoverage = Triclusterer1.TotalCoverage(triclusterSet);
                            //        swLog.WriteLine(Math.Round(start + i * interval, 6) + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + Math.Round(Triclusterer1.TotalCoverage(triclusterSet) * 100, 3) +
                            //            "\t" + Math.Round(Triclusterer1.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer1.CoverageIntent(triclusterSet) * 100, 3) +
                            //            "\t" + Math.Round(Triclusterer1.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer1.TotalDiversity(triclusterSet) * 100, 3) +
                            //            "\t" + Math.Round(Triclusterer1.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer1.DiversityIntent(triclusterSet) * 100, 3) +
                            //            "\t" + Math.Round(Triclusterer1.DiversityModus(triclusterSet) * 100, 3));
                            //        if (specFlag)
                            //        {
                            //            best = Triclusterer1.TestDiagContSimilarity(triclusterSet);
                            //            swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + (Math.Round(start + i * interval, 6)).ToString() + par + " - sim.txt", false, Encoding.Unicode);
                            //            foreach (KeyValuePair<Tricluster, double> t in best)
                            //            {
                            //                swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrShort());
                            //            }
                            //            swTemp.Close();
                            //        }
                            //        Triclusterer1.DecypherNames(triclusterSet);
                            //        writer_numerical(sw, triclusterSet);
                            //        stopwatch.Reset();
                            //        swLog.Flush();
                            //    }
                            //}
                        }
                        break;

                    case "KMeans":
                        NumericalTriadicContext context;
                        if (ckbFirstTriples.Checked)
                        {
                            context = ContextLoader.LoadNumericalContext(tbContext.Text, Convert.ToInt32(tbFirstTriples.Text));
                        }
                        else
                        {
                            context = ContextLoader.LoadNumericalContext(tbContext.Text);
                        }

                        string[] prms = tbOptions.Text.Split(';');
                        foreach (string p in prms)
                        {
                            // Get options k and gamma
                            string[] split = p.Split(',');
                            int k = Convert.ToInt32(split[0]);
                            double gamma = Convert.ToDouble(split[1]);
                            int iterMax = 0, initStyle = 0;
                            if (split.Length > 2)
                            {
                                initStyle = Convert.ToInt32(split[2]);
                            }
                            if (split.Length > 3)
                            {
                                iterMax = Convert.ToInt32(split[3]);
                            }
                           
                            KMeansMaster KMaster = new KMeansMaster(k, gamma);
                            KMaster.LoadContext(context);
                            int iterCount;

                            stopwatch.Start();
                            triclusterSet = KMaster.Triclustering(out iterCount, initStyle, iterMax);
                            stopwatch.Stop();

                            sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + "_" + contextFileName + " - " + p + par + ".txt", false, Encoding.Unicode);

                            double avgVariance = context.CalculateNumericalParameters(triclusterSet);
                            double avgDensity = context.CalculateDensities(triclusterSet);

                            swLog.WriteLine(p + "\t" + limit + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + avgVariance + "\t" + avgDensity + "\t" + Math.Round(context.TotalCoverage(triclusterSet) * 100, 3) +
                                    "\t" + Math.Round(context.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(context.CoverageIntent(triclusterSet) * 100, 3) +
                                    "\t" + Math.Round(context.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(context.TotalDiversity(triclusterSet) * 100, 3) +
                                    "\t" + Math.Round(context.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(context.DiversityIntent(triclusterSet) * 100, 3) +
                                    "\t" + Math.Round(context.DiversityModus(triclusterSet) * 100, 3));
                            swLog.WriteLine("Finished in " + iterCount + "steps.");

                            // output triclusters
                            context.DecypherNames(triclusterSet);
                            writer_numerical(sw, triclusterSet);
                            stopwatch.Reset();
                            swLog.Flush();
                        }

                        break;

                    default: // OAC(box) и OAC(prime)
                        string[] param = tbOptions.Text.Split(';');
                        foreach (string p in param)
                        {
                            if (!p.Contains(':')) // если задано перечисление параметров
                            {
                                sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + p + par + ".txt", false, Encoding.Unicode);
                                stopwatch.Start();
                                triclusterSet = Triclusterer.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, new List<string>(), Convert.ToDouble(p));
                                stopwatch.Stop();
                                if (cbMethod.SelectedItem.ToString() == "Spectral")
                                {
                                    Triclusterer.CalculateDensities(triclusterSet);
                                }
                                else if (Convert.ToDouble(p) == 0) // если не было условия на минимальную плотность, плотности нужно вычислить
                                {
                                    if (cbMethod.SelectedItem.ToString() == "OAC (box)")
                                    {
                                        Triclusterer.CalculateDensities(triclusterSet);
                                    }
                                    else if (cbMethod.SelectedItem.ToString() == "OAC (prime)")
                                    {
                                        Triclusterer.CalculateDensitiesPrime(triclusterSet); // в этом случае будут также посчитаны оценки плотности
                                    }
                                }

                                if (ckbInjections.Checked)
                                {
                                    Triclusterer.removeInjections(triclusterSet, Convert.ToDouble(tbDifference.Text));
                                }

                                //totalCoverage = Triclusterer.TotalCoverage(triclusterSet);
                                swLog.WriteLine(p + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + Math.Round(Triclusterer.TotalCoverage(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.CoverageIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.TotalDiversity(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.DiversityIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityModus(triclusterSet) * 100, 3));
                                if (specFlag)
                                {
                                    best = Triclusterer.TestDiagContSimilarity(triclusterSet);
                                    swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + p + par + " - sim.txt", false, Encoding.Unicode);
                                    foreach (KeyValuePair<Tricluster, double> t in best)
                                    {
                                        swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrShort());
                                    }
                                    swTemp.Close();
                                }
                                Triclusterer.DecypherNames(triclusterSet);
                                writer(sw, triclusterSet, (cbMethod.SelectedItem.ToString() == "OAC (prime)"));
                                stopwatch.Reset();
                                swLog.Flush();
                            }
                            else // если задан интервал значения параметров
                            {
                                string str1 = p.Substring(0, p.IndexOf(':'));
                                string str2 = p.Substring(p.IndexOf(':') + 1);
                                double start, interval, end;
                                start = Convert.ToDouble(str1);
                                end = Convert.ToDouble(str2.Substring(str2.IndexOf(':') + 1));
                                interval = Convert.ToDouble(str2.Substring(0, str2.IndexOf(':')));
                                for (int i = 0; i <= Convert.ToInt32(end / interval); i++) // если задать "for(double p = start; p <= end; p += interval)" будет быстро копиться погрешность
                                {
                                    sw = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + (Math.Round(start + i * interval, 6)).ToString() + par + ".txt", false, Encoding.Unicode);
                                    stopwatch.Start();
                                    triclusterSet = Triclusterer.Triclustering(cbMethod.SelectedItem.ToString(), ckbParallel.Checked, new List<string>(), Math.Round(start + i * interval, 6));
                                    stopwatch.Stop();
                                    if (cbMethod.SelectedItem.ToString() == "Spectral")
                                    {
                                        Triclusterer.CalculateDensities(triclusterSet);
                                    }
                                    else if (Math.Round(start + i * interval, 6) == 0)
                                    {
                                        if (cbMethod.SelectedItem.ToString() == "OAC (box)")
                                        {
                                            Triclusterer.CalculateDensities(triclusterSet);
                                        }
                                        else if (cbMethod.SelectedItem.ToString() == "OAC (prime)")
                                        {
                                            Triclusterer.CalculateDensitiesPrime(triclusterSet);
                                        }
                                    }
                                    //totalCoverage = Triclusterer.TotalCoverage(triclusterSet);
                                    swLog.WriteLine(Math.Round(start + i * interval, 6) + "\t" + stopwatch.ElapsedMilliseconds + "\t" + triclusterSet.Count + "\t" + Math.Round(Triclusterer.TotalCoverage(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.CoverageIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.CoverageModus(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.TotalDiversity(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityExtent(triclusterSet) * 100, 3) + "\t" + Math.Round(Triclusterer.DiversityIntent(triclusterSet) * 100, 3) +
                                        "\t" + Math.Round(Triclusterer.DiversityModus(triclusterSet) * 100, 3));
                                    if (specFlag)
                                    {
                                        best = Triclusterer.TestDiagContSimilarity(triclusterSet);
                                        swTemp = new StreamWriter(tbOutput.Text + cbMethod.SelectedItem.ToString() + " - " + (Math.Round(start + i * interval, 6)).ToString() + par + " - sim.txt", false, Encoding.Unicode);
                                        foreach (KeyValuePair<Tricluster, double> t in best)
                                        {
                                            swTemp.WriteLine(t.Value.ToString() + "\t" + t.Key.DescrShort());
                                        }
                                        swTemp.Close();
                                    }
                                    Triclusterer.DecypherNames(triclusterSet);
                                    writer(sw, triclusterSet, (cbMethod.SelectedItem.ToString() == "OAC (prime)"));
                                    stopwatch.Reset();
                                    swLog.Flush();
                                }
                            }
                        }
                        break;
                }
                swLog.Close();
            }
            catch (Exception exception)
            {
                Triclusterer.IsLoaded = false;
                MessageBox.Show(exception.Message);
            }
            finally
            {
                if (swLog != null)
                    swLog.Close();
            }
            MessageBox.Show("Done", "Success", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Asterisk);
        }

        private void tbContext_TextChanged(object sender, EventArgs e) // Flags context as not loaded, for the file could have changed.
        {
            Triclusterer.IsLoaded = false;
        }

        private void writer(StreamWriter sw, List<Tricluster> triclusterSet, bool isPrime) // записывает множество трикластеров в файл
        {
            sw.WriteLine(triclusterSet.Count);
            if (isPrime)
            {
                sw.WriteLine("Density\tEstimate 1\tEstimate 2\tEstimate 3\tCoverage\tObjects coverage\tAttributes coverage\tConditions coverage\tExtent\tIntent\tModus");
            }
            else
            {
                sw.WriteLine("Density\tCoverage\tObjects coverage\tAttributes coverage\tConditions coverage\tExtent\tIntent\tModus");
            }
            foreach (Tricluster t in triclusterSet)
            {
                sw.WriteLine(t.DescrShort());
            }
            sw.Close();
        }

        private void writer_numerical(StreamWriter sw, List<Tricluster> triclusterSet) // записывает множество трикластеров в файл
        {
            sw.WriteLine(triclusterSet.Count);
            sw.WriteLine("Density\tVariance\tAverage\tCoverage\tObjects coverage\tAttributes coverage\tConditions coverage\tExtent\tIntent\tModus");
            foreach (Tricluster t in triclusterSet)
            {
                sw.WriteLine(t.DescrNumerical());
            }
            sw.Close();
        }

        private void tbOutput_TextChanged(object sender, EventArgs e) // меняет путь к папке в диалоге, если пользователь явно изменил путь
        {
            folderBrowserDialog.SelectedPath = tbOutput.Text;
        }

        private void cbMethod_SelectedIndexChanged(object sender, EventArgs e) // в зависимости от метода "блокирует" некоторые элементы
        {
            if (cbMethod.SelectedItem.ToString() == "Spectral" || cbMethod.SelectedItem.ToString() == "Trias")
            {
                prevCheckParallel = ckbParallel.Checked;
                ckbParallel.Checked = false;
                ckbParallel.Enabled = false;
                prevForbParallel = true;
            }
            else
            {
                if (prevForbParallel)
                {
                    ckbParallel.Checked = prevCheckParallel;
                    ckbParallel.Enabled = true;
                    prevForbParallel = false;
                }
            }

            if (cbMethod.SelectedItem.ToString() == "KMeans")
            {
                tbOptions.Text = "2, 0";
            }

            if (cbMethod.SelectedItem.ToString() == "OAC (prime)" || cbMethod.SelectedItem.ToString() == "OAC (box)" || cbMethod.SelectedItem.ToString() == "NOAC" || cbMethod.SelectedItem.ToString() == "KMeans")
            {
                if (prevForbInjections)
                {
                    ckbInjections.Checked = prevCheckInjections;
                    ckbInjections.Enabled = true;
                    //tbDifference.Enabled = true;
                    //label6.Enabled = true;
                    prevForbInjections = false;
                }
            }
            else
            {
                prevCheckInjections = ckbInjections.Checked;
                ckbInjections.Checked = false;
                ckbInjections.Enabled = false;
                //tbDifference.Enabled = false;
                //label6.Enabled = false;
                prevForbInjections = true;
            }

            if (cbMethod.SelectedItem.ToString() == "Trias" && tbOptions.Text != "") // для триаса автоматически множит содержания окна параметров на три одинаковые части. Обратное преобразование только если все три условия одинаковы
            {
                tbOptions.Text = "*" + tbOptions.Text + "|*" + tbOptions.Text + "|*" + tbOptions.Text;
            }
            else if (tbOptions.Text.Contains('|'))
            {
                string[] temp = tbOptions.Text.Split('|');
                if (temp.Count() == 3 && temp[0] == temp[1] && temp[1] == temp[2])
                {
                    tbOptions.Text = temp[0];
                }
            }
            tbOptions.Text = tbOptions.Text.Replace("*", "");
        }

        private void ckbFirstTriples_CheckedChanged(object sender, EventArgs e)
        {
            tbFirstTriples.Enabled = ckbFirstTriples.Checked;
            lblFirstTriples.Enabled = ckbFirstTriples.Checked;
            Triclusterer.IsLoaded = false;
        }

        private void tbFirstTriples_TextChanged(object sender, EventArgs e)
        {
            Triclusterer.IsLoaded = false;
        }

        private void ckbInjections_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbInjections.Checked)
            {
                tbDifference.Enabled = true;
                lblDiff.Enabled = true;
            }
            else
            {
                tbDifference.Enabled = false;
                lblDiff.Enabled = false;
            }
        }
    }
}
