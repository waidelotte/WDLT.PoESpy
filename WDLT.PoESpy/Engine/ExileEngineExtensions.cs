using System;
using System.Text;
using System.Windows;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Engine
{
    public static class ExileEngineExtensions
    {
        public static void ClipboardSetWhisper(this POEFetchResult fetch)
        {
            Clipboard.SetText(fetch.Listing.Whisper);
        }

        public static void ClipboardSetItem(this POEFetchResult fetch)
        {
            if (!string.IsNullOrWhiteSpace(fetch?.Item?.Extended?.Text))
            {
                var data = Convert.FromBase64String(fetch.Item.Extended.Text);
                Clipboard.SetText(Encoding.UTF8.GetString(data));
            }
        }
    }
}