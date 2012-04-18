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

        private string trainingDataFilePath = null;
        private string testDataFilePath = null;

        #endregion

        #region Public Methods

        static void Main(string[] args)
        {
            // setup logging
            Logger.OnLogLevel = (int)(LogLevel.Progress | LogLevel.Error | LogLevel.Warning | LogLevel.Info);
            using (Logger.LogWriter = new StreamWriter("log.txt"))
            {
                using (Logger.TraceWriter = new StreamWriter("trace.txt"))
                {
                    Program app = new Program();
                    app.Run(args);
                }
            }
        }


        public Program()
        {
            this.startTime = DateTime.Now;
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
            TrainAndEvaluateClassifier(data, testData, 0.999, true);
            Logger.OnLogLevel &= ~(int)LogLevel.Trace;

            TrainAndEvaluateClassifier(data, testData, 0.999, false);
            TrainAndEvaluateClassifier(data, testData, 0.99, true);
            TrainAndEvaluateClassifier(data, testData, 0.99, false);
            TrainAndEvaluateClassifier(data, testData, 0.95, true);
            TrainAndEvaluateClassifier(data, testData, 0.95, false);
            TrainAndEvaluateClassifier(data, testData, 0.50, true);
            TrainAndEvaluateClassifier(data, testData, 0.50, false);
            //TrainAndEvaluateClassifier(data, testData, 0.00);

            Logger.Log(LogLevel.Progress, System.Console.Out.NewLine);
            Logger.Log(LogLevel.Progress, "Runtime: {0}", this.Runtime.ToString("c"));
            Logger.Log(LogLevel.Progress, System.Console.Out.NewLine);
            Logger.Log(LogLevel.Progress, "Press any key to exit");
            System.Console.Read();
        }

        private bool ParseArguments(string[] args)
        {
            if (1 > args.Length)
            {
                System.Console.Out.WriteLine("Usage:");
                System.Console.Out.WriteLine("mlApp %training_data_file% [%test_data_file%]");

                return false;
            }

            this.trainingDataFilePath = args[0];
            if (2 <= args.Length)
            {
                this.testDataFilePath = args[1];
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
                string.Format("{0}_DTID3_{1}.txt", Path.GetFileNameWithoutExtension(this.trainingDataFilePath), splitStoppingConfidence.ToString("0.00")), 
                decisionTree.ToString());

            // evaluate the classifier
            DTClassifier classifier = new DTClassifier(decisionTree);
            AccuracyEvaluator evaluator = new AccuracyEvaluator(classifier);
            double accuracy = evaluator.Evaluate(testData);
            System.Console.Out.WriteLine("Split Stop. Conf.: {0} Accuracy: {1} UseGainRatio: {2}", splitStoppingConfidence.ToString("0.000"), accuracy.ToString("0.0000"), useGainRatio);
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
