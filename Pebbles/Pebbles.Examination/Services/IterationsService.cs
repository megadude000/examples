using Company.Pebbles.Examination.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Company.Pebbles.Examination.Services
{
    internal class IterationsService : IIterationsService
    {
        private int iterator;
        private IList<string> iterations;

        public string Current { get => iterator == -1 ? "" : iterations[iterator]; }
        public bool IsNextExist { get => iterator < iterations.Count - 1; }

        public IterationsService()
        {
            iterator = -1;
        }

        public void SetIterations(IEnumerable<string> audioFileNames, bool isRandom)
        {
            if (isRandom)
            {
                iterations = RandomSort(audioFileNames).ToList();
            }
            else
            {
                iterations = audioFileNames.ToList();
            }
        }

        public string NextIteration()
        {
            return iterations[++iterator];
        }

        private IEnumerable<string> RandomSort(IEnumerable<string> data)
        {
            var rand = new Random();
            return data.OrderBy(_ => rand.Next());
        }
    }
}
