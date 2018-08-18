using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThisIsTheTestMod
{
    public class Wuerfel<T>
    {
        T b;
        public Wuerfel()
        {
            b = 1;
        }
        public Wuerfel(int b)
        {
            setWuerfel(b);
        }

        public int getWuerfel()
        {
            return b;
        }

        /// <exception cref="Exception">WEIL DU DUMM BIST</exception>
        public void setWuerfel(int b)
        {
            if (b > 0)
                this.b = b;
            else
                throw new Exception("DU PLEB");


            do
            {
                while (true)
                    break;
                break;
            } while (true);

            for(int i = 0; i < 10; i++)
            {
                //blub
                continue;
                //bla
            }
        }
    }

    public class MainClass
    {
        static int Main(string[] args)
        {
            Console.WriteLine(3.0f); //1
            try
            {
                Console.WriteLine(3.0f); //2
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("Error Detected. BLISSFULLY IGNORED");//nur wenns kaputt geht
            }

            Wuerfel block1 = new Wuerfel(3);
            block1.setWuerfel(2);

            return 0;//3
        }

        static void test()
        {
            throw new Exception("Brah");
        }
    }
}

