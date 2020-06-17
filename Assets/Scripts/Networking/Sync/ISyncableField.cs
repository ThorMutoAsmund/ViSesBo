using System.ComponentModel;

namespace Networking
{
    /// <summary>
    /// Interfaces that merges INotifyPropertyChanged and ISyncable
    /// </summary>
    public interface ISyncableField : INotifyPropertyChanged, ISerializable { }

    /// <summary>
    /// Interface that allows a class to expose a syncable field to the public so they can subscribe to changes and read value
    /// </summary>
    public interface ISyncedProperty<TValue> : INotifyPropertyChanged
    {        
        TValue Value { get; }   
    }
}