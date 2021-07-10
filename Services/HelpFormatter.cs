using System.Collections.Generic;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace leetbot_night.Services
{
    internal class HelpFormatter : BaseHelpFormatter
    {
        private readonly DiscordEmbedBuilder _messageBuilder;
        private readonly CommandContext _cmdctx;

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {
            _cmdctx = ctx;
            _messageBuilder = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x3889C4),
                Title = ":information_source: Help",
                Description = "",
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"// leetbot v{BotMain.Version}" }
            };
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _messageBuilder.Title += $": command !{command.QualifiedName}\n";
            _messageBuilder.Description += $":clipboard: **Description:** {command.Description ?? "no description provided."}\n";
            if (command.Aliases.Any())
                _messageBuilder.Description += $":pencil: **Aliases:** `{string.Join("`, `", command.Aliases)}`\n";
            if (!command.Overloads[0].Arguments.Any())
                return this;
            string arglist = string.Join("\n",
                command.Overloads[0]
                    .Arguments
                    .Select(xarg => $"`{xarg.Name}` ({_cmdctx.CommandsNext.GetUserFriendlyTypeName(xarg.Type)}) - {xarg.Description ?? "no description provided."}"));
            _messageBuilder.Description += $":wrench: **Arguments:**\n{arglist}\n";
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            string subcmdstring = string.Join("\n", subcommands.Select(xc =>
                $"`!{xc.QualifiedName}" +
                $"{string.Join("", xc.Overloads[0].Arguments.Select(xarg => $" <{xarg.Name}>"))}` - {xc.Description}"));
            _messageBuilder.Description += $":tools: **Commands:**\n{subcmdstring}\n";
            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new(embed: _messageBuilder);
        }
    }
}
