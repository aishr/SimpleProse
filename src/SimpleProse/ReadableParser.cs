using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Z3;

namespace SimpleProse
{
    public class ReadableParser
    {
        //Original TypeScript function was declared export
        public static string ToReadable(Expr expr, string[] varList)
        {
            var reorderResult = ParseResult(expr);

            return varList.Length == 0 ? reorderResult : ReplaceVarNames(reorderResult, varList);
        }
        
        //Assume lst is array of strings and sep is string
        private static string ParseResult(Expr expr)
        {
            var result = "";
            //Replaces lines 16-19 of readable.ts
            //symbols for logical relations
            var logSym = new Dictionary<Z3_decl_kind, string>()
            {
                {Z3_decl_kind.Z3_OP_AND, "&&"}, 
                {Z3_decl_kind.Z3_OP_OR, "||"}
            };

            //symbols for mathematical operations
            //Note: "-" is not included because negative numbers are in the form (- x)           
            var logOp = new Dictionary<Z3_decl_kind, string>()
            {
                {Z3_decl_kind.Z3_OP_EQ, "="},
                {Z3_decl_kind.Z3_OP_GE, ">="},
                {Z3_decl_kind.Z3_OP_GT, ">"},
                {Z3_decl_kind.Z3_OP_LT, "<"},
                {Z3_decl_kind.Z3_OP_ADD, "+"},
                {Z3_decl_kind.Z3_OP_MUL, "*"},
                {Z3_decl_kind.Z3_OP_DIV, "/"}
            };

            if (expr.Args.Length == 0)
            {
                var x = expr.ToString();
                return x;
                //parseResult returns string
            }

            //logical symbol should be inserted between each child clause
            //logSym.ContainsKey(lst[0]) should do same thing as lst[0] in logSym in line 31 of readable.ts
            if (logSym.ContainsKey(expr.FuncDecl.DeclKind))
            {
                //Not sure what array.splice() is in TypeScript with one argument
                //Ash: same as js! one argument just means take the entire rest of the list starting at the argument index given
                //Ash: Given an array x = [1, 2, 3, 4], splice(1) -> [2, 3, 4]
                for (var i = 0; i < expr.Args.Length; i++)
                {
                    if (i == expr.Args.Length - 1)
                    {
                        result += ParseResult(expr.Args[i]);
                        return result;
                    }
                    result += ParseResult(expr.Args[i]) + " " + logSym[expr.FuncDecl.DeclKind] + "\n";
                }
            }

            //handles indexing into an array
            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_SELECT)
            {
                //parseResult expects string[] as first argument
                return ParseResult(expr.Args[0]) + "[" + ParseResult(expr.Args[1]) + "]";
            }

            //Adds not symbol (!) to beginning of clause
            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_NOT)
            {
                //parseResult expects string[] as first argument
                return "!(" + ParseResult(expr.Args[0]) + ")";
            }

            if (expr.FuncDecl.DeclKind == Z3_decl_kind.Z3_OP_UMINUS)
            {
                return "- (" + ParseResult(expr.Args[0]) + ")";
            }
            return ParseResult(expr.Args[0]) + " " + expr.FuncDecl.Name + " " + ParseResult(expr.Args[1]);
        }

        private static string ReplaceVarNames(string expr, string[] varList)
        {
            for(var i = 0; i < varList.Length; ++i)
            {
                //Meant to replace lines 78-79 in readable.ts
                //Assuming "gi" means case insensitivity
                expr = Regex.Replace(expr, @"Inv_" + i + "_n", varList[i], RegexOptions.IgnoreCase);
            }
               
            return expr;
        }
    }
}
