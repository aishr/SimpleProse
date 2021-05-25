using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Features;

namespace SimpleProse.Prose
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "Score", isComplete: true) { }
        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;
        public static double ScoreForContext = 0;
        
        /*
        [FeatureCalculator("Id")]
        public static double Score_Id(double input) => 10;
        */
        
        [FeatureCalculator("MoveFirstRight")]
        public static double Score_MoveFirstRight(double input, double k) => k;
        
        [FeatureCalculator("k", Method = CalculationMethod.FromLiteral)]
        public static double KScore(int type) => type < 0 ? 1 : 3;
        
    }
}