namespace mlApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using mlLib.Arff;
    using mlLib.Classifier;
    using mlLib.DecisionTree;
    using mlLib.Evaluator;
    using mlLib.Learner;
    using mlLib.Log;

    class Program
    {
        #region Private Members

        private DateTime startTime = DateTime.Now;
        private List<string> reportData = null;

        private string trainingDataFilePath = null;
        private string testDataFilePath = null;
        private List<double> confidenceLevelList = null;

        #endregion

        #region Public Methods

        static void Main(string[] args)
        {
            // setup logging
            Logger.OnLogLevel = (int)(LogLevel.Progress | LogLevel.Error | LogLevel.Warning | LogLevel.Info);
            using (Logger.LogWriter = new StreamWriter("log.txt"))
            {
                Program app = new Program();
                app.Run(args);
            }
        }


        public Program()
        {
            this.startTime = DateTime.Now;
            this.reportData = new List<string>();
        }

        #endregion


        #region Private Methods

        private TimeSpan Runtime
        {
            get
            {
                return DateTime.Now - this.startTime;
            }
        }

        private void Run(string[] args)
        {        
            // Parse the arguments
            if (!this.ParseArguments(args))
            {
                return;
            }

            // load the data file
            Instances data = LoadDataFile(this.trainingDataFilePath);
            Instances testData = data;
            if (!string.IsNullOrWhiteSpace(this.testDataFilePath))
            {
                testData = LoadDataFile(this.testDataFilePath);
            }

            Logger.Log(LogLevel.Progress, System.Console.Out.NewLine);

            Logger.OnLogLevel |= (int)LogLevel.Trace;
            foreach (double confLvl in this.confidenceLevelList)
            {
                using (Logger.TraceWriter = new StreamWriter(string.Format("classificationPaths_true_{0}.txt", confLvl.ToString("0.0000"))))
                {
                    TrainAndEvaluateClassifier(data, testData, confLvl, true);   
                }

                using (Logger.TraceWriter = new StreamWriter(string.Format("classificationPaths_false_{0}.txt", confLvl.ToString("0.0000"))))
                {
                    TrainAndEvaluateClassifier(data, testData, confLvl, false);
                }
            }
            Logger.OnLogLevel &= ~(int)LogLevel.Trace;

            Logger.Log(LogLevel.Progress, "{0}Execution Report:{0}{0}", System.Console.Out.NewLine);
            Logger.Log(LogLevel.Progress, "Split Stop. Conf.\tUseGainRatio\tAccuracy{0}", System.Console.Out.NewLine);
            Logger.Log(LogLevel.Progress, "-----------------\t------------\t--------{0}", System.Console.Out.NewLine);
            foreach (string line in this.reportData)
            {
                Logger.Log(LogLevel.Progress, line);
            }

            Logger.Log(LogLevel.Progress, System.Console.Out.NewLine);
            Logger.Log(LogLevel.Progress, "Runtime: {0}", this.Runtime.ToString("c"));
            Logger.Log(LogLevel.Progress, System.Console.Out.NewLine);
            Logger.Log(LogLevel.Progress, "Press any key to exit");
            System.Console.ReadKey();
        }

        private bool ParseArguments(string[] args)
        {
            if (3 != args.Length)
            {
                System.Console.Out.WriteLine("Usage:");
                System.Console.Out.WriteLine("  mlApp %training_data_file% %test_data_file% %confidence_levels%");
                System.Console.Out.WriteLine();
                System.Console.Out.WriteLine("  %confidence_levels% - coma(,) separated list of values from 0 to 1");

                return false;
            }

            this.trainingDataFilePath = args[0];
            this.testDataFilePath = args[1];
            this.confidenceLevelList = new List<double>();

            string[] confLvlList = args[2].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string confLvl in confLvlList)
            {
                this.confidenceLevelList.Add(
                    double.Parse(confLvl));
            }

            return true;
        }

        private void TrainAndEvaluateClassifier(Instances data, Instances testData, double splitStoppingConfidence, bool useGainRatio)
        {
            // learn the tree
            ID3Learner learner = new ID3Learner(splitStoppingConfidence, true, useGainRatio);
            Node decisionTree = learner.Learn(data);

            // output the tree
            File.WriteAllText(
                string.Format("{0}_DTID3_{1}_{2}.txt", Path.GetFileNameWithoutExtension(this.trainingDataFilePath), useGainRatio, splitStoppingConfidence.ToString("0.0000")), 
                decisionTree.ToString());

            // evaluate the classifier
            DTClassifier classifier = new DTClassifier(decisionTree);
            AccuracyEvaluator evaluator = new AccuracyEvaluator(classifier);
            double accuracy = evaluator.Evaluate(testData);

            this.reportData.Add(
                string.Format("{0}\t\t\t{1}\t\t{2}{3}", splitStoppingConfidence.ToString("0.0000"), useGainRatio, accuracy.ToString("0.0000"), System.Console.Out.NewLine));
        }

        private Instances LoadDataFile(string filePath)
        {
            Instances data = null;

            // validate the path
            if (string.IsNullOrWhiteSpace(filePath)
                || !File.Exists(filePath))
            {
                Logger.Log(LogLevel.Error, "Invalid data file path. filePath=[{0}]", filePath);
                throw new ArgumentException("filePath");
            }

            // read in the data from file
            using (StreamReader stream = new StreamReader(filePath))
            {
                Logger.Log(LogLevel.Progress, "Reading [{0}] :", Path.GetFileName(filePath));
                data = ArffReader.Read(stream, "Class");
                Logger.Log(LogLevel.Progress, " Done.{0}", System.Console.Out.NewLine);
            }

            return data;
        }

        #endregion
    }
}
