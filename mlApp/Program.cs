namespace mlApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using mlLib;
    using mlLib.DecisionTree;
    using mlLib.Learners;

    class Program
    {
        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            Instances data = null;
            
            // read in the data from file
            using (StreamReader stream = new StreamReader(args[0]))
            {
                data = ArffReader.Read(stream, "Class");   
            }

            
            // learn the tree
            ID3Learner learner = new ID3Learner(0.0, true);
            Node decisionTree = learner.Learn(data);

            // output the tree
            System.Console.Out.WriteLine(decisionTree);

            System.Console.Out.WriteLine("Runtime: {0}", (DateTime.Now - start).ToString("c"));
            System.Console.Out.WriteLine("Press any key to exit");
            System.Console.Read();
        }
    }
}
