// See LICENSE file in the root directory
//

namespace LGK.Networking
{
    public interface IMessageSerializer
    {
        void Serialize(NetworkWriter writer);
    }

    public interface IMessageDeserializer
    {
        void Deserialize(IConnection conn, NetworkReader reader);
    }

    public interface IObjectSerializer
    {
        void Serialize(NetworkSyncWriter writer);
    }

    public interface IObjectDeserializer
    {
        void Deserialize(IConnection conn, NetworkReader reader);
    }
}

