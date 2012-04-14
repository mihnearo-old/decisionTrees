namespace mlLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class implements a container for the the data instance.
    /// </summary>
    class Instance
    {
        public Dictionary<string, string> Data
        {
            get;
            set;
        }

        /// <summary>
        /// Builds a string representation of the object.
        /// </summary>
        /// <returns>string representation of object</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string attribute in this.Data.Keys.OrderBy(k => k))
            {
                if (0 != builder.Length)
                {
                    builder.Append(",");
                }

                builder.Append("'");
                builder.Append(attribute);
                builder.Append("'='");
                builder.Append(this.Data[attribute]);
                builder.Append("'");
            }

            return builder.ToString();
        }
    }
}
