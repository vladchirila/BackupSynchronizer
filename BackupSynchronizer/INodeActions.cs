namespace BackupSynchronizer
{
    public interface INodeActions<V, T, U> where V: INode<T, U> where T : INode<T, U> where U : INodeElement
    {
        void Add(T node, T subnode);
        void Add(T node, U element);
        void Remove(T node);
        void Remove(U nodeElement);
    }
}
