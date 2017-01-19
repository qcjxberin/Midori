﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MidoriBot.Common.Preconditions;
using MidoriBot.Common;
using System.Text;

namespace MidoriBot.Modules
{
    [Name("Help"), Group]
    public sealed class midori_HelpCommand : ModuleBase
    {
        private readonly CommandService MidoriCommands;
        private readonly IDependencyMap MidoriDeps;

        public midori_HelpCommand(CommandService _MidoriCommands, IDependencyMap _MidoriDeps)
        {
            if (_MidoriCommands == null) throw new ArgumentNullException(nameof(_MidoriCommands));

            MidoriCommands = _MidoriCommands;
            MidoriDeps = _MidoriDeps;
        }

        [Command("Help"), Summary("View all my commands!"), Hidden, MinPermissions(AccessLevel.User)]
        public async Task CommandHelp()
        {
            IEnumerable<IGrouping<string, CommandInfo>> CommandGroups = (await MidoriCommands.Commands.CheckConditions(Context, MidoriDeps))
                .Where(c => !c.Preconditions.Any(p => p is HiddenAttribute))
                .GroupBy(c => (c.Module.IsSubmodule ? c.Module.Parent.Name : c.Module.Name));

            NormalEmbed HelpEmbed = new NormalEmbed();
            StringBuilder HEDesc = new StringBuilder();

            HelpEmbed.Title = "My Commands";
            HelpEmbed.ThumbnailUrl = Context.Client.CurrentUser.AvatarUrl;
            HEDesc.AppendLine("**You can use the following commands:**");

            foreach (IGrouping<string, CommandInfo> Group in CommandGroups)
            {
                HEDesc.AppendLine($"**{Group.Key}**:");
                foreach (CommandInfo Command in Group)
                {
                    HEDesc.AppendLine($"• `{Command.Name}`: {Command.Summary}");
                }
            }
            HEDesc.AppendLine($"\nYou can use `{MidoriConfig.CommandPrefix}Help <command>` for more information on that command.");

            HelpEmbed.Description = HEDesc.ToString();
            await (Context.User.CreateDMChannelAsync().Result).SendEmbedAsync(HelpEmbed);
            await Context.Channel.SendMessageAsync($"{Context.User.Mention}, help sent to your Direct Messages!");
        }

        [Command("Help"), Summary("Shows summary for a command."), Hidden]
        public async Task SpecificHelp(string cmdname)
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<CommandInfo> Commands = (await MidoriCommands.Commands.CheckConditions(Context, MidoriDeps))
                .Where(c => (c.Aliases.FirstOrDefault().Equals(cmdname, StringComparison.OrdinalIgnoreCase) ||
                (c.Module.IsSubmodule ? c.Module.Aliases.FirstOrDefault().Equals(cmdname, StringComparison.OrdinalIgnoreCase) : false))
                    && !c.Preconditions.Any(p => p is HiddenAttribute));

            if (Commands.Any())
            {
                sb.AppendLine($"{Commands.Count()} {(Commands.Count() > 1 ? $"entries" : "entry")} for `{cmdname}`");

                foreach (CommandInfo Command in Commands)
                {
                    sb.AppendLine($"**Summary**");
                    sb.AppendLine($"{Command.Summary ?? "No summary :C"}");
                    sb.AppendLine("**Usage**");
                    sb.AppendLine($"{MidoriConfig.CommandPrefix}{(Command.Module.IsSubmodule ? $"{Command.Module.Name} " : "")}{Command.Name} " + string.Join(" ", Command.Parameters.Select(p => formatParam(p))).Replace("`", ""));
                    sb.AppendLine();
                }
            }
            else
            {
                await ReplyAsync($"I couldn't find any command matching \"{cmdname}\". :(");
                return;
            }
            await ReplyAsync(sb.ToString());
        }

        private string formatParam(ParameterInfo param)
        {
            var sb = new StringBuilder();
            if (param.IsMultiple)
            {
                sb.Append($"`[({param.Type.Name}): {param.Name}...]`");
            }
            else if (param.IsRemainder)
            {
                sb.Append($"`<({param.Type.Name}): {param.Name}...>`");
            }
            else if (param.IsOptional)
            {
                sb.Append($"`[({param.Type.Name}): {param.Name}]`");
            }
            else
            {
                sb.Append($"`<({param.Type.Name}): {param.Name}>`");
            }

            if (!string.IsNullOrWhiteSpace(param.Summary))
            {
                sb.Append($" ({param.Summary})");
            }
            return sb.ToString();
        }
    }
}
