﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace MidoriBot.Common
{
    public class ErrorEmbed : EmbedBase
    {
        public ErrorEmbed()
        {
            Color = new Color(255, 0, 0);
        }
    }

    public class MEmbedFooter : EmbedFooterBuilder
    {
        public MEmbedFooter(IDiscordClient Client)
        {
            WithIconUrl(Client.CurrentUser.AvatarUrl);
        }
    }

    public class EmbedBase : EmbedBuilder
    {
        public EmbedBase()
        {
            
        }
    }

    public class NormalEmbed : EmbedBase
    {
        public NormalEmbed(byte Red = 51, byte Green = 255, byte Blue = 204)
        {
            Color = new Color(Red, Green, Blue);
        }
    }
}
