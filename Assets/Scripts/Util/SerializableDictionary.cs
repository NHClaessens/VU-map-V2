using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    // Save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // Load dictionary from lists
    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
            throw new Exception("There are unequal numbers of keys and values. Can't deserialize SerializableDictionary.");


        for (int i = 0; i < keys.Count; i++)
            Add(keys[i], values[i]);
    }
}

[Serializable]
public class NewSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<KeyValuePair<TKey, TValue>> keyValuePairs = new List<KeyValuePair<TKey, TValue>>();

    // Save the dictionary to a list of key-value pairs
    public void OnBeforeSerialize()
    {
        keyValuePairs.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keyValuePairs.Add(pair);
        }
    }

    // Load dictionary from the list of key-value pairs
    public void OnAfterDeserialize()
    {
        Clear();

        foreach (KeyValuePair<TKey, TValue> pair in keyValuePairs)
        {
            Add(pair.Key, pair.Value);
        }
    }
}

[Serializable]
public class FloatFloatSerializableDictionary : NewSerializableDictionary<float, float>{}