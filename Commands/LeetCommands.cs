using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace leetbot_night.Commands
{
    public class LeetCommands : BaseCommandModule
    {
        [Command("leet"),
         Description("translates message to leetspeak.")]
        public async Task Leet(CommandContext ctx,
            [Description("your string to translate."), RemainingText] string input)
        {
            input = input.ToUpper();
            var output = "```";

            foreach (char c in input)
            {
                switch (c.ToString())
                {
                    case "A":
                    case "А":
                        output += "4";
                        break;
                    case "B":
                    case "В":
                        output += "8";
                        break;
                    case "C":
                    case "С":
                        output += "(";
                        break;
                    case "D":
                    case "Д":
                        output += "|)";
                        break;
                    case "E":
                    case "Е":
                    case "Ё":
                        output += "3";
                        break;
                    case "F":
                        output += "|=";
                        break;
                    case "G":
                    case "Б":
                        output += "6";
                        break;
                    case "H":
                    case "Н":
                        output += "|-|";
                        break;
                    case "I":
                        output += "!";
                        break;
                    case "J":
                        output += ")";
                        break;
                    case "K":
                    case "К":
                        output += "|<";
                        break;
                    case "L":
                        output += "1";
                        break;
                    case "M":
                    case "М":
                        output += "|\\/|";
                        break;
                    case "N":
                        output += "|\\|";
                        break;
                    case "O":
                    case "О":
                        output += "()";
                        break;
                    case "P":
                    case "Р":
                        output += "|>";
                        break;
                    case "Q":
                        output += "9";
                        break;
                    case "R":
                        output += "|2";
                        break;
                    case "S":
                        output += "5";
                        break;
                    case "T":
                    case "Т":
                        output += "7";
                        break;
                    case "U":
                        output += "|_|";
                        break;
                    case "V":
                        output += "\\/";
                        break;
                    case "W":
                        output += "\\/\\/";
                        break;
                    case "X":
                    case "Х":
                        output += "><";
                        break;
                    case "Y":
                    case "У":
                        output += "'/";
                        break;
                    case "Z":
                        output += "2";
                        break;
                    case "Г":
                        output += "r";
                        break;
                    case "Ж":
                        output += "}|{";
                        break;
                    case "З":
                        output += "'/_";
                        break;
                    case "И":
                    case "Й":
                        output += "|/|";
                        break;
                    case "Л":
                        output += "J|";
                        break;
                    case "П":
                        output += "/7";
                        break;
                    case "Ф":
                        output += "qp";
                        break;
                    case "Ц":
                        output += "||_";
                        break;
                    case "Ч":
                        output += "'-|";
                        break;
                    case "Ш":
                        output += "LLI";
                        break;
                    case "Щ":
                        output += "LLL";
                        break;
                    case "Ъ":
                        output += "'b";
                        break;
                    case "Ы":
                        output += "bI";
                        break;
                    case "Ь":
                        output += "b";
                        break;
                    case "Э":
                        output += "-)";
                        break;
                    case "Ю":
                        output += "10";
                        break;
                    case "Я":
                        output += "9I";
                        break;
                    default:
                        output += c.ToString();
                        break;
                }
            }
            output += "```";

            var embed = new DiscordEmbedBuilder
            {
                Color = ctx.Message.Author is DiscordMember m ? m.Color : new DiscordColor(0xFFFFFF),
                Description = output,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                }
            };
            await ctx.RespondAsync(embed: embed);
            if (ctx.Guild != null) await ctx.Message.DeleteAsync();
        }

        [Command("leetr"),
         Description("translates russian leetspeak to human (if it's not working try `!leetren` for english).")]
        public async Task LeetR(CommandContext ctx,
            [Description("your string to translate."), RemainingText] string inputleet)
        {
            var output = "";

            for (var i = 0; i < inputleet.Length;)
            {
                if (inputleet.Length > 3 && inputleet[..4] == "|\\/|")
                {
                    output += "М";
                    inputleet = inputleet.Remove(0, 4);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "'/_")
                {
                    output += "З";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "|-|")
                {
                    output += "Н";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "}|{")
                {
                    output += "Ж";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "|/|")
                {
                    output += "И";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "||_")
                {
                    output += "Ц";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "'-|")
                {
                    output += "Ч";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "LLI")
                {
                    output += "Ш";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "LLL")
                {
                    output += "Щ";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|<")
                {
                    output += "К";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "()")
                {
                    output += "О";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "J|")
                {
                    output += "Л";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "/7")
                {
                    output += "П";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|>")
                {
                    output += "Р";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|)")
                {
                    output += "Д";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "><")
                {
                    output += "Х";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "'/")
                {
                    output += "У";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "qp")
                {
                    output += "Ф";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "'b")
                {
                    output += "Ъ";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "bI")
                {
                    output += "Ы";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "-)")
                {
                    output += "Э";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "10")
                {
                    output += "Ю";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "9I")
                {
                    output += "Я";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet[..1] == "4")
                {
                    output += "А";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "6")
                {
                    output += "Б";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "8")
                {
                    output += "В";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "r")
                {
                    output += "Г";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "3")
                {
                    output += "Е";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "(")
                {
                    output += "С";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "7")
                {
                    output += "Т";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "b")
                {
                    output += "Ь";
                    inputleet = inputleet.Remove(0, 1);
                }
                else
                {
                    output += inputleet[..1];
                    inputleet = inputleet.Remove(0, 1);
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0xFF00FF),
                Description = output,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "transleetion (russian)",
                    IconUrl = null
                }
            };

            await ctx.RespondAsync(embed: embed);
        }

        [Command("leetren"),
         Description("translates english leetspeak to human (if it's not working try `!leetr` for russian).")]
        public async Task LeetREn(CommandContext ctx,
            [Description("your string to translate."), RemainingText] string inputleet)
        {
            var output = "";

            for (var i = 0; i < inputleet.Length;)
            {
                if (inputleet.Length > 3 && inputleet[..4] == "|\\/|")
                {
                    output += "M";
                    inputleet = inputleet.Remove(0, 4);
                }
                else if (inputleet.Length > 3 && inputleet[..4] == "\\/\\/")
                {
                    output += "W";
                    inputleet = inputleet.Remove(0, 4);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "|-|")
                {
                    output += "H";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "|\\|")
                {
                    output += "N";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 2 && inputleet[..3] == "|_|")
                {
                    output += "U";
                    inputleet = inputleet.Remove(0, 3);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|=")
                {
                    output += "F";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|)")
                {
                    output += "D";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|<")
                {
                    output += "K";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "()")
                {
                    output += "O";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|>")
                {
                    output += "P";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "|2")
                {
                    output += "R";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "\\/")
                {
                    output += "V";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "><")
                {
                    output += "X";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet.Length > 1 && inputleet[..2] == "'/")
                {
                    output += "Y";
                    inputleet = inputleet.Remove(0, 2);
                }
                else if (inputleet[..1] == "4")
                {
                    output += "A";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "8")
                {
                    output += "B";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "(")
                {
                    output += "C";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "3")
                {
                    output += "E";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "6")
                {
                    output += "G";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "!")
                {
                    output += "I";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == ")")
                {
                    output += "J";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "1")
                {
                    output += "L";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "9")
                {
                    output += "Q";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "5")
                {
                    output += "S";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "7")
                {
                    output += "T";
                    inputleet = inputleet.Remove(0, 1);
                }
                else if (inputleet[..1] == "2")
                {
                    output += "Z";
                    inputleet = inputleet.Remove(0, 1);
                }
                else
                {
                    output += inputleet[..1];
                    inputleet = inputleet.Remove(0, 1);
                }
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0xFF00FF),
                Description = output,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "transleetion (english)",
                    IconUrl = null
                }
            };
            await ctx.RespondAsync(embed: embed);
        }
    }
}
