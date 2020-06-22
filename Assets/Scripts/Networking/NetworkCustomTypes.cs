using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static Networking.NetworkManager;

namespace Networking
{
    public static class NetworkCustomTypes
    {
        private static byte CustomCodeColor = 1;
        private static byte CustomCodeQueue = 2;

        public static void Register()
        {
            PhotonPeer.RegisterType(typeof(UnityEngine.Color), CustomCodeColor, ColorSerialize, ColorDeserialize);
            PhotonPeer.RegisterType(typeof(Queue<object>), CustomCodeQueue, QueueSerialize, QueueDeserialize);
        }

        private static readonly byte[] colorMemory = new byte[3 * 4];
        private static readonly byte[] lengthMemory = new byte[2];

        public static short ColorSerialize(StreamBuffer outStream, object customObject)
        {
            var color = (UnityEngine.Color)customObject;
            lock (colorMemory)
            {
                byte[] bytes = colorMemory;
                int index = 0;
                Protocol.Serialize(color.r, bytes, ref index);
                Protocol.Serialize(color.g, bytes, ref index);
                Protocol.Serialize(color.b, bytes, ref index);
                outStream.Write(bytes, 0, 3 * 4);
            }

            return 3 * 4;
        }

        public static object ColorDeserialize(StreamBuffer inStream, short length)
        {
            var color = new UnityEngine.Color();
            lock (colorMemory)
            {
                inStream.Read(colorMemory, 0, 3 * 4);
                int index = 0;
                Protocol.Deserialize(out color.r, colorMemory, ref index);
                Protocol.Deserialize(out color.g, colorMemory, ref index);
                Protocol.Deserialize(out color.b, colorMemory, ref index);
            }

            return color;
        }

        public static short QueueSerialize(StreamBuffer outStream, object customObject)
        {
            short length = 0;
            var queue = (Queue<object>)customObject;

            outStream.Write(BitConverter.GetBytes((UInt16)queue.Count), 0, 2);
            length += 2;

            while (queue.Count > 0)
            {
                var obj = queue.Dequeue<object>();

                var bytes = ObjectToByteArray(obj);

                outStream.Write(BitConverter.GetBytes((UInt16)bytes.Length), 0, 2);
                length += 2;

                outStream.Write(bytes, 0, bytes.Length);
                length += (short)bytes.Length;
            }

            return length;
        }

        public static object QueueDeserialize(StreamBuffer inStream, short length)
        {
            var queue = new Queue<object>();
            lock (lengthMemory)
            {
                inStream.Read(lengthMemory, 0, 2);
                UInt16 queueLength = BitConverter.ToUInt16(lengthMemory, 0);

                while (queue.Count < queueLength)
                {
                    inStream.Read(lengthMemory, 0, 2);
                    UInt16 objLength = BitConverter.ToUInt16(lengthMemory, 0);

                    byte[] bytes = new byte[objLength];
                    inStream.Read(bytes, 0, objLength);

                    var obj = ByteArrayToObject(bytes);

                    queue.Enqueue(obj);
                }
            }

            return queue;
        }

        private static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            if (obj is UnityEngine.Color colorObj)
            {
                obj = new SerializeableColor(colorObj);
            }
            else if (obj is UnityEngine.Vector3 vector3Obj)
            {
                obj = new SerializeableVector3(vector3Obj);
            }
            else if (obj is UnityEngine.Quaternion quaternionObj)
            {
                obj = new SerializeableQuaternion(quaternionObj);
            }

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);

            return ms.ToArray();
        }

        // Convert a byte array to an Object
        private static object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            object obj = (object)binForm.Deserialize(memStream);

            if (obj is SerializeableColor serializeableColorObj)
            {
                obj = serializeableColorObj.ToUnityColor();
            }
            else if (obj is SerializeableVector3 serializeableVector3Obj)
            {
                obj = serializeableVector3Obj.ToUnityVector3();
            }
            else if (obj is SerializeableQuaternion serializeableQuaternionObj)
            {
                obj = serializeableQuaternionObj.ToUnityQuaternion();
            }



            return obj;
        }
    }

    [Serializable]
    public struct SerializeableVector3
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public SerializeableVector3(UnityEngine.Vector3 vector3)
        {
            this.x = vector3.x;
            this.y = vector3.y;
            this.z = vector3.z;
        }

        public UnityEngine.Vector3 ToUnityVector3()
        {
            return new UnityEngine.Vector3(this.x, this.y, this.z);
        }
    }

    [Serializable]
    public struct SerializeableQuaternion
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public SerializeableQuaternion(UnityEngine.Quaternion quaternion)
        {
            this.x = quaternion.x;
            this.y = quaternion.y;
            this.z = quaternion.z;
            this.w = quaternion.w;
        }

        public UnityEngine.Quaternion ToUnityQuaternion()
        {
            return new UnityEngine.Quaternion(this.x, this.y, this.z, this.w);
        }
    }

    [Serializable]
    public struct SerializeableColor
    {
        public float r { get; set; }
        public float g { get; set; }
        public float b { get; set; }
        public float a { get; set; }

        public SerializeableColor(UnityEngine.Color color)
        {
            this.r = color.r;
            this.g = color.g;
            this.b = color.b;
            this.a = color.a;
        }

        public UnityEngine.Color ToUnityColor()
        {
            return new UnityEngine.Color(this.r, this.g, this.b, this.a);
        }
    }
}

