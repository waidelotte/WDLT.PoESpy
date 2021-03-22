using System;
using System.Text;
using WDLT.Clients.POE.Models;

namespace WDLT.PoESpy.Engine
{
    public static class ExileEngineExtensions
    {
        public static string WhisperText(this POEFetchResult fetch)
        {
            return fetch.Listing.Whisper;
        }

        public static string RawItemText(this POEFetchResult fetch)
        {
            if (!string.IsNullOrWhiteSpace(fetch?.Item?.Extended?.Text))
            {
                var data = Convert.FromBase64String(fetch.Item.Extended.Text);
                return Encoding.UTF8.GetString(data);
            }
            else
            {
                return null;
            }
        }
    }
}