using System.Collections.Generic;
using ProtoBuf;


public static class ProtoSerializeHelper<T> where T:new()
{
    private static List<T> listCache;
    
    public static T CreateInstance(T oldValue)
    {
        if (typeof(T).IsValueType)
        {
            return oldValue;
        }
        return oldValue == null ? new T() : oldValue;
    }

    public static List<T> GetListCache()
    {
        if (listCache == null)
        {
            listCache = new List<T>();
        }
        listCache.Clear();
        return listCache;
    }

}