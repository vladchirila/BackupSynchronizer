using System.Collections.Generic;

namespace BackupSynchronizer
{
    public interface INode<T, U> where T : INode<T, U> where U : INodeElement
    {
        bool IsEqual(T other);
        IEnumerable<T> GetSubnodes();
        IEnumerable<U> GetElements();
    }
}
