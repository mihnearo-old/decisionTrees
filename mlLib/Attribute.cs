namespace mlLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class implements a container for the the attribute metadata.
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// Gets or sets the name of the attribute.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of possible values for the attribute.
        /// </summary>
        public string[] Values
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

            // write the name
            builder.Append("'");
            builder.Append(this.Name);
            builder.Append("'");

            // write the values
            bool isFirst = true;
            builder.Append(" {");
            foreach (string value in this.Values)
            {
                if (!isFirst)
                {
                    builder.Append(",");
                }

                builder.Append("'");
                builder.Append(value);
                builder.Append("'");

                isFirst = false;
            }
            builder.Append("}");

            return builder.ToString();
        }
    }
}
