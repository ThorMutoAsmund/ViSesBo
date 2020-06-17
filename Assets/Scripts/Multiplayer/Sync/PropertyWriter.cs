using Networking;
using System.ComponentModel;
using System.Reflection;

namespace Multiplayer
{
    /// <summary>
    /// Class that wraps a property and implements ISyncable so the property can be synced in a multiplayer setting
    /// </summary>
    public class PropertyWriter : ISerializable
    {
        /// <summary>
        /// The target object that the property velongs to
        /// </summary>
        public object Target { get; private set; }

        /// <summary>
        /// Info about the property to read and write from
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Constructor is private. Use the static Create method instead
        /// </summary>
        private PropertyWriter() { }

        /// <summary>
        /// Create an instance given a target and property info for the property to read and write from
        /// </summary>
        /// <param name="target"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static ISerializable Create(object target, PropertyInfo propertyInfo)
        {
            var result = new PropertyWriter()
            {
                PropertyInfo = propertyInfo,
                Target = target
            };

            return result;
        }

        /// <summary>
        /// Set the property value to the given object
        /// </summary>
        /// <param name="value"></param>
        public void Deserialize(bool _, object value)
        {
            this.PropertyInfo.SetValue(this.Target, value);
        }

        /// <summary>
        /// Return the property value as an object
        /// </summary>
        /// <returns></returns>
        public object Serialize(bool _)
        {
            return this.PropertyInfo.GetValue(this.Target);
        }
    }
}