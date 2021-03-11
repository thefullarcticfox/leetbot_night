using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace leetbot_night.Interactivities
{
	public class DiceRolling : BaseCommandModule
	{
		public static int RollD(int d)
		{
			var r = new Random();
			return r.Next(1, d + 1);
		}

		public static string HumanNameFormat(string input)
		{
			string[] words = input.ToLower().Split(' ');
			string formatname = words.Aggregate("", (current, word) =>
				current + word.Substring(0, 1).ToUpper() + word[1..] + " ");
			formatname = formatname.Remove(formatname.Length - 1, 1);
			return formatname;
		}

		[Command("rolldice"),
		 Description("rolls die(dice).")]
		public async Task RollDice(CommandContext ctx,
			[Description("number of die sides.\nYou can enter more than one die.")] params int[] d)
		{
			var answer = "";
			var dicesum = 0;
			foreach (int die in d)
			{
				int onedie = RollD(die);
				dicesum += onedie;
				answer += $"{onedie} + ";
			}

			switch (d.Length)
			{
				case 0:
					await ctx.RespondAsync("You didn't roll any.");
					break;
				case 1:
					await ctx.RespondAsync($":game_die: Rolled {dicesum}");
					break;
				default:
				{
					if (d.Length > 1)
					{
						answer = answer[0..^3];
						answer += $" = {dicesum}";
						await ctx.RespondAsync($":game_die: Rolled {answer}");
					}
					break;
				}
			}
		}

		[Command("kojimaname"),
			Description("Kojima Name Generator by Brian David Gilbert.\n" +
						"https://www.youtube.com/watch?v=t-3i6GBYvdw")]
		public async Task KojimaName(CommandContext ctx,
			[Description("enter `categories` parameter to see name categories descriprion."), RemainingText] string mode)
		{
			DiscordEmbedBuilder embed;
			string embedtitle;
			string embedmsg;
			if (mode == "categories")         // categories help
			{
				embedtitle = "\nName Categories";
				embedmsg = "*Kojima names fall into a finite number of categories. " +
						   "This section will determine the category in which you name belongs.*\n\n" +
						   "**NORMAL NAME**\n" +
						   "Kojima’s early work includes lots of characters that have names that are " +
						   "widely considered to be “normal.” Was this just because, in the early years, " +
						   "he didn’t have the power to say, “I’m Hideo Kojima, I can name someone DieHardman " +
						   "if I want to” without people questioning him? Probably.\n" +
						   "**OCCUPATIONAL NAME**\n" +
						   "Kojima’s characters tend to be driven by the work that they do. That often " +
						   "carries over to their names.You, too, can be nothing more than your job.\n" +
						   "**HORNY NAME**\n" +
						   "Kojima’s characters and stories are irrevocably horny.Weirdly horny, sure, but horny nonetheless.\n" +
						   "**THE NAME**\n" +
						   "Kojima loves to make people have names that start with the word “The” " +
						   "and they usually symbolize fears or unstoppable forces. You are now that unstoppable force.\n" +
						   "**COOL NAME**\n" +
						   "Kojima loves to be cool. Sometimes, his idea of cool is a bit strange, " +
						   "but it is always cool to Hideo Kojima, and that’s what matters.\n" +
						   "**VIOLENT NAME**\n" +
						   "Sometimes, a Kojima name can be very threatening and violent, " +
						   "like Sniper Wolf, or The Fury. Now you can also be threatening and violent.\n" +
						   "**NAME THAT LACKS SUBTEXT**\n" +
						   "Sometimes, Kojima gives up and just names a character exactly what they are. " +
						   "Congratulations. You are exactly what you do.";
				embed = new DiscordEmbedBuilder
				{
					Title = "KOJIMA NAME GENERATOR" + embedtitle,
					Description = embedmsg,
					Color = new DiscordColor(0xDA0050),
					Footer = new DiscordEmbedBuilder.EmbedFooter
					{
						Text = "Created by Brian David Gilbert at Polygon."
					}
				};
				await ctx.RespondAsync(embed: embed);
				return;
			}

			// main stuff here
			InteractivityExtension interactivity = ctx.Client.GetInteractivity();
			var answers = new string[21];
			string[] questions = {
				"What is your full name?",
				"What do you do at your occupation?\n" +
				"Condense the verb in your answer into a single ­er noun.\n" +
				"(e.g. if you are a sheep farmer, your answer will be “farmer”)",
				"What was your first pet’s specific species/breed?\n" +
				"If you never had a pet, please answer with an animal you wish you owned.",
				"What’s your most embarrassing childhood memory?\n" +
				"Be specific and condense this story into two words.",
				"What is the object you’d least like to be stabbed by?",
				"What is something you are good at?\n" +
				"(Verb ending in ­ing)",
				"How many carrots do you believe you could eat in one sitting, if someone, like, forced you to eat as many carrots as possible?",
				"What is your greatest intangible fear?\n" +
				"(e.g.death, loneliness, fear itself)",
				"What is your greatest tangible fear?\n" +
				"(e.g.horses)",
				"What is the last thing you did before starting this worksheet?",
				"What condition is your body currently in?\n" +
				"(single word answer)",
				"Favorite state of matter?\n" +
				"(gas, liquid, solid, plasma, idk)",
				"A word your name kind of sounds like?\n" +
				"(e.g.Brian ­> Brain)",
				"What is your Zodiac sign?\n" +
				"(Aries, Taurus, Gemini, Cancer, Leo, Virgo, Libra, Scorpio, Sagittarius, Capricorn, Aquarius, Pisces)",
				"If you had to define your personality in one word, what would it be?",
				"Who is your favorite film character?\n" +
				"(NOTE: must be played by Kurt Russell)",
				"What is the last word of the title of your favorite Kubrick film?",
				"What is the first word in the title of your favorite Joy Division album?",
				"What is a scientific term you picked up from listening to National Public Radio once?",
				"What is a piece of military hardware you think looks cool even though war is bad?",
				"What is something you’d enjoy watching Mads Mikkelsen do?\n" +
				"Please condense into one word."
			};
			embedtitle = "\nSection 1: Determining How Many Names You Have";
			int dice = RollD(6);
			embedmsg = "*Kojima often creates characters that have many alternate names, so we must first figure out how many names you will have.*\n\n";
			var alternatenames = false;
			if (dice == 6)
			{
				alternatenames = true;
				embedmsg += $"Rolled {dice}: You have 1 name + 6 other alternate names.";
			}
			else embedmsg += $"Rolled {dice}: You have 1 name.";
			embed = new DiscordEmbedBuilder
			{
				Title = "KOJIMA NAME GENERATOR" + embedtitle,
				Description = embedmsg,
				Color = new DiscordColor(0xDA0050),
				Footer = new DiscordEmbedBuilder.EmbedFooter
				{
					Text = "Get ready for the first question in 5 seconds"
				}
			};

			DiscordMessage questionmsg = await ctx.RespondAsync(embed: embed);
			await Task.Delay(5000);             // 5 seconds delay to see Section 1

			embedtitle = "\nSection 2: Personal Information";
			embedmsg = "*Kojima’s characters have names that are directly related to their own character traits. This information will make sure your name fits your personality.*\n\n";
			ulong caughtmsgid = 0;              // check so it don't fire up on the same message
			for (var i = 0; i < questions.Length; i++)
			{
				if (i == 15)
				{
					embedtitle = "\nSection 3: Kojima Information";
					embedmsg = "*Kojima character names reflect his own idiosyncrasies. He can’t help himself.*\n\n";
				}
				embed = new DiscordEmbedBuilder
				{
					Title = "KOJIMA NAME GENERATOR" + embedtitle,
					Description = $"{embedmsg}{i + 1}. {questions[i]}",
					Color = new DiscordColor(0xDA0050),
					Footer = new DiscordEmbedBuilder.EmbedFooter
					{
						Text = "Enter `stop it` to stop generating it"
					}
				};
				await questionmsg.ModifyAsync(embed: embed.Build());
				InteractivityResult<DiscordMessage> answermsg = await interactivity.WaitForMessageAsync(xm => xm.ChannelId.Equals(ctx.Message.ChannelId) && xm.Author.Equals(ctx.Message.Author) && (xm.Id != caughtmsgid), TimeSpan.FromMinutes(25));
				if (answermsg.TimedOut || answermsg.Result.Content == "stop it")
				{
					await ctx.RespondAsync("You've decided to stop or timed out.");
					await questionmsg.DeleteAsync();
					return;
				}
				else answers[i] = answermsg.Result.Content;
				caughtmsgid = answermsg.Result.Id;
				if (ctx.Guild != null) await answermsg.Result.DeleteAsync();
			}

			int truename = -1;
			var namepart = "";
			var clonecondition = new bool[7];
			var kojimacondition = new bool[7];
			var names = new string[7];
			var categories = new string[7];
			dice = RollD(20);                                   // Rolling for a true name
			if (dice == 1) truename = 0;                        // You have a NORMAL NAME
			else if (dice is >= 2 and <= 6) truename = 1;		// You have an OCCUPATIONAL NAME
			else if (dice is >= 7 and <= 10) truename = 2;		// You have a HORNY NAME
			else if (dice is >= 11 and <= 13) truename = 3;		// You have a THE NAME
			else if (dice is >= 14 and <= 17) truename = 4;		// You have a COOL NAME
			else if (dice is >= 18 and <= 19) truename = 5;		// You have a VIOLENT NAME
			else if (dice == 20) truename = 6;                  // You have a NAME THAT LACKS SUBTEXT

			for (var i = 0; i < 7; i++)
			{
				dice = RollD(4);
				string mancondition = dice == 4 ? "man" : "";
				dice = RollD(8);
				string conditioncondition = dice switch
				{
					6 => "Big ",
					7 => "Old ",
					8 => $"{answers[10]} ",
					_ => ""
				};
				dice = RollD(12);
				if (dice == 12)
					clonecondition[i] = true;
				else
					clonecondition[i] = false;
				dice = RollD(100);
				if (dice == 69)
					kojimacondition[i] = true;
				else
					kojimacondition[i] = false;
				switch (i)
				{
					case 0:
						categories[i] = "NORMAL NAME";
						names[i] = HumanNameFormat($"{conditioncondition}{answers[0]}{mancondition}");
						break;

					case 1:
						categories[i] = "OCCUPATIONAL NAME";
						dice = RollD(4);
						namepart = dice switch
						{
							1 => answers[14],
							2 => answers[5],
							3 => answers[12],
							4 => answers[15],
							_ => namepart
						};

						names[i] = HumanNameFormat($"{conditioncondition}{namepart} {answers[1]}{mancondition}");
						break;

					case 2:
						categories[i] = "HORNY NAME";
						dice = RollD(4);
						namepart = dice switch
						{
							1 => answers[11],
							2 => "Naked",
							3 => answers[5],
							4 => answers[13],
							_ => namepart
						};
						names[i] = HumanNameFormat($"{conditioncondition}{namepart} {answers[2]}{mancondition}");
						break;

					case 3:
						categories[i] = "THE NAME";
						dice = RollD(4);
						namepart = dice switch
						{
							1 => answers[7],
							2 => answers[8],
							3 => answers[3],
							4 => answers[19],
							_ => namepart
						};
						names[i] = HumanNameFormat($"The {conditioncondition}{namepart}{mancondition}");
						break;

					case 4:
						categories[i] = "COOL NAME";
						dice = RollD(6);
						namepart = dice switch
						{
							1 => answers[16],
							2 => answers[17],
							3 => answers[18],
							4 => answers[5],
							5 => answers[7],
							6 => answers[12],
							_ => namepart
						};
						names[i] = HumanNameFormat($"{conditioncondition}{answers[20]} {namepart}{mancondition}");
						break;

					case 5:
						categories[i] = "VIOLENT NAME";
						dice = RollD(4);
						namepart = dice switch
						{
							1 => answers[18],
							2 => answers[11],
							3 => answers[19],
							4 => answers[8],
							_ => namepart
						};
						names[i] = HumanNameFormat($"{conditioncondition}{namepart} {answers[4]}{mancondition}");
						break;

					case 6:
						categories[i] = "NAME THAT LACKS SUBTEXT";
						names[i] = HumanNameFormat($"{conditioncondition}{answers[9]}{mancondition}");
						break;
				}
				if (clonecondition[i])
					names[i] += " (clone)";
				if (!kojimacondition[i])
					continue;
				clonecondition[i] = false;
				names[i] = "Hideo Kojima";
			}

			embedtitle = "\nYou have 1 name.";
			if (alternatenames)
				embedtitle = "\nYou have 1 name + 6 other alternate names.";

			embedmsg = $"**Your {(alternatenames ? "true " : "")}name:**\n" +
				$"{categories[truename]}: {HumanNameFormat(names[truename])}\n\n" +
				"**Alternate names:**\n";

			for (var i = 0; i < 7; i++)
			{
				if (i == truename)
					continue;
				embedmsg += $"{categories[i]}: {HumanNameFormat(names[i])}\n";
			}

			if (clonecondition.Contains(true) && (alternatenames || clonecondition[truename]))
				embedmsg += "\nYou have a **clone** name. That means you're a clone of someone else, " +
							"or you have been brainwashed into becoming a mental doppelganger of someone else.\n" +
							"Find someone who has completed this worksheet and replace 50% " +
							"of your Kojima name with 50% of their Kojima name.";

			if (kojimacondition.Contains(true) && (alternatenames || kojimacondition[truename]))
				embedmsg += "\nOh no. You are **Hideo Kojima**. Hideo Kojima created you and is also you. " +
							"You are the man who created himself and there is nothing you can do about it. " +
							"You’re in Kojima’s world — your world — and that’s just the breaks, pal. " +
							"You’re Hideo Kojima. Go do the things that Hideo Kojima does.";

			embed = new DiscordEmbedBuilder
			{
				Title = "KOJIMA NAME GENERATOR" + embedtitle,
				Description = embedmsg,
				Color = new DiscordColor(0xDA0050),
				Footer = new DiscordEmbedBuilder.EmbedFooter
				{
					Text = "Created by Brian David Gilbert at Polygon."
				}
			};
			await questionmsg.ModifyAsync(embed: embed.Build());

			if (ctx.Guild != null)
				await ctx.Message.DeleteAsync();
		}
	}
}
