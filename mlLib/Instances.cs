namespace mlLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class that implements a container for the data instances.
    /// </summary>
    public class Instances
    {
        #region Constants

        /// <summary>
        /// String representing unknown attribute value.
        /// </summary>
        public const string UnknownValue = "?";

        #endregion

        #region Private Members

        private List<Attribute> attributeList;
        private List<Instance> instanceList;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes an instance of the Instances class.
        /// </summary>
        public Instances()
        {
            this.attributeList = new List<Attribute>();
            this.instanceList = new List<Instance>();
        }

        /// <summary>
        /// Adds an attribute definition to the container.
        /// </summary>
        /// <param name="name">name of the attribute</param>
        /// <param name="values">list of possible values for the attribute</param>
        public void AddAttribute(string name, List<string> values)
        {
            if (string.IsNullOrWhiteSpace(name)
                || null == values || 0 == values.Count)
            {
                throw new ArgumentNullException(
                    string.Format("name = [{0}], values = [{1}]", name, values));
            }

            // create the attribute object
            Attribute attribute = new Attribute()
            {
                Name = name,
                Values = values.ToArray()
            };

            // add the attribute object to the list
            this.attributeList.Add(attribute);
        }

        /// <summary>
        /// Adds a data instance to the container
        /// </summary>
        /// <param name="values">list of feature values for the instance</param>
        public void AddInstance(List<string> values)
        {
            if (null == values
                || this.attributeList.Count != values.Count)
            {
                throw new ArgumentNullException(
                    string.Format("values = [{0}]", values));
            }

            // create the instance object
            Instance item = new Instance()
            {
                Data = new Dictionary<string,string>()
            };

            for (int idx = 0; idx < values.Count; idx++)
            {
                if (!UnknownValue.Equals(values[idx])
                    && !this.attributeList[idx].Values.Contains(values[idx]))
                {
                    throw new FormatException(
                        string.Format("Invalid data file. attribut = [{0}], invalid value=[{1}]", 
                            this.attributeList[idx].Name, values[idx]));
                }

                item.Data.Add(this.attributeList[idx].Name, values[idx]);
            }

            // add the data instance to the list
            this.instanceList.Add(item);
        }

        /// <summary>
        /// Builds a string representation of the object.
        /// </summary>
        /// <returns>string representation of object</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            // write out attributes
            builder.Append("@attributes\n");
            foreach (Attribute attribute in this.attributeList)
            {
                builder.Append(attribute);
                builder.Append("\n");
            }

            // write out the data
            builder.Append("\n@data\n");
            foreach (Instance instance in this.instanceList)
            {
                builder.Append(instance);
                builder.Append("\n");
            }

            return builder.ToString();
        }

        #endregion
    }
}
