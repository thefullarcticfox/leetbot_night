using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace leetbot_night.Cryptography
{
    [Group("rsa"),
     Description("provides asymmetric RSA message encryption and decryption.")]
    public class CryptoRsa : BaseCommandModule
    {
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Check `!help rsa` for command usage.");
        }

        public static string Encrypt(string text, RSAParameters rsaKeyInfo)
        {
            string encryptedbase64Txt;
            // Create a UnicodeEncoder to convert between byte array and string.
            var byteConverter = new UnicodeEncoding();
            byte[] inputdata = byteConverter.GetBytes(text);
            try
            {
                var rsaAlg = RSA.Create(rsaKeyInfo);
                encryptedbase64Txt = Convert.ToBase64String(rsaAlg.Encrypt(inputdata, RSAEncryptionPadding.Pkcs1));
            }
            catch (CryptographicException cex)
            {
                return $":x:Cryptographic error: {cex.Message}";
            }
            return encryptedbase64Txt;
        }

        public static string Decrypt(string encryptedtxt, RSAParameters rsaKeyInfo)
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
            // Create a UnicodeEncoder to convert between byte array and string.
            var byteConverter = new UnicodeEncoding();
            var rsaAlg = RSA.Create(rsaKeyInfo);
            try
            {
                decrypted = byteConverter.GetString(rsaAlg.Decrypt(cryptoBytes, RSAEncryptionPadding.Pkcs1));
            }
            catch (CryptographicException cex)
            {
                return $":x:Cryptographic error: {cex.Message}";
            }
            return decrypted;
        }

        public static string GeneratePrivateKey()
        {
            var rsaGen = RSA.Create(1024);
            return Convert.ToBase64String(rsaGen.ExportPkcs8PrivateKey());
        }

        [Command("publickey"),
         Description("shows public key used in RSA encryption commands.")]
        public async Task GetPublicKey(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = BotMain.RsaPublicKey,
                Title = ":key: Public Key",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 1024-bit Public X.509 Encryption Key",
                    IconUrl = null
                }
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("keygen"),
         Description("generates a 1024-bit private key in base64.")]
        public async Task GenerateKey(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = GeneratePrivateKey(),
                Title = ":key: Your Private Key",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 1024-bit Private PKCS#8 Encryption Key",
                    IconUrl = null
                }
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("keypairgen"),
         Description("generates a pair of public and private keys for RSA-1024 in base64.")]
        public async Task GenerateKeypair(CommandContext ctx)
        {
            var rsaGen = RSA.Create(1024);

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = $"Public X.509 key: `{Convert.ToBase64String(rsaGen.ExportSubjectPublicKeyInfo())}`\n" +
                              $"Private PKCS#8 key: `{Convert.ToBase64String(rsaGen.ExportPkcs8PrivateKey())}`",
                Title = ":closed_lock_with_key: Your Public/Private Keypair",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"🔒 {rsaGen.KeySize}-bit Encryption Keys",
                    IconUrl = null
                }
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("encrypt"),
         Description("encrypts your message (up to 117 bytes long) " +
                     "using your custom private 1024-bit private encryption key.")]
        public async Task MsgCustomKeyEncrypt(CommandContext ctx,
            [Description("private key.")] string privatekey,
            [Description("your message to encrypt."), RemainingText] string msg = "")
        {
            if (msg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var output = "";
            var rsaBase = RSA.Create(1024);
            try
            {
                rsaBase.ImportSubjectPublicKeyInfo(Convert.FromBase64String(BotMain.RsaPublicKey), out _);
            }
            catch (FormatException)
            {
                output += ":x:Public key in config file is not a valid base64 string.\n" +
                          "Use `!rsa keypairgen` to generate one.\n";
            }
            try
            {
                rsaBase.ImportPkcs8PrivateKey(Convert.FromBase64String(privatekey), out _);
            }
            catch (FormatException)
            {
                output += ":x:Entered key is not a valid base64 string.\n" +
                          "Use `!rsa keygen` to generate one.";
            }
            output += Encrypt(msg, rsaBase.ExportParameters(true));
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = output,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 RSA-1024 Encryption",
                    IconUrl = null
                }
            };
            if (ctx.Guild != null) await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: embed);
        }

        [Command("encryptdm"),
         Description("encrypts your message (up to 117 bytes long) " +
                     "with a generated 1024-bit private encryption key. " +
                     "Sends private key in your Discord DMs.")]
        public async Task MsgKeyEncrypt(CommandContext ctx,
            [Description("your message to encrypt."), RemainingText] string msg = "")
        {
            if (msg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var rsaBase = RSA.Create(1024);
            rsaBase.ImportSubjectPublicKeyInfo(Convert.FromBase64String(BotMain.RsaPublicKey), out _);
            rsaBase.ImportPkcs8PrivateKey(Convert.FromBase64String(GeneratePrivateKey()), out _);
            string output = Encrypt(msg, rsaBase.ExportParameters(true));
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = output,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 RSA-1024 Encryption",
                    IconUrl = null
                }
            };

            if (ctx.Guild != null) await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: embed);

            if (!output.Contains(":x:"))
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor(0x6432aa),
                    Description = Convert.ToBase64String(rsaBase.ExportPkcs8PrivateKey()),
                    Title = ":key: Your Key",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "🔒 1024-bit Private PKCS#8 Encryption Key",
                        IconUrl = null
                    }
                };
                if (ctx.Guild != null) await ctx.Member.SendMessageAsync(embed: embed);
                else await ctx.RespondAsync(embed: embed);
            }
        }

        [Command("decrypt"),
         Description("decrypts message with a private key.")]
        public async Task MsgKeyDecrypt(CommandContext ctx,
            [Description("private key.")] string privatekey,
            [Description("encrypted message."), RemainingText] string encryptedmsg = "")
        {
            if (encryptedmsg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var output = "";
            var rsaBase = RSA.Create(1024);
            try
            {
                rsaBase.ImportSubjectPublicKeyInfo(Convert.FromBase64String(BotMain.RsaPublicKey), out _);
            }
            catch (FormatException)
            {
                output += ":x:Public key in config file is not a valid base64 string.\n" +
                          "Use `!rsa keypairgen` to generate one.\n";
            }
            try
            {
                rsaBase.ImportPkcs8PrivateKey(Convert.FromBase64String(privatekey), out _);
            }
            catch (FormatException)
            {
                output += ":x:Entered key is not a valid base64 string.\n" +
                         "Use `!rsa keygen` to generate one.";
            }
            output += Decrypt(encryptedmsg, rsaBase.ExportParameters(true));
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = output,
                Title = ":unlock: Decrypted message"
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("qencrypt"),
         Description("(quick) encrypts your message (up to 117 bytes long) " +
                     "using RSA-1024 encryption.")]
        public async Task MsgEncrypt(CommandContext ctx,
            [Description("your message to encrypt."), RemainingText] string msg = "")
        {
            if (msg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var rsaBase = RSA.Create(1024);
            var output = "";
            try
            {
                rsaBase.ImportSubjectPublicKeyInfo(Convert.FromBase64String(BotMain.RsaPublicKey), out _);
                rsaBase.ImportPkcs8PrivateKey(Convert.FromBase64String(BotMain.RsaPrivateKey), out _);
            }
            catch (FormatException)
            {
                output += ":x:Public and/or private key(s) in config file " +
                          "is not a valid base64 string.\n" +
                          "Use `!rsa keypairgen` to generate them.\n";
            }
            output += Encrypt(msg, rsaBase.ExportParameters(true));
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = output,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.Message.Author is DiscordMember mx ? mx.DisplayName : ctx.Message.Author.Username,
                    IconUrl = ctx.Message.Author.AvatarUrl
                },
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "🔒 RSA-1024 QuickEncrypted",
                    IconUrl = null
                }
            };
            if (ctx.Guild != null)
                await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: embed);
        }

        [Command("qdecrypt"),
         Description("decrypts RSA-1024 QuickEncrypted messages.")]
        public async Task MsgDecrypt(CommandContext ctx,
            [Description("encrypted message."), RemainingText] string encryptedmsg = "")
        {
            if (encryptedmsg.Length == 0)
            {
                await ctx.RespondAsync("You didn't enter the message.");
                return;
            }
            var rsaBase = RSA.Create(1024);
            var output = "";
            try
            {
                rsaBase.ImportSubjectPublicKeyInfo(Convert.FromBase64String(BotMain.RsaPublicKey), out _);
                rsaBase.ImportPkcs8PrivateKey(Convert.FromBase64String(BotMain.RsaPrivateKey), out _);
            }
            catch (FormatException)
            {
                output += ":x:Public and/or private key(s) in config file " +
                          "is not a valid base64 string.\n" +
                          "Use `!rsa keypairgen` to generate them.\n";
            }
            output += Decrypt(encryptedmsg, rsaBase.ExportParameters(true));
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(0x6432aa),
                Description = output,
                Title = ":unlock: Decrypted message"
            };
            await ctx.RespondAsync(embed: embed);
        }
    }
}
