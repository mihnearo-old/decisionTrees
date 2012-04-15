namespace mlLib.DecisionTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class implements a container for a node element in a decision tree.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the value of the node.
        /// </summary>
        public string Label
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the collection of child nodes for the current node.
        /// </summary>
        public Dictionary<string, Node> Children
        {
            get;
            set;
        }

        #region Public Methods

        /// <summary>
        /// Builds a string representation of the object.
        /// </summary>
        /// <returns>string representation of object</returns>
        public override string ToString()
        {
            return this.ToString(string.Empty);
        }

        #endregion

        private string ToString(string indentation)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(this.Label);
            builder.Append("\n");

            if (null == this.Children)
            {
                return builder.ToString();
            }

            foreach (string value in this.Children.Keys)
            {
                builder.Append(indentation);
                builder.Append(" |{");
                builder.Append(value);
                builder.Append("} ");
                builder.Append(this.Children[value].ToString(indentation + "    "));
            }

            return builder.ToString();
        }

    }
}
