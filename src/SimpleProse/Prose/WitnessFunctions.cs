using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Utils;

namespace SimpleProse.Prose
{
    public class WitnessFunctions : DomainLearningLogic
    {
        public WitnessFunctions(Grammar grammar) : base(grammar) { }

        [WitnessFunction("MoveFirstRight", 0, Verify = true, DependsOnParameters = new [] {1})]
        public DisjunctiveExamplesSpec WitnessMoveFirstRightInput(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec kSpec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 0");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = ((IEnumerable<int>) input.Bindings.First().Value).ToList();
                var lsts = spec.DisjunctiveExamples[input];
                var k = (int) kSpec.Examples[input];
                if (lsts == null)
                {
                    return null;
                }

                if (!examples.ContainsKey(input))
                {
                    examples[input] = new List<object>();
                }
                
                
                foreach (List<int> lst in lsts)
                {
                    var result = lst.ToList();
                    var item = result[k];
                    result.RemoveAt(k);
                    result.Insert(0, item);
                    ((List<object>) examples[input]).Add(result);
                    
                }
            }

            return new DisjunctiveExamplesSpec(examples);
        }

        [WitnessFunction("MoveFirstRight", 1)]
        public DisjunctiveExamplesSpec WitnessMoveFirstRightK(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            Console.WriteLine($"Witness Function {rule.Id} 1");
            var examples = new Dictionary<State, IEnumerable<object>>();
            foreach (var input in spec.ProvidedInputs)
            {
                var before = ((IEnumerable<int>) input.Bindings.First().Value).ToList();
                var finalList = Enumerable.Range(1, before.Count - 1).ToEnumerable();

                examples[input] = finalList;

            }
            return new DisjunctiveExamplesSpec(examples);
            
        }
        //Make the witness functions return 2 on the inside function and 1 on the outside function
        //Change MoveRight to MoveRightOne, MoveRightTwo
    }
}