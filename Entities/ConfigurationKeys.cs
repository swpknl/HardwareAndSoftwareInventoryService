﻿namespace Entities
{
    using System;

    /// <summary>
    /// The configuration keys.
    /// </summary>
    public static class ConfigurationKeys
    {
        public static string DbUserName { get; set; }

        public static string DbPassword { get; set; }

        public static TimeSpan BaseTime { get; set; }

        public static TimeSpan TimeSpan { get; set; }

        public static string[] DirectoriesToExclude { get; set; }

        public static string[] FileTypesToMonitor { get; set; }
    }
}