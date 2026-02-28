namespace CefDotnetApp.AgentCore.Core
{
    public enum SearchEngineType
    {
        None,
        Brave,
        SearXNG
    }

    /// <summary>
    /// Routes web_search / web_search_raw calls to the currently active search engine.
    /// Setting a Brave API key activates Brave; setting a SearXNG URL activates SearXNG.
    /// The last configured engine wins.
    /// </summary>
    public class WebSearchRouter
    {
        private readonly BraveSearchService _brave;
        private readonly SearXNGSearchService _searxng;

        public SearchEngineType ActiveEngine { get; private set; } = SearchEngineType.None;

        public WebSearchRouter(BraveSearchService brave, SearXNGSearchService searxng)
        {
            _brave = brave;
            _searxng = searxng;
        }

        public void ActivateBrave()
        {
            ActiveEngine = SearchEngineType.Brave;
        }

        public void ActivateSearXNG()
        {
            ActiveEngine = SearchEngineType.SearXNG;
        }

        public string Search(string query, int count = 5)
        {
            switch (ActiveEngine) {
                case SearchEngineType.Brave:
                    return _brave.Search(query, count);
                case SearchEngineType.SearXNG:
                    return _searxng.Search(query, count);
                default:
                    return "[error] No search engine configured. Use brave_set_api_key() or searxng_set_url() first.";
            }
        }

        public string SearchRaw(string query, int count = 5)
        {
            switch (ActiveEngine) {
                case SearchEngineType.Brave:
                    return _brave.SearchRaw(query, count);
                case SearchEngineType.SearXNG:
                    return _searxng.SearchRaw(query, count);
                default:
                    return "[error] No search engine configured. Use brave_set_api_key() or searxng_set_url() first.";
            }
        }
    }
}
