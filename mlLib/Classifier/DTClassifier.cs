namespace mlLib.Classifier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using mlLib.Arff;
    using mlLib.DecisionTree;
    using mlLib.Log;

    /// <summary>
    /// Class implements a decision tree based classifier.
    /// </summary>
    public class DTClassifier
    {
        #region Private Members

        /// <summary>
        /// The model used to classify instances.
        /// </summary>
        private Node model = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initalizes a new instance of the DTClassifier class.
        /// </summary>
        /// <param name="model">the trained model</param>
        public DTClassifier(Node model)
        {
            this.model = model;
        }

        /// <summary>
        /// Determines the class label for a data example.
        /// </summary>
        /// <param name="data">data example to classify</param>
        /// <returns>class label</returns>
        public string Classify(Instance data)
        {
            StringBuilder trace = new StringBuilder();

            Node current = this.model;
            while (null != current)
            {
                if (null == current.Children)
                {
                    // we have reaced a leaf node - label contains the 
                    // class name
                    trace.Append("\t");
                    trace.Append(current.Label);
                    Logger.Log(LogLevel.Trace, trace.ToString());

                    return current.Label;
                }

                if (!data.Data.ContainsKey(current.Label))
                {
                    throw new ArgumentException("Data sample is incompatible with the model!");
                }

                string attributeValue = data.Data[current.Label];
                if (!current.Children.ContainsKey(attributeValue))
                {
                    throw new ArgumentException("Data sample is incompativle with the model!");
                }

                // record trace
                trace.Append("(");
                trace.Append(current.Label);
                trace.Append("|");
                trace.Append(attributeValue);
                trace.Append(")");

                // pick the branch
                current = current.Children[attributeValue];
            }

            throw new Exception("Invalid model");
        }

        #endregion
    }
}
