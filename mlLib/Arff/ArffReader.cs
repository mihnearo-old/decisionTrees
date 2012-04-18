namespace mlLib.Arff
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using mlLib.Log;

    /// <summary>
    /// Class reads and parses a data file in arff format.
    /// </summary>
    public static class ArffReader
    {
        #region Constants

        private const string DataSectionMarker = "@data";

        private static readonly Regex AttributeLineRegex = 
            new Regex(
                @"^@attribute\s(?:(?:\'(?'Name'.+?)\')|(?'Name'\S+?))\s{(?'Values'.+?)}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static readonly Regex ValueListRegex =
            new Regex(
                @"(?:(?:\'(?'Value'.+?)\')|(?'Value'\S+?))(?:,|$)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads and parses the data stream in arff format into an instance of class Instances.
        /// </summary>
        /// <param name="dataStream">data stream with arff data</param>
        /// <param name="classAttributeName">name of the attribute to be used as class</param>
        /// <returns>object containing all training data</returns>
        public static Instances Read(StreamReader dataStream, string classAttributeName)
        {
            if (null == dataStream)
            {
                throw new ArgumentNullException("dataStream");
            }

            Instances data = new Instances();
            data.AddClassAttribute(classAttributeName);

            // move to the start of the stream - just in case
            dataStream.BaseStream.Seek(0, SeekOrigin.Begin);

            bool readingData = false;
            while (!dataStream.EndOfStream)
            {
                string line = dataStream.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    // skip over empty lines
                    continue;
                }

                // is this the data header
                if (line.Equals(DataSectionMarker))
                {
                    readingData = true;
                    continue;
                }

                // process the line
                if (!readingData)
                {
                    ParseMetadataLine(line, ref data);
                }
                else
                {
                    ParseDataLine(line, ref data);
                }
            }

            return data;
        }

        #endregion

        #region Private Methods

        private static void ParseDataLine(string line, ref Instances data)
        {
            List<string> valueList = ParseValueList(line);
            if (null == valueList
                || 0 == valueList.Count)
            {
                throw new Exception("Unexpected exception");
            }

            data.AddInstance(valueList);

            if (data.Examples.Length % 1000 == 0)
            {
                // output some progress
                Logger.Log(LogLevel.Progress, " {0}", data.Examples.Length);
            }
        }

        private static void ParseMetadataLine(string line, ref Instances data)
        {
            // the only metadata lines we care about are attributes
            Match attributeMatch = AttributeLineRegex.Match(line);
            if (!attributeMatch.Success)
            {
                return;
            }

            // retrieve the data
            string attributeName = attributeMatch.Groups["Name"].Value;
            string attributeValueList = attributeMatch.Groups["Values"].Value;

            // lets parse the value list
            List<string> valueList = ParseValueList(attributeValueList);
            if (null == valueList
                || 0 == valueList.Count)
            {
                throw new Exception("Unexpected exception");
            }

            // add the attribute definition
            data.AddAttribute(attributeName, valueList);

        }

        private static List<string> ParseValueList(string attributeValueList)
        {
            MatchCollection valueMatchList = ValueListRegex.Matches(attributeValueList);
            if (0 == valueMatchList.Count)
            {
                throw new FormatException("Attribute meta did not specify the list of values.");
            }

            List<string> valueList = new List<string>();
            foreach (Match valueMatch in valueMatchList)
            {
                if (!valueMatch.Success)
                {
                    continue;
                }

                valueList.Add(valueMatch.Groups["Value"].Value);
            }

            return valueList;
        }

        #endregion
    }
}
