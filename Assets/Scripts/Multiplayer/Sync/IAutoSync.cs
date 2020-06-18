using Networking;
using System.ComponentModel;

namespace Multiplayer
{
    /// <summary>
    /// Interface that tells that this class can auto sync
    /// </summary>
    public interface IAutoSync
    {
        MultiplayerAutoSync AutoSync { get; }
    }
}