using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Antlr4.Runtime.Misc;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Diagnostics;
using Microsoft.ProgramSynthesis.Extraction.Json.Build.NodeTypes;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Logging;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils.Interactive;
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
                    typeof(Semantics).GetTypeInfo().Assembly)
                });
        }

        private static void TestSimple(IEnumerable<int> input, IEnumerable<int> output)
        {
            GetGrammar(@"Prose/test.grammar");
            var inputExamples = new List<Tuple<IEnumerable<int>, IEnumerable<int>>>()
            {
                new Tuple<IEnumerable<int>, IEnumerable<int>>(input, output)
            };

            var spec = CreateSimpleExampleSpec(inputExamples, Grammar);
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
                actual.ForEach(Console.WriteLine);
            }
            Console.WriteLine();
            Console.WriteLine();
        }
        
        
        public static ExampleSpec CreateSimpleExampleSpec(IEnumerable<Tuple<IEnumerable<int>, IEnumerable<int>>> examples, Result<Grammar> grammar)
        {
            var proseExamples = new Dictionary<State, object>();
            foreach (var example in examples)
            {
                var input = State.CreateForExecution(grammar.Value.InputSymbol, example.Item1);
                var astAfter = example.Item2;
                proseExamples.Add(input, astAfter);
            }
            var spec = new ExampleSpec(proseExamples);
            return spec;
        }

        private static void Main()
        {
            var input = new List<int> {0, 1, 2};
            var intOutput = new List<int> {1, 2, 0};
            var output = new List<int> {2, 1, 0};
            //TestSimple(input, intOutput);
            //TestSimple(intOutput, output);
            TestSimple(input, output);

        }
    }
}
