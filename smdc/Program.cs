using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smdc
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("smdc - (c) 2013 AtivKang Team");
            Console.WriteLine();

            if (args.Length < 1)
            {
                Usage();
                return;
            }

            SmdCompiler compiler = new SmdCompiler();

            compiler.Input = args[0];

            if (args.Length > 1)
                compiler.Output = args[1];

           // try
           // {
                compiler.Compile();
           // }
           // catch (Exception ex)
           // {
           //     Console.WriteLine(ex.Message);
           // }
        }

        private static void Usage()
        {
            Console.WriteLine("usage: smdc <input file> [output file]");
        }
    }
}
