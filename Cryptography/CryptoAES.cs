using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace leetbot_night.Cryptography
{
    [Group("aes"),
     Description("provides symmetric AES message encryption and decryption.")]
    public class CryptoAes : BaseCommandModule
    {
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Check `!help aes` for command usage.");
        }

        public static string Encrypt(string key, string text)
        {
            using var aesAlg = new AesManaged();
            try
            {
                aesAlg.IV = Convert.FromBase64String(BotMain.AesIv);
            }
            catch (FormatException)
            {
                return ":x:AES IV in config file " +
                       "is not a valid base64 string.\n" +
                       "Use `!aes keypairgen` to generate one.";
            }
            try
            {
                aesAlg.Key = Convert.FromBase64String(key);
            }
            catch (FormatException)
            {
                return ":x:Entered key is not a valid base64 string.\n" +
                       "Use `!aes keygen` to generate one.";
            }
            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            // Create the streams used for encryption.
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);   //Write all data to the stream.
            }
            byte[] encrypted = msEncrypt.ToArray();
            string encryptedbase64Txt = Convert.ToBase64String(encrypted);
            return encryptedbase64Txt.Length > 1940 ? ":x:Your message is too long to encrypt or decrypt in discord." : encryptedbase64Txt;
        }

        public static string Decrypt(string key, string encryptedtxt)
        {
            string decrypted;
            byte[] cryptoBytes;
            try
            {
                cryptoBytes = Convert.FromBase64String(encryptedtxt);
            }
            catch (FormatException)
            {
                return ":x:Entered string is not a valid base64 string.";
            }
            using var aesAlg = new AesManaged();
            try
            {
                aesAlg.IV = Convert.FromBase64String(BotMain.AesIv);
            }
            catch (FormatException)
            {
                return ":x:AES IV in config file " +
                       "is not a valid base64 string.\n" +
                       "Use `!aes keypairgen` to generate one.";
            }
            try
            {
                aesAlg.Key = Convert.FromBase64String(key);
            }
            catch (FormatException)
            {
                return ":x:Entered key is not a valid base64 string.\n" +
                       "Use `!aes keygen` to generate one.";
            }
            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            // Create the streams used for decryption.
            using var msDecrypt = new MemoryStream(cryptoBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            // Read the decrypted bytes from the decrypting stream and place them in a string.
            try
            {
                decrypted = srDecrypt.ReadToEnd();
            }
            catch (CryptographicException cex)
            {
                decrypted = $":x:Cryptographic error: {cex.Message}";
            }
            return decrypted;
        }

        public static string GenerateKey()
        {
            var aesGen = new AesManaged();
            return Convert.ToBase64String(aesGen.Key);
        }

        [Command("keygen"),
         Description("generates a 256-bit key in base64.")]
        public async Task GenKey(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = GenerateKey(),
                Title = ":key: Your Key",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 256-bit Encryption Key",
                    IconUrl = null
                }
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("keypairgen"),
         Description("generates a pair of 256-bit key " +
                     "and initialization vector in base64.")]
        public async Task GenKeyIvPair(CommandContext ctx)
        {
            var aesGen = new AesManaged();
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = $"Key: `{Convert.ToBase64String(aesGen.Key)}`\n" +
                              $"IV: `{Convert.ToBase64String(aesGen.IV)}`",
                Title = ":closed_lock_with_key: Your Key/IV pair",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"🔒 {aesGen.KeySize}-bit Encryption Key and IV",
                    IconUrl = null
                }
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("qencrypt"),
         Description("(quick) encrypts your message using AES-256 encryption.")]
        public async Task MsgEncrypt(CommandContext ctx,
            [Description("your message to encrypt."), RemainingText] string msg = "")
        {
            if (msg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = Encrypt(BotMain.AesKey, msg),
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 AES-256 QuickEncrypted",
                    IconUrl = null
                }
            };
            if (ctx.Guild != null)
                await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: embed);
        }

        [Command("qdecrypt"),
         Description("decrypts AES-256 QuickEncrypted messages.")]
        public async Task MsgDecrypt(CommandContext ctx,
            [Description("encrypted message."), RemainingText] string encryptedmsg = "")
        {
            if (encryptedmsg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = Decrypt(BotMain.AesKey, encryptedmsg),
                Title = ":unlock: Decrypted message"
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("encrypt"),
         Description("encrypts your message using your custom 256-bit encryption key.")]
        public async Task MsgKeyEncrypt(CommandContext ctx,
            [Description("encryption key.")] string key,
            [Description("your message to encrypt."), RemainingText] string msg = "")
        {
            if (msg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = Encrypt(key, msg),
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 AES-256 Encryption",
                    IconUrl = null
                }
            };
            if (ctx.Guild != null)
                await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: embed);
        }

        [Command("encryptdm"),
         Description("encrypts your message with a generated 256-bit encryption key. " +
                     "Sends encryption key in your Discord DMs.")]
        public async Task MsgFastKeyEncrypt(CommandContext ctx,
            [Description("your message to encrypt."), RemainingText] string msg = "")
        {
            if (msg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            string generatedkey = GenerateKey();
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = generatedkey,
                Title = ":key: Your Key",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 256-bit Encryption Key",
                    IconUrl = null
                }
            };
            if (ctx.Guild != null)
                await ctx.Member.SendMessageAsync(embed: embed);
            else
                await ctx.RespondAsync(embed: embed);
            embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = Encrypt(generatedkey, msg),
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 AES-256 Encryption",
                    IconUrl = null
                }
            };
            if (ctx.Guild != null)
                await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: embed);
        }

        [Command("decrypt"),
         Description("decrypts message with a custom 256-bit encryption key.")]
        public async Task MsgKeyDecrypt(CommandContext ctx,
            [Description("encryption key.")] string key,
            [Description("encrypted message."), RemainingText] string encryptedmsg = "")
        {
            if (encryptedmsg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x458056),
                Description = Decrypt(key, encryptedmsg),
                Title = ":unlock: Decrypted message"
            };
            await ctx.RespondAsync(embed: embed);
        }
    }
}
