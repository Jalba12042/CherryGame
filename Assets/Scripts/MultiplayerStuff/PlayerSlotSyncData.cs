using Unity.Netcode;

// One entry per claimed customize-screen slot, replicated so every connected machine can
// render everyone else's slot live (not just find out once the round starts).
public struct PlayerSlotSyncData : INetworkSerializable, System.IEquatable<PlayerSlotSyncData>
{
    public ulong ownerClientId;
    // Which local slot (0-3) on the owning machine this is - lets one machine manage several
    // local players independently.
    public int localSlotOnOwner;
    public PlayerCustomizationData data;
    public bool ready;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ownerClientId);
        serializer.SerializeValue(ref localSlotOnOwner);
        serializer.SerializeValue(ref data);
        serializer.SerializeValue(ref ready);
    }

    public bool Equals(PlayerSlotSyncData other)
    {
        return ownerClientId == other.ownerClientId && localSlotOnOwner == other.localSlotOnOwner &&
               data.Equals(other.data) && ready == other.ready;
    }
}
