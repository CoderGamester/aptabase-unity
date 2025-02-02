namespace AptabaseSDK
{
    public interface IDispatcher
    {
        public void Enqueue(Event data);

        public Task Flush();
        
        public void FlushOrSaveToDisk();
    }
}