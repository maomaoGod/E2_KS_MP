using System;

namespace E2MultiPlayer
{
    public interface IDisposer
    {
        bool Disposed { get; }
        bool Dispose();
    }
    
    public interface ISingleton:IDisposer
    {

    }
}