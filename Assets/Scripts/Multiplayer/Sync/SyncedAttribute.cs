using System;

namespace Multiplayer
{
    /// <summary>
    /// Attribute used to mark a property as being synced
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SyncedAttribute : Attribute { }
}
