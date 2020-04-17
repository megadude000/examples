using System.Collections.Generic;

namespace Company.Pebbles.Examination.Services.Interfaces
{
    public interface IIterationsService
    {
        string Current { get; }
        bool IsNextExist { get; }

        string NextIteration();
        void SetIterations(IEnumerable<string> audioFileNames, bool isRandom);
    }
}
