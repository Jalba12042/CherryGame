using Unity.Netcode;

[System.Serializable]
public struct PlayerCustomizationData : INetworkSerializable, System.IEquatable<PlayerCustomizationData>
{
    public int headIndex;
    public int faceIndex;
    public int torsoIndex;
    public int bottomIndex;

    public int colorIndex;
    public int nameIndex;

    public static PlayerCustomizationData Unset => new PlayerCustomizationData
    {
        headIndex = -1,
        faceIndex = -1,
        torsoIndex = -1,
        bottomIndex = -1,
        colorIndex = -1,
        nameIndex = -1
    };

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref headIndex);
        serializer.SerializeValue(ref faceIndex);
        serializer.SerializeValue(ref torsoIndex);
        serializer.SerializeValue(ref bottomIndex);
        serializer.SerializeValue(ref colorIndex);
        serializer.SerializeValue(ref nameIndex);
    }

    public bool Equals(PlayerCustomizationData other)
    {
        return headIndex == other.headIndex && faceIndex == other.faceIndex && torsoIndex == other.torsoIndex &&
               bottomIndex == other.bottomIndex && colorIndex == other.colorIndex && nameIndex == other.nameIndex;
    }
}
