namespace mlLib.Learners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using mlLib;
    using mlLib.DecisionTree;


    /// <summary>
    /// Class implements a ID3 decision tree learner
    /// </summary>
    public class ID3Learner
    {
        #region Public Methods

        /// <summary>
        /// Learns a decision tree model for the data passed as arguments
        /// </summary>
        /// <param name="data">training data</param>
        /// <returns>decision tree</returns>
        public Node Learn(Instances data)
        {
            if (null == data
                || null == data.Examples
                || null == data.Attributes
                || null == data.ClassAttribute)
            {
                throw new ArgumentException("data");
            }

            return Learn(
                data.Examples, 
                data.Attributes.Where(a => !a.Name.Equals(data.ClassAttribute.Name)).ToArray(), 
                data.ClassAttribute);
        }

        #endregion

        #region Private Methods

        private Node Learn(Instance[] instanceList, mlLib.Attribute[] attributeList, mlLib.Attribute classAttribute)
        {
            // compute the class distribution of all the examples
            Dictionary<string, int> classDistribution = GetClassDistribution(instanceList, classAttribute);

            string[] classesWithExamples = classDistribution.Where(a => a.Value > 0).Select(a => a.Key).ToArray();
            if (1 == classesWithExamples.Length)
            {
                // all examples belong to the same class so we have reached a leaf node
                return new Node()
                {
                    Label = classesWithExamples[0],
                    Children = null
                };
            }

            string mostCommonClass = classDistribution.OrderBy(a => a.Value).First().Key;
            if (null == attributeList
                || 0 == attributeList.Length)
            {
                // no more attributes to split on
                return new Node()
                {
                    Label = mostCommonClass,
                    Children = null
                };
            }

            mlLib.Attribute decisionAttribute = GetDecisionAttribute(attributeList);
            if (null == decisionAttribute)
            {
                throw new Exception("Unexpected error.");
            }

            // recursively build the tree
            Node root = new Node()
            {
                Label = decisionAttribute.Name,
                Children = new Dictionary<string, Node>()
            };

            foreach (string value in decisionAttribute.Values)
            {
                Node childNode = null;

                Instance[] valueInstances = instanceList.Where(a => a.Data[decisionAttribute.Name].Equals(value)).ToArray();
                if (0 == valueInstances.Length)
                {
                    // if there are not example for the node value assign the 
                    // label of most instances to the value branch
                    childNode = new Node()
                    {
                        Label = mostCommonClass,
                        Children = null
                    };
                    root.Children.Add(value, childNode);

                    continue;
                }

                // build the subtree recursively
                childNode = this.Learn(
                    valueInstances, 
                    attributeList.Where(a => !a.Name.Equals(decisionAttribute.Name)).ToArray(), 
                    classAttribute);
                root.Children.Add(value, childNode);
            }

            return root;
        }

        private mlLib.Attribute GetDecisionAttribute(mlLib.Attribute[] attributeList)
        {
            return attributeList[0];
        }

        private static Dictionary<string, int> GetClassDistribution(Instance[] instanceList, mlLib.Attribute classAttribute)
        {
            // lets check some termination conditions
            Dictionary<string, int> classDistribution = new Dictionary<string, int>();
            foreach (string value in classAttribute.Values)
            {
                classDistribution.Add(value, 0);
            }

            foreach (Instance example in instanceList)
            {
                classDistribution[example.Data[classAttribute.Name]]++;
            }

            return classDistribution;
        }

        #endregion
    }
}
