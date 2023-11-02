﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.BotABordelV2.Extensions;
internal static class ConfigurationExtensions
{
    public static string Retrieve(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<string>(key)
            ?? throw new KeyNotFoundException();
    }

    public static T Retrieve<T>(this IConfiguration configuration, string key)
    {
        return configuration.GetValue<T>(key)
            ?? throw new KeyNotFoundException();
    }
}
