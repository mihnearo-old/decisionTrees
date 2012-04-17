namespace mlLib.Learner
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using mlLib.Arff;
    using mlLib.DecisionTree;


    /// <summary>
    /// Class implements a ID3 decision tree learner
    /// </summary>
    public class ID3Learner
    {
        #region Member Variables

        private double splitStoppingConfidenceLevel = 0.0f;
        private bool handleUnknownAsValue = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Intializes an instance of the ID3Learner class.
        /// </summary>
        /// <param name="splitStoppingConfidenceLevel">confidence leve for split stopping test</param>
        /// <param name="handleUnknownAsValue">flag specifying how unknown attribute values should be handled</param>
        public ID3Learner(double splitStoppingConfidenceLevel, bool handleUnknownAsValue)
        {
            this.splitStoppingConfidenceLevel = splitStoppingConfidenceLevel;
            this.handleUnknownAsValue = handleUnknownAsValue;
        }

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

            // adjust the attribute list
            var attributeList = data.Attributes.Where(a => !a.Name.Equals(data.ClassAttribute.Name));
            if (this.handleUnknownAsValue)
            {
                attributeList = attributeList.Select(a => 
                    {
                        List<string> values = a.Values.ToList();
                        values.Add(Instances.UnknownValue);

                        return new Arff.Attribute()
                        {
                            Name = a.Name,
                            Values = values.ToArray()
                        };
                    });
            }

            return Learn(
                data.Examples,
                attributeList.ToArray(), 
                data.ClassAttribute);
        }

        #endregion

        #region Private Methods

        private Node Learn(Instance[] instanceList, Arff.Attribute[] attributeList, Arff.Attribute classAttribute)
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

            var decisionAttribute = GetDecisionAttribute(instanceList, attributeList, classDistribution, classAttribute);
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
            foreach (string value in decisionAttribute.Values)
            {
                Node childNode = null;

                if (!instanceGroups.ContainsKey(value)
                    || 0 == instanceGroups[value].Length)
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
                    instanceGroups[value], 
                    attributeList.Where(a => !a.Name.Equals(decisionAttribute.Name)).ToArray(), 
                    classAttribute);
                root.Children.Add(value, childNode);
            }

            return root;
        }

        private Arff.Attribute GetDecisionAttribute(Instance[] instanceList, Arff.Attribute[] attributeList, Dictionary<string, int> classDistribution, Arff.Attribute classAttribute)
        {
            Arff.Attribute decisionAttribute = null;
            double decisionAttributeEntropy = double.MaxValue;

            // compute entropy of each attribute
            foreach (Arff.Attribute attribute in attributeList)
            {
                // group the instances by their values for the attribute being evaluated
                var groupedInstanceList = instanceList.GroupBy(i => i.Data[attribute.Name]);
                if (!this.handleUnknownAsValue)
                {
                    groupedInstanceList = groupedInstanceList.Where(g => !g.Key.Equals(Instances.UnknownValue));
                }

                // compute the chi-squared statistic for the data
                double dataChiSquared = ComputeAttributeChiSquared(groupedInstanceList, instanceList.Length, classDistribution, classAttribute);
                double criticalChiSquared = ChiSquare.CriticalChiSquareValue(1.0f - this.splitStoppingConfidenceLevel, attribute.Values.Length - 1);
                //System.Console.Out.WriteLine("Chi-Square test for [{0}]: data=[{1}] critical=[{2}]", attribute.Name, dataChiSquared, criticalChiSquared);
                if (dataChiSquared < criticalChiSquared)
                {
                    // attribute did not pass chi-square split test
                    continue;
                }

                double attributeEntropy = ComputeAttributeEntropy(groupedInstanceList, instanceList.Length, classAttribute);
                if (decisionAttributeEntropy > attributeEntropy)
                {
                    // found a better attribute
                    decisionAttribute = attribute;
                    decisionAttributeEntropy = attributeEntropy;
                }
            }

            if (null != decisionAttribute)
            {
                System.Console.Out.WriteLine("Selected Attribute - name=[{0}] entropy=[{1}].", decisionAttribute.Name, decisionAttributeEntropy);
            }
            else
            {
                System.Console.Out.WriteLine("No relevant attribute found.");
            }

            return decisionAttribute;
        }

        private double ComputeAttributeChiSquared(IEnumerable<IGrouping<string, Instance>> groupedInstanceList, int totlaInstanceCount, Dictionary<string, int> classDistribution, Arff.Attribute classAttribute)
        {
            double attributeDeviation = 0.0f;
            foreach (var group in groupedInstanceList)
            {
                var attributeValueClassDistribution = GetClassDistribution(group.ToArray(), classAttribute);
                if (null == attributeValueClassDistribution)
                {
                    throw new Exception("Unexpected exception.");
                }

                foreach (var classValue in classDistribution)
                {
                    // compute the expected probability for the class value
                    double expectedInstances = (double)classValue.Value / (double)totlaInstanceCount * group.Count();

                    // get the actual instances
                    double actualInstances = (double)attributeValueClassDistribution[classValue.Key];

                    // adjust result value
                    attributeDeviation +=
                        Math.Pow(actualInstances - expectedInstances, 2.0f) / expectedInstances;

                    //System.Console.Out.WriteLine("Chi-Square for [{0}]-[{1}]: act_int[{2}] exp_inst=[{3}] dev=[{4}]", group.Key, classValue.Key, actualInstances, expectedInstances, attributeDeviation);
                }
            }

            return attributeDeviation;
        }

        private double ComputeAttributeEntropy(IEnumerable<IGrouping<string, Instance>> groupedInstanceList, int totlaInstanceCount, Arff.Attribute classAttribute)
        {
            double attributeEntropy = 0.0f;
            foreach (var instanceGroup in groupedInstanceList)
            {
                // get the class distribution for the value
                Dictionary<string, int> classDistribution = GetClassDistribution(instanceGroup.ToArray(), classAttribute);

                // get the entropy of the value
                double valueEntropy = ComputeEntropy(instanceGroup.Count(), classDistribution);

                // compute the attribute entropy
                attributeEntropy += (double)instanceGroup.Count() / (double)totlaInstanceCount * valueEntropy;
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

        private static Dictionary<string, int> GetClassDistribution(Instance[] instanceList, Arff.Attribute classAttribute)
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
