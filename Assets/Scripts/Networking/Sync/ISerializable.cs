﻿namespace Networking
{
    public interface ISerializable
    {
        object Serialize(bool fullSync);
        void Deserialize(bool fullSync, object data);
    }
}