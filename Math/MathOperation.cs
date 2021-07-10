using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace leetbot_night.Math
{
    public class MathOperationConverter : IArgumentConverter<MathOperation>
    {
        public Task<Optional<MathOperation>> ConvertAsync(string value, CommandContext ctx)
        {
            Optional<MathOperation> mathop = value switch
            {
                "+" => MathOperation.Add,
                "-" => MathOperation.Subtract,
                "*" => MathOperation.Multiply,
                "/" => MathOperation.Divide,
                "%" => MathOperation.Modulo,
                "^" => MathOperation.Power,
                _ => new Optional<MathOperation>()
            };
            return Task.FromResult(mathop);
        }
    }

    public enum MathOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        Power
    }
}
