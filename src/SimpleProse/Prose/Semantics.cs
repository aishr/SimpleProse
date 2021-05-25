using System.Collections.Generic;
using System.Linq;

namespace SimpleProse.Prose
{
    public static class Semantics
    {
        /*
        public static IEnumerable<int> Id(IEnumerable<int> input)
        {
            return input;
        }
        */
        public static IEnumerable<int> MoveFirstRight(IEnumerable<int> input, int k)
        {
            var result = input.ToList();
            if (k >= result.Count)
            {
                return null;
            }
            
            result.Insert(k + 1, result[0]);
            result.RemoveAt(0);

            return result;
        }
        
    }
}