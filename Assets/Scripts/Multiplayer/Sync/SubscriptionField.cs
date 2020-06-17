using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Multiplayer
{
    /// <summary>
    /// Class that allows a field to be automatically synchronized in a multiplayer setting
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SubscriptionField<TValue> : ISyncableField, ISyncedProperty<TValue>
    {
        /// <summary>
        /// Name of the synced field
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Property change event handler
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The inner value being synced
        /// </summary>
        public TValue Value
        {
            get => this.value;
            set
            {
                if (!EqualityComparer<TValue>.Default.Equals(this.value, value))
                {
                    this.value = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(this.Name));
                }
            }
        }

        /// <summary>
        /// Cast the object into the inner field value
        /// </summary>
        /// <param name="value"></param>
        public void Deserialize(bool _, object value)
        {
            this.Value = (TValue)value;
        }

        /// <summary>
        /// Return the inner field value as an object
        /// </summary>
        /// <returns></returns>
        public object Serialize(bool _) => this.Value;

        /// <summary>
        /// Actual value
        /// </summary>
        private TValue value;

        /// <summary>
        /// Private constructor. This class must be instantiated through one of the static constructors
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private SubscriptionField(string name, TValue value = default)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Create a synced property with a property name and the given initial value
        /// </summary>
        /// <param name="input"></param>
        public static SubscriptionField<TValue> Create(string name, TValue value = default)
        {
            return new SubscriptionField<TValue>(name, value);
        }

        /// <summary>
        /// Operator overload that returns the value of the synced property
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator TValue(SubscriptionField<TValue> value) => value.Value;

        /// <summary>
        /// Implicit operator overload that creates a synced property with a property name and default initial value
        /// </summary>
        /// <param name="name"></param>
        public static implicit operator SubscriptionField<TValue>(string name) => Create(name);

        /// <summary>
        /// Implicit operator overload that creates a synced property with a property name and the given initial value
        /// </summary>
        /// <param name="input"></param>
        public static implicit operator SubscriptionField<TValue>((string Name, TValue Value) input) => Create(input.Name, value: input.Value);
    }
}