namespace BackupSynchronizer
{
    public interface INodeElement
    {
        bool IsEqual(INodeElement other);
    }
}
