namespace airbnbmonitor.Code.Interfaces
{
    using System;

    public interface ICacheService
    {
        T Get<T>(string key, Func<T> getItemCallback) where T : class;
    }
}
