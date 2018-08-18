using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial2
{
    class Program
    {
        static void interpretCommand(string[] args, ref OurRandom RNG)
        {
            if (args[0] == "init")
            {
                RNG.init();
                string input = Console.ReadLine();
                interpretCommand(input.Split(null), ref RNG);
            }
            else if (args[0] == "roll")
            {
                switch (args.Count())
                {
                    case 1:
                        Console.WriteLine(RNG.RandomNumber);
                        break;
                    case 2:
                        int value = 0;
                        if (!Int32.TryParse(args[1], out value))
                            throw new Exception("Invalid Number");
                        while (value-- > 0)
                        {
                            Console.WriteLine(RNG.RandomNumber);
                        }
                        break;
                    case 3:
                        int value1 = 0;
                        if (!Int32.TryParse(args[1], out value1))
                            throw new Exception("Invalid Number");
                        int value2 = 0;
                        if (!Int32.TryParse(args[2], out value2))
                            throw new Exception("Invalid Number");
                        Console.WriteLine(RNG.getRandomNumber(value1, value2));
                        break;
                }
            }
        }

        static int Main(string[] args)
        {
            if (args.Count() < 1)
                return 1;

            OurRandom RNG = new OurRandom();

            try
            {
                interpretCommand(args, ref RNG);
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception Caught: " + e.ToString());
            }

            return 0;
        }
    }
}
