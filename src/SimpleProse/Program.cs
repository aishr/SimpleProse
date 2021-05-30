using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Logging;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.Z3;
using SimpleProse.Prose;

namespace SimpleProse
{
    class Program
    {
        private static SynthesisEngine _prose;

        private static Result<Grammar> Grammar { get; set; }
        
        // a or b or c
        //private static readonly Node InputTree = new Node("or", new List<Node>());

        // !a -> b or c
        //private static readonly Node OutputTree = new Node("implies", new List<Node>());
        private const string PathToFiles = @"./../../../";

        private const string SimpleBakeryPrefix = "(declare-const Inv_0_n (Array Int Int)) " +
                                                  "(declare-const Inv_1_n (Array Int Int)) " +
                                                  "(declare-const Inv_2_n Int) " +
                                                  "(declare-const Inv_3_n Int) ";

        private static void GetGrammar(string grammarFileName)
        {
            
            var reader = new StreamReader(PathToFiles + grammarFileName);
            var grammar = reader.ReadToEnd();
            Grammar = DSLCompiler.Compile(
                new CompilerOptions() {
                InputGrammarText = grammar,
                References = CompilerReference.FromAssemblyFiles(
                    typeof(Semantics).GetTypeInfo().Assembly, 
                    typeof(Node).GetTypeInfo().Assembly)
                });
        }

        private static void TestSimple(IEnumerable<int> input, IEnumerable<int> output)
        {
            GetGrammar(@"Prose/test.grammar");
            var inputExamples = new List<Tuple<IEnumerable<int>, IEnumerable<int>>>()
            {
                new (input, output)
            };

            var spec = Utils.CreateSimpleExampleSpec(inputExamples, Grammar);
            RankingScore.ScoreForContext = 100;
            var scoreFeature = new RankingScore(Grammar.Value);
            DomainLearningLogic learningLogic = new WitnessFunctions(Grammar.Value);

            _prose = new SynthesisEngine(Grammar.Value,
                new SynthesisEngine.Config
                {
                    LogListener = new LogListener(),
                    Strategies = new ISynthesisStrategy[] {new DeductiveSynthesis(learningLogic)},
                    UseThreads = false,
                    CacheSize = int.MaxValue
                });
            var learned = _prose.LearnGrammarTopK(spec, scoreFeature);

            Console.WriteLine("Possible Programs: " + learned);

            var finalPrograms = learned.RealizedPrograms.ToList();
            foreach (var program in finalPrograms)
            {
                var inputState = State.CreateForExecution(Grammar.Value.InputSymbol, input);
                var actual = (List<int>)program.Invoke(inputState);
                foreach (var num in actual)
                {
                    Console.Write(num + ",");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
        }
        
        private static void Test(string input, string output)
        {
            GetGrammar(@"Prose/test.grammar");
            using (var ctx = new Context())
            {
                var testInput = SmtLib.Smt2FileTest(ctx, input);
                var testInputTree = Utils.HandleSmtLibParsed(testInput, ctx);
                Console.WriteLine(testInputTree.Expr.ToString());

                var testOutput = SmtLib.Smt2FileTest(ctx, output);
                var testOutputTree = Utils.HandleSmtLibParsed(testOutput, ctx);


                Console.WriteLine("Input: " + ReadableParser.ToReadable(testInputTree.Expr, new List<string>().ToArray()));
                Console.WriteLine();
                Console.WriteLine("Output: " + ReadableParser.ToReadable(testOutputTree.Expr, new List<string>().ToArray()));
                Console.WriteLine();

                var inputExamples = new List<Tuple<Node, Node>>()
                {
                    new (testInputTree, testOutputTree)
                };

                var spec = Utils.CreateExampleSpec(inputExamples, Grammar);
                RankingScore.ScoreForContext = 100;
                var scoreFeature = new RankingScore(Grammar.Value);
                DomainLearningLogic learningLogic = new WitnessFunctions(Grammar.Value);

                _prose = new SynthesisEngine(Grammar.Value,
                  new SynthesisEngine.Config
                  {
                      LogListener = new LogListener(),
                      Strategies = new ISynthesisStrategy[] { new DeductiveSynthesis(learningLogic) },
                      UseThreads = false,
                      CacheSize = int.MaxValue
                  });
                var learned = _prose.LearnGrammarTopK(spec, scoreFeature);
                _prose.Configuration.LogListener.SaveLogToXML("imp.xml");

                Console.WriteLine("Possible Programs: " + learned);
                var finalPrograms = learned.RealizedPrograms.ToList();
                foreach (var program in finalPrograms)
                {
                    var inputState = State.CreateForExecution(Grammar.Value.InputSymbol, testInputTree);
                    var actual = (Node)program.Invoke(inputState);
                    Console.WriteLine("Actual: " + ReadableParser.ToReadable(actual.Expr, new List<string>().ToArray()));
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }
        
        private static void Main()
        {
            /*
            var input = new List<int> {0, 1, 2};
            var intOutput = new List<int> {1, 2, 0};
            var output = new List<int> {2, 1, 0};
            //TestSimple(input, intOutput);
            //TestSimple(intOutput, output);
            TestSimple(input, output);
            */
            Test("move-input.smt2", "move-output.smt2");
        }
    }
}
