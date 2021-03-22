using System;
using System.Diagnostics.CodeAnalysis;

namespace WDLT.PoESpy.Engine.Models
{
    public class ExileTradeWebSocketSetting
    {
        public string League { get; }
        public string Query { get; }
        public string Name { get; }

        public ExileTradeWebSocketSetting([NotNull] string league, [NotNull] string query, [NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(league)) throw new ArgumentNullException(nameof(league));
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentNullException(nameof(query));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            League = league.Trim();
            Query = query.Trim();
            Name = name.Trim();
        }
    }
}