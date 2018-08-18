using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tutorial2
{
    class OurRandom
    {
        private Random rng = null;

        public int RandomNumber
        {
            get => (rng != null ? rng.Next() : throw new Exception("Number Generator not initialized."));
        }

        public void init()
        {
            if (rng == null)
                rng = new Random();
            else
                throw new Exception("Already Initialized");
        }

        public int getRandomNumber(int min = 0, int max = Int32.MaxValue)
        {
            if (rng != null)
                return rng.Next(min, max);
            else
                throw new Exception("Number Generator not initialized.");
        }
    }
}
