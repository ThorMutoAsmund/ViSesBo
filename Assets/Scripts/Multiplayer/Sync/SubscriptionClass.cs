using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Multiplayer
{
    /// <summary>
    /// Class that allows a field to be automatically synchronized in a multiplayer setting
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SubscriptionClass<TValue> : ISyncableField where TValue : IAutoSync
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
                void PropertyChanged(object sender, PropertyChangedEventArgs e)
                {
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(this.Name));
                }

                if (this.value != null)
                {
                    this.value.AutoSync.PropertyChanged -= PropertyChanged;
                }
                this.value = value;
                if (this.value != null)
                {
                    this.value.AutoSync.PropertyChanged += PropertyChanged;
                }
            }
        }

        /// <summary>
        /// Return the inner field value as an object
        /// </summary>
        /// <returns></returns>
        public object Serialize(bool fullSync)
        {
            var dataQueue = new Queue<object>();
            
            if (this.Value != null)
            {
                if (fullSync)
                {
                    this.Value.AutoSync.FullStateSync(true, dataQueue);
                }
                else
                {
                    _ = this.Value.AutoSync.PartialStateSync(true, dataQueue);
                }
            }

            return dataQueue.ToArray();
        }

        /// <summary>
        /// Cast the object into the inner field value
        /// </summary>
        /// <param name="value"></param>
        public void Deserialize(bool fullSync, object data)
        {
            if (this.Value != null)
            {
                var dataQueue = new Queue<object>(data as object[]);
                if (fullSync)
                {
                    this.Value.AutoSync.FullStateSync(false, dataQueue);
                }
                else
                {
                    _ = this.Value.AutoSync.PartialStateSync(false, dataQueue);
                }
            }
        }

        /// <summary>
        /// Actual value
        /// </summary>
        private TValue value;

        /// <summary>
        /// Private constructor. This class must be instantiated through one of the static constructors
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private SubscriptionClass(string name, TValue value = default)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Operator overload that returns the value of the synced class
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator TValue(SubscriptionClass<TValue> value) => value.Value;

        /// <summary>
        /// Implicit operator overload that creates a synced class with a property name and default initial value
        /// </summary>
        /// <param name="name"></param>
        public static implicit operator SubscriptionClass<TValue>(string name) => new SubscriptionClass<TValue>(name);

        /// <summary>
        /// Implicit operator overload that creates a synced class with a property name and the given initial value
        /// </summary>
        /// <param name="input"></param>
        public static implicit operator SubscriptionClass<TValue>((string Name, TValue Value) input) => new SubscriptionClass<TValue>(input.Name, input.Value);

    }
}