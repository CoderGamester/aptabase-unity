using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AptabaseSDK.TinyJson;
using UnityEngine;

namespace AptabaseSDK
{
    public class WebGLDispatcher: IDispatcher
    {
        private const string EVENT_ENDPOINT = "/api/v0/event";
        private const string APTDABASE_KEY = "AptabaseKey";
        
        private static string _apiURL;
        private static WebRequestHelper _webRequestHelper;
        private static string _appKey;
        private static EnvironmentInfo _environment;
        
        private bool _flushInProgress;
        private readonly Queue<Event> _events;
        
        public WebGLDispatcher(string appKey, string baseURL, EnvironmentInfo env)
        {
            //create event queue
            _events = new Queue<Event>();
            
            //web request setup information
            _apiURL = $"{baseURL}{EVENT_ENDPOINT}";
            _appKey = appKey;
            _environment = env;
            _webRequestHelper = new WebRequestHelper();
        }
        
        public void Enqueue(Event data)
        {
            _events.Enqueue(data);
        }
        
        private void Enqueue(List<Event> data)
        {
            foreach (var eventData in data)
                _events.Enqueue(eventData);
        }

        public async Task Flush()
        {
            if (_flushInProgress || _events.Count <= 0)
                return;

            _flushInProgress = true;
            var failedEvents = new List<Event>();
            
            //flush all events
            do
            {
                var eventToSend = _events.Dequeue();
                try
                {
                    var result = await SendEvent(eventToSend);
                    if (!result) failedEvents.Add(eventToSend);
                }
                catch
                {
                    failedEvents.Add(eventToSend);
                }

            } while (_events.Count > 0);
            
            if (failedEvents.Count > 0) 
                Enqueue(failedEvents);

            _flushInProgress = false;
        }

        public async void FlushOrSaveToDisk()
        {
            await Flush();
            
            PlayerPrefs.SetString(APTDABASE_KEY, _events.ToList().ToJson());
        }
        
        private static async Task<bool> SendEvent(Event eventData)
        {
            var webRequest = _webRequestHelper.CreateWebRequest(_apiURL, _appKey, _environment, eventData.ToJson());
            var result = await _webRequestHelper.SendWebRequestAsync(webRequest);
            return result;
        }
    }
}