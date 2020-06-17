using ExitGames.Client.Photon;

namespace Multiplayer
{
    public static class MultiplayerCustomTypes
    {        
        private static byte CustomCodeColor = 1;

        public static void Register()
        {
            PhotonPeer.RegisterType(typeof(UnityEngine.Color), CustomCodeColor, ColorSerialize, ColorDeserialize);
        }

        private static readonly byte[] memVector3 = new byte[3 * 4];

        public static short ColorSerialize(StreamBuffer outStream, object customObject)
        {
            UnityEngine.Color color = (UnityEngine.Color)customObject;
            lock (memVector3)
            {
                byte[] bytes = memVector3;
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
            UnityEngine.Color color = new UnityEngine.Color();
            lock (memVector3)
            {
                inStream.Read(memVector3, 0, 3 * 4);
                int index = 0;
                Protocol.Deserialize(out color.r, memVector3, ref index);
                Protocol.Deserialize(out color.g, memVector3, ref index);
                Protocol.Deserialize(out color.b, memVector3, ref index);
            }

            return color;
        }
    }
}
