using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace leetbot_night.Math
{
	public class MathCommands : BaseCommandModule
	{
		[Command("sum"), Hidden,
		 Description("sums integers.")]
		public async Task NumSum(CommandContext ctx,
			[Description("Integers to sum.")] params int[] args)
		{
			await ctx.TriggerTypingAsync();
			int sum = args.Sum();
			await ctx.RespondAsync($"The sum of these numbers is {sum}");
		}

		// here we use our custom type, for which have registered a converter during initialization
		[Command("math"),
		 Description("does basic math.")]
		public async Task SimpleMath(CommandContext ctx,
			[Description("first operand.")] double num1,
			[Description("math operator.\nSupported operators: `+`, `-`, `*`, `/`, `%`, `^`.")] MathOperation operation,
			[Description("second operand.")] double num2)
		{
			var result = 0.0;
			switch (operation)
			{
				case MathOperation.Add:
					result = num1 + num2;
					break;

				case MathOperation.Subtract:
					result = num1 - num2;
					break;

				case MathOperation.Multiply:
					result = num1 * num2;
					break;

				case MathOperation.Divide:
					result = num1 / num2;
					break;

				case MathOperation.Modulo:
					result = num1 % num2;
					break;

				case MathOperation.Power:
					result = System.Math.Pow(num1, num2);
					break;
			}
			await ctx.RespondAsync($"The result is {System.Math.Round(result, 7)}");
		}
	}
}
