using ProtoBuf;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public abstract class ProtoSerializeBaseMono : MonoBehaviour, ISerializationCallbackReceiver
{
    [System.Serializable]
    public class SerializeData
    {
        public byte[] Content;

        public SerializeData(byte[] data)
        {
            this.Content = data;
        }

        public static implicit operator SerializeData(byte[] data)
        {
            return new SerializeData(data);
        }
    }

    [SerializeField, HideInInspector]
    private List<Object> referencens = new List<Object>();

    [SerializeField, HideInInspector]
    private List<SerializeData> data = new List<SerializeData>();


    public void OnAfterDeserialize()
    {
        this.DeserializeObjects();
    }

    public void OnBeforeSerialize()
    {
        this.referencens.Clear();
        this.data.Clear();
        this.SerializeObjects();
    }

    protected abstract void SerializeObjects();
    protected abstract void DeserializeObjects();

    protected void SerializeObject<T>(T obj)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            Serializer.Serialize<T>(stream, obj, this.referencens);
            this.data.Add(stream.ToArray());
        }
    
    }

    protected T DeserializeObject<T>(T def)
    {
        if(this.data.Count <= 0)
        {
            return def;
        }

        using (MemoryStream stream = new MemoryStream(this.data[0].Content))
        {
            this.data.RemoveAt(0);
            T result = Serializer.Deserialize<T>(stream, this.referencens);
            return result != null ? result : def;
        }
    }
}
