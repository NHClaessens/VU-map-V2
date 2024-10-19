using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableList<T> : List<T>, ISerializationCallbackReceiver {
    [SerializeField] private List<T> backer = new List<T>();

    public void OnBeforeSerialize() {
        backer.Clear();

        foreach(T value in this) {
            backer.Add(value);
        }
    }

    public void OnAfterDeserialize() {
        Clear();

        foreach(T value in backer) {
            Add(value);
        }
    }
}
