using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WDLT.Clients.POE;
using WDLT.Clients.POE.Enums;
using WDLT.Clients.POE.Exception;
using WDLT.Clients.POE.Models;
using WDLT.PoESpy.Engine.Events;

namespace WDLT.PoESpy.Engine
{
    public class ExileEngine
    {
        public event EventHandler<string> OnMessageEvent;
        public event EventHandler<ExileRateLimitArgs> OnRateLimitEvent;

        private readonly ConcurrentDictionary<string, DateTimeOffset> _rateLimits;
        private readonly POEClient _client;

        public ExileEngine(string userAgent)
        {
            _client = new POEClient(userAgent);
            _rateLimits = new ConcurrentDictionary<string, DateTimeOffset>();
        }

        public void SetSession(string id)
        {
            _client.POESESSID = id;
        }

        public string GetSession()
        {
            return _client.POESESSID;
        }

        public Task<POEResult<List<POEStatic>>> TradeStaticAsync()
        {
            return CanBeExceptionAsync(() => _client.TradeStaticAsync(), "api-trade-static");
        }

        public Task<POEResult<List<POELeague>>> TradeLeaguesAsync()
        {
            return CanBeExceptionAsync(() => _client.TradeLeaguesAsync(), "api-trade-leagues");
        }

        public Task<POESearchResult> SearchAsync(string league, POESearchPayload payload)
        {
            return CanBeExceptionAsync(() => _client.TradeSearchAsync(league, payload), "api-trade-search");
        }

        public Task<List<POECharacter>> Characters(string account)
        {
            return CanBeExceptionAsync(() => _client.Characters(account), "api-characters");
        }

        public Task DownloadImageAsync(string path, string savePath)
        {
            return _client.DownloadAsync(new Uri(POEClient.CDN + path), savePath);
        }

        public Task<POEAccountName> AccountNameByCharacter(string character)
        {
            return CanBeExceptionAsync(() => _client.AccountNameByCharacter(character), "api-accbychar");
        }

        public Task<POESearchResult> SearchByAccountAsync(string account, string league, EPOESort sort, EPOEOnlineStatus online)
        {
            return SearchAsync(league, new POESearchPayload
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
                _rateLimits.AddOrUpdate(endpoint, rt.BanUntil, (s, o) => rt.BanUntil);

                OnRateLimitEvent?.Invoke(this, new ExileRateLimitArgs(rt, endpoint));
            }
        }

        private async Task<T> CanBeExceptionAsync<T>(Func<Task<T>> task, string endpoint)
        {
            if (_rateLimits.TryGetValue(endpoint, out var limit))
            {
                if (limit >= DateTimeOffset.Now)
                {
                    Log("[Rate-Limit-Guard] Try again later");
                    return default;
                }
                else
                {
                    _rateLimits.TryRemove(endpoint, out _);
                }
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
                    Log($"[Rate-Limit][{endpoint}] Exceeded");
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

                Log(text);
                return default;
            }
            catch (Exception e)
            {
                Log("Error: " + e.Message);
                return default;
            }
        }

        private void Log(string message)
        {
            OnMessageEvent?.Invoke(this, message);
        }
    }
}