namespace RailNet.Core
{
    public interface IRailNetClient
    {
        bool Connected { get; }

        void Disconnect();
    }
}
