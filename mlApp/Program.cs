namespace mlApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using mlLib;

    class Program
    {
        static void Main(string[] args)
        {
            Instances data = null;
            
            // read in the data from file
            using (StreamReader stream = new StreamReader(args[0]))
            {
                data = ArffReader.Read(stream);   
            }

            // output the data read
            System.Console.Out.WriteLine(data);
            System.Console.Read();
        }
    }
}
