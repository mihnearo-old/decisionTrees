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

    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            Instances data = null;
            Instances testData = null;
            
            // read in the data from file
            using (StreamReader stream = new StreamReader(args[0]))
            {
                data = ArffReader.Read(stream, "Class");   
            }

            testData = data;
            if (2 <= args.Length)
            {
                using (StreamReader stream = new StreamReader(args[1]))
                {
                    data = ArffReader.Read(stream, "Class");
                }
            }
            
            // learn the tree
            ID3Learner learner = new ID3Learner(0.99, true);
            Node decisionTree = learner.Learn(data);

            // output the tree
            System.Console.Out.WriteLine(decisionTree);

            // evaluate the classifier
            DTClassifier classifier = new DTClassifier(decisionTree);
            AccuracyEvaluator evaluator = new AccuracyEvaluator(classifier);
            
            double accuracy = evaluator.Evaluate(testData);
            System.Console.Out.WriteLine("Accuracy: {0}", accuracy);

            System.Console.Out.WriteLine("Runtime: {0}", (DateTime.Now - start).ToString("c"));
            System.Console.Out.WriteLine("Press any key to exit");
            System.Console.Read();
        }
    }
}
