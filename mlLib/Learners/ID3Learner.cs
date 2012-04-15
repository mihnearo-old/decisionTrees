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
            var classDistribution = GetClassDistribution(instanceList, classAttribute);

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

            var decisionAttribute = GetDecisionAttribute(instanceList, attributeList, classAttribute);
            if (null == decisionAttribute)
            {
                // can't find a attribute to split on that passes the split-termination condition
                return new Node()
                {
                    Label = mostCommonClass,
                    Children = null
                };
            }

            // recursively build the tree
            var root = new Node()
            {
                Label = decisionAttribute.Name,
                Children = new Dictionary<string, Node>()
            };

            // pre-process the instances
            var instanceGroups = instanceList
                .GroupBy(a => a.Data[decisionAttribute.Name])
                .ToDictionary(g => g.Key, v => v.ToArray());

            // build the sub-trees
            // Note: by looping over the set of possible values for the attribute
            //       we will drop any instances with unknown values for the attribute
            foreach (string value in decisionAttribute.Values)
            {
                Node childNode = null;

                Instance[] valueInstances = instanceGroups[value];
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

        private mlLib.Attribute GetDecisionAttribute(Instance[] instanceList, mlLib.Attribute[] attributeList, mlLib.Attribute classAttribute)
        {
            mlLib.Attribute decisionAttribute = null;
            double decisionAttributeEntropy = double.MaxValue;

            // compute entropy of each attribute
            foreach (mlLib.Attribute attribute in attributeList)
            {
                double attributeEntropy = ComputeAttributeEntropy(instanceList, classAttribute, attribute);
                if (decisionAttributeEntropy > attributeEntropy)
                {
                    // found a better attribute
                    decisionAttribute = attribute;
                    decisionAttributeEntropy = attributeEntropy;
                }
            }

            return decisionAttribute;
        }

        private static double ComputeAttributeEntropy(Instance[] instanceList, mlLib.Attribute classAttribute, mlLib.Attribute attribute)
        {
            double attributeEntropy = 0.0f;

            var groupedInstances = instanceList.GroupBy(i => i.Data[attribute.Name]);
            foreach (var instanceGroup in groupedInstances)
            {
                // lets drop the group of instances with unknow values
                if (instanceGroup.Key.Equals(Instances.UnknownValue))
                {
                    continue;
                }

                // get the class distribution for the value
                Dictionary<string, int> classDistribution = GetClassDistribution(instanceGroup.ToArray(), classAttribute);

                // get the entropy of the value
                double valueEntropy = ComputeEntropy(instanceGroup.Count(), classDistribution);

                // compute the attribute entropy
                attributeEntropy += (double)instanceGroup.Count() / (double)instanceList.Count() * valueEntropy;
            }

            return attributeEntropy;
        }

        private static double ComputeEntropy(int totalExamples, Dictionary<string, int> classDistribution)
        {
            double result = 0;
            foreach (var distrib in classDistribution)
            {
                double p = 0.0f;
                if (0 != totalExamples)
                {
                    p = (double)distrib.Value / (double)totalExamples;
                }

                if (p.Equals(0.0f))
                {
                    continue;
                }

                result += (-1.0f) * p * Math.Log(p);
            }

            return result;
        }

        private static Dictionary<string, int> GetClassDistribution(Instance[] instanceList, mlLib.Attribute classAttribute)
        {
            // lets check some termination conditions
            var classDistribution = new Dictionary<string, int>();
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
