using System.Text;
using Newtonsoft.Json;

namespace Kyuutie.api.twitch
{
    internal class TwitchApi
    {
        private readonly string _authToken;
        private readonly string _clientId;
        private readonly HttpClient _client;
        public TwitchApi(string clientId, string authToken)
        {
            this._authToken = authToken;
            this._clientId = clientId;
            this._client = new HttpClient();
            this._client.DefaultRequestHeaders.Add("Authorization", $"Bearer {this._authToken}"); 
            this._client.DefaultRequestHeaders.Add("Client-Id", this._clientId); 
        }

        private async Task<string> MakeRequestAsync(string url, HttpMethod method, string application = "")
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = method,
                    RequestUri = new Uri($"https://api.twitch.tv/helix/{url}"),
                    Content = new StringContent(application, Encoding.UTF8, "application/json") 
                };
                HttpResponseMessage response = this._client.SendAsync(request).Result;
                return response.Content.ReadAsStringAsync().Result; 
            } catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null; 
        }
        public async Task<SearchChannels> SearchChannelsAsync(string channelName)
        {
            SearchChannelsResponse response = JsonConvert.DeserializeObject <SearchChannelsResponse> (await MakeRequestAsync($"search/channels?query={channelName.ToLower()}", HttpMethod.Get))!;
            if(response is not null)
            {
                foreach(SearchChannels channel in response.data) 
                {
                    if (channel.broadcaster_login.ToLower() == channelName.ToLower() | channel.display_name.ToLower() == channelName.ToLower())
                    {
                        return channel;
                    }
                }
            }

            return null;
        }
        

        public async Task<ChannelInformation?> GetChannelInformationAsync(string channelId)
        {
            ChannelInformationResponse? response = JsonConvert.DeserializeObject<ChannelInformationResponse>(await MakeRequestAsync($"channels?broadcaster_id={channelId}", HttpMethod.Get));
            return response?.data[0];
        }
        
        
    }
    
    // ReSharper disable InconsistentNaming
    internal class ChannelInformationResponse
    {

        public List<ChannelInformation?> data { get; set; }
    }
    public class ChannelInformation
    {

        public string broadcaster_id { get; set; }
        public string broadcaster_login { get; set; }
        public string broadcaster_name { get; set; }
        public string broadcaster_language { get; set; }
        public string game_id { get; set; }
        public string game_name { get; set; }
        public string title { get; set; }
        public int delay { get; set; }
    }
    internal class SearchChannelsResponse
    {
        public List<SearchChannels> data { get; set; }
    }
    public class SearchChannels
    {
        public string broadcaster_language { get; set; }
        public string broadcaster_login { get; set; }
        public string display_name { get; set; }
        public string game_id { get; set; }
        public string game_name { get; set; }
        public string id { get; set; }
        public bool is_live { get; set; }
        public List<object> tags_ids { get; set; }
        public string thumbnail_url { get; set; }
        public string title { get; set; }
        public string started_at { get; set; }
    }

    
    // ReSharper enable InconsistentNaming
}
