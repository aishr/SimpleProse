using System.Collections.Generic;
using System.Linq;
using Microsoft.Z3;

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
        public static Node MoveFirstRight(Node input, int k)
        {
            var result = input.Children.ToList();
            if (k >= result.Count)
            {
                return null;
            }
            
            result.Insert(k + 1, result[0]);
            result.RemoveAt(0);

            var newChildren = result.Select(x => (BoolExpr) x.Expr);

            var newExpr = input.Ctx.MkOr(newChildren);

            return Utils.HandleSmtLibParsed(newExpr, input.Ctx);

        }
        
    }
}