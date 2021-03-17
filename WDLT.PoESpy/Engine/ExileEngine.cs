using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Swordfish.NET.Collections;
using WDLT.Clients.POE;
using WDLT.Clients.POE.Enums;
using WDLT.Clients.POE.Exception;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Helpers;
using WDLT.PoESpy.Properties;
using WDLT.PoESpy.Services;

namespace WDLT.PoESpy.Engine
{
    public class ExileEngine
    {
        public ConcurrentObservableCollection<RateLimitTimer> RateLimits { get; }
        public List<POELeague> Leagues { get; private set; }
        public List<POEStatic> Static { get; private set; }

        private readonly POEClient _client;
        private readonly ISnackbarMessageQueue _snackbarMessageQueue;

        public ExileEngine(ISnackbarMessageQueue snackbarMessageQueue)
        {
            _snackbarMessageQueue = snackbarMessageQueue;
            _client = new POEClient(Settings.Default.UserAgent);
            RateLimits = new ConcurrentObservableCollection<RateLimitTimer>();
        }

        public async Task<bool> InitAsync()
        {
            var leagues = await CanBeExceptionAsync(() => _client.TradeLeaguesAsync(), "api-trade-leagues");
            if(leagues == null) return false;
            Leagues = leagues.Result;

            var st = await CanBeExceptionAsync(() => _client.TradeStaticAsync(), "api-trade-static");
            if (st == null) return false;

            Static = st.Result;

            ImageCacheService.CreateDirectories();
            foreach (var currency in Static.SelectMany(s => s.Entries).Where(w => !string.IsNullOrWhiteSpace(w.Image)))
            {
                if(ImageCacheService.Exist(currency.Id)) continue;
                await _client.DownloadAsync(new Uri(POEClient.CDN + currency.Image), ImageCacheService.Get(currency.Id));
            }

            return true;
        }

        public void SetSession(string id)
        {
            _client.POESESSID = id;
        }

        public Task<POESearchResult> SearchAsync(POESearchPayload payload)
        {
            return CanBeExceptionAsync(() => _client.TradeSearchAsync(Settings.Default.League, payload), "api-trade-search");
        }

        public Task<List<POECharacter>> Characters(string account)
        {
            return CanBeExceptionAsync(() => _client.Characters(account), "api-characters");
        }

        public Task<POEAccountName> AccountNameByCharacter(string character)
        {
            return CanBeExceptionAsync(() => _client.AccountNameByCharacter(character), "api-accbychar");
        }

        public Task<POESearchResult> SearchByAccountAsync(string account, EPOESort sort, EPOEOnlineStatus online)
        {
            return SearchAsync(new POESearchPayload
            {
                Sort = new POESearchSort
                {
                    Price = sort
                },
                Query =
                {
                    Status = { Option = online },
                    Filters = new POESearchFilters
                    {
                        TradeFilters = new POESearchTradeFilters
                        {
                            Filters = new POESearchFilter
                            {
                                Account = new POESearchAccountFilter
                                {
                                    Input = account
                                }
                            }
                        }
                    }
                }
            });
        }

        public async Task<bool> AccountExistAsync(string account)
        {
            try
            {
                await _client.AccountPinsAsync(account, 1);
                return true;
            }
            catch (POEException)
            {
                return false;
            }
        }

        public Task<POEResult<List<POEFetchResult>>> FetchAsync(IEnumerable<string> pageIds)
        {
            return CanBeExceptionAsync(() => _client.TradeFetchAsync(pageIds), "api-trade-fetch");
        }

        private void AddRateLimits(IEnumerable<POERateLimit> limits, string endpoint)
        {
            foreach (var rt in limits)
            {
                var exist = RateLimits.FirstOrDefault(f =>
                    f.Endpoint == endpoint && f.RateLimit.Type == rt.Type &&
                    f.RateLimit.Window == rt.Window);

                if (exist == null)
                {
                    RateLimits.Add(new RateLimitTimer(rt, endpoint, t => RateLimits.Remove(t)));
                }
                else
                {
                    exist.Limit = TimeSpan.FromSeconds(rt.Ban);
                }
            }
        }

        private async Task<T> CanBeExceptionAsync<T>(Func<Task<T>> task, string endpoint)
        {
            if (RateLimits.Any(a => a.Endpoint == endpoint))
            {
                _snackbarMessageQueue.Enqueue("[Rate-Limit-Guard] Try again later", null, null, null, false, true, TimeSpan.FromSeconds(3));
                return default;
            }

            try
            {
                return await task.Invoke();
            }
            catch (POERateLimitException rl)
            {
                if (rl.RateLimits.Any())
                {
                    AddRateLimits(rl.RateLimits, endpoint);
                }
                else
                {
                    _snackbarMessageQueue.Enqueue($"[Rate-Limit][{endpoint}] Exceeded", null, null, null, false, true, TimeSpan.FromSeconds(3));
                }

                return default;
            }
            catch (POEException p)
            {
                string text;

                switch (p.Error.Code)
                {
                    case 1:
                        text = "Not Found";
                        break;
                    case 6:
                        text = $"Forbidden: {endpoint}";
                        break;
                    default:
                        text = "Error: " + p.Error?.Message;
                        break;
                }

                 _snackbarMessageQueue.Enqueue(text, null, null, null, true, false, TimeSpan.FromSeconds(3));
                 return default;
            }
            catch (Exception e)
            {
                _snackbarMessageQueue.Enqueue("Error: " + e.Message, null, null, null, true, false, TimeSpan.FromSeconds(10));
                return default;
            }
        }
    }
}