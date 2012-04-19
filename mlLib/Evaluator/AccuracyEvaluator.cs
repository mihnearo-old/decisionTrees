namespace mlLib.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using mlLib.Arff;
    using mlLib.Classifier;
    using mlLib.Log;

    /// <summary>
    /// Class implements an evaluator that measures the accuracy of a classifier
    /// </summary>
    public class AccuracyEvaluator
    {
        #region Private Members

        private DTClassifier classifier = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes a new instance of the AccuracyEvaluator class.
        /// </summary>
        /// <param name="classifier">classifier to evaluate</param>
        public AccuracyEvaluator(DTClassifier classifier)
        {
            this.classifier = classifier;
        }

        /// <summary>
        /// Computes the accuracy of the classifier on the data set passed as a parameter.
        /// </summary>
        /// <param name="data">test data to evaluate classifier on</param>
        /// <returns>number between 0.0 and 1.0 representing the accuracy</returns>
        public double Evaluate(Instances data)
        {
            if (null == data
                || null == data.ClassAttribute)
            {
                throw new ArgumentNullException("data");
            }

            Logger.Log(LogLevel.Progress, "Running Accuracy Evaluator ");

            int correct = 0;
            foreach (Instance example in data.Examples)
            {
                string predictedClass = this.classifier.Classify(example);
                string actualClass = example.Data[data.ClassAttribute.Name];
                if (predictedClass == actualClass)
                {
                    correct++;
                }

                if (DateTime.Now.Second % 10 == 0)
                {
                    Logger.Log(LogLevel.Progress, ".");
                }
            }

            Logger.Log(LogLevel.Progress, " Done{0}", System.Console.Out.NewLine);
            return (double)correct / (double)data.Examples.Length;
        }

        #endregion
    }
}
