using Photon.Pun;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Networking
{
    /// <summary>
    /// Class which aims in syncing the fields and properties of an INetworkedObject
    /// </summary>
    public class AutoSync : MonoBehaviourPun, IChangeTracking, INotifyPropertyChanged
    {
        /// <summary>
        /// The component to sync
        /// </summary>
        public UnityEngine.Object ObjectToSync
        {
            get => this.objectToSync;
            set
            {
                this.objectToSync = value;
                ReadProperties();
            }
        }

        /// <summary>
        /// Returns true if there are changes
        /// </summary>
        public bool IsChanged => this.dirtyBuffer.Count != 0;

        /// <summary>
        /// Event called when a property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<string[]> SyncCompleted;

        /// <summary>
        /// Properties and fields that are being synced hashed by their property name
        /// </summary>
        private Dictionary<string, ISerializable> syncables = new Dictionary<string, ISerializable>();

        /// <summary>
        /// Stores the last synced values so an equation can be made to see if they have changed
        /// </summary>
        private Dictionary<string, object> valueStore = new Dictionary<string, object>();
        
        /// <summary>
        /// A set of names of properties that have changed
        /// </summary>
        private HashSet<string> dirtyBuffer = new HashSet<string>();

        /// <summary>
        /// Reusable data queue for synchronization
        /// </summary>
        private Queue<object> partialDataQueue = new Queue<object>();

        private UnityEngine.Object objectToSync;
 

        ///// <summary>
        ///// Create an instance given the component to be synced
        ///// </summary>
        ///// <param name="componentToSync"></param>
        ///// <returns></returns>
        //public static AutoSync Create(UnityEngine.Component componentToSync)
        //{
        //    return componentToSync ? new AutoSync(componentToSync, componentToSync.GetComponent<PhotonView>()) : default;
        //}

        ///// <summary>
        ///// Create an instance given a scriptable object to be synced
        ///// </summary>
        ///// <param name="scriptableObjectToSync"></param>
        ///// <returns></returns>
        //public static AutoSync Create(ScriptableObject scriptableObjectToSync, PhotonView photonView)
        //{
        //    return scriptableObjectToSync ? new AutoSync(scriptableObjectToSync, photonView) : default;
        //}

        /// <summary>
        /// Read properties of assigned object
        /// </summary>
        private void ReadProperties()
        {
            // Read normal CLR properties with Synced attribute
            var syncedProperties = this.objectToSync.GetType().GetProperties().
                Where(prop => prop.IsDefined(typeof(SyncedAttribute), false));

            // Create hash table
            this.syncables = syncedProperties.ToDictionary(prop => prop.Name, prop => PropertyWriter.Create(this.objectToSync, prop));

            // Read all fields with Synced attribute that implement ISyncableField
            var syncedFields = this.objectToSync.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).
                Where(field => field.IsDefined(typeof(SyncedAttribute), false) && typeof(ISyncableField).IsAssignableFrom(field.FieldType));

            // Add synced fields to syncables list and register their PropertyChanged
            foreach (var field in syncedFields)
            {
                if (field.GetValue(this.objectToSync) is ISyncableField syncableField)
                {
                    this.syncables[field.Name] = syncableField;

                    syncableField.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
                    {
                        var propertyName = e.PropertyName;
                        this.valueStore[propertyName] = (sender as ISyncableField).Serialize(false);
                        this.dirtyBuffer.Add(propertyName);
                        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    };
                }
            }
        }

        /// <summary>
        /// Should be called in the owning component's Update method
        /// </summary>
        private void Update()
        {
            if (this.photonView.IsMine && this.IsChanged)
            {
                var keys = PartialStateSync(true, this.partialDataQueue);
                AcceptChanges();

                var dataArray = this.partialDataQueue.ToArray();
                this.partialDataQueue.Clear();

                if (this.photonView)
                {
                    this.photonView.RPC(nameof(this.RpcPartialStateSync), RpcTarget.Others,  (object)dataArray);
                }

                this.SyncCompleted(this, keys);
            }
        }

        /// <summary>
        /// Clears the dirty buffer (comes from IChangeTracking)
        /// </summary>
        public void AcceptChanges()
        {
            this.dirtyBuffer.Clear();
        }

        /// <summary>
        /// Write all syncables to the stream regardless of whether they are dirty or not
        /// </summary>
        /// <param name="isWriting"></param>
        /// <param name="dataQueue"></param>
        public void FullStateSync(bool isWriting, Queue<object> dataQueue)
        {
            if (isWriting)
            {
                foreach (var serializable in this.syncables.Values)
                {
                    var value = serializable.Serialize(true);
                    dataQueue.Enqueue(value);
                }
            }
            else
            {
                foreach (var serializable in this.syncables.Values)
                {
                    var value = dataQueue.Dequeue();
                    serializable.Deserialize(true, value);
                }
            }
        }

        /// <summary>
        /// Write all dirty (changed) syncables to the stream
        /// </summary>
        /// <param name="isWriting"></param>
        /// <param name="dataQueue"></param>
        public string[] PartialStateSync(bool isWriting, Queue<object> dataQueue)
        {
            if (isWriting)
            {
                foreach (var propertyName in this.dirtyBuffer)
                {
                    dataQueue.Enqueue(propertyName);
                    dataQueue.Enqueue(this.valueStore[propertyName]);
                }
                return this.dirtyBuffer.ToArray();
            }
            else
            {
                var result = new List<string>();
                while (dataQueue.Count > 1)
                {
                    var propertyName = dataQueue.Dequeue<string>();
                    result.Add(propertyName);
                    var value = dataQueue.Dequeue();

                    if (this.syncables.ContainsKey(propertyName))
                    {
                        // Invoke setter on the synced field/property
                        this.syncables[propertyName].Deserialize(false, value);

                        // In case the setter does not call onChange, call it here, and mark the field as not dirty
                        this.valueStore[propertyName] = value;
                        this.dirtyBuffer.Remove(propertyName);
                    }
                }
                return result.ToArray();
            }
        }

        /// <summary>
        /// Call this to mark a property dirty and store its value in the valuestore
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void OnChange(string propertyName, object value)
        {
            this.valueStore[propertyName] = value;
            this.dirtyBuffer.Add(propertyName);
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [PunRPC]
        protected virtual void RpcPartialStateSync(object[] dataArray)
        {
            var dataQueue = new Queue<object>(dataArray);

            var keys = this.PartialStateSync(false, dataQueue);
            this.AcceptChanges();

            this.SyncCompleted(this, keys);
        }
    }
}
