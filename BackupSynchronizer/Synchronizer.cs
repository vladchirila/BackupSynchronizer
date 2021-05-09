using System;
using System.Collections.Generic;
using System.Linq;

namespace BackupSynchronizer
{
    public class Synchronizer
    {
        public IEnumerable<SynchronizeAction<T, U>> Synchronize<T, U>(INode<T, U> source, INode<T, U> dest) where T : INode<T, U> where U : INodeElement
        {
            return SynchronizeFiles(source, dest)
                .Concat(SynchronizeFolders(source, dest));
        }

        private IEnumerable<SynchronizeAction<T, U>> SynchronizeFiles<T, U>(INode<T, U> source, INode<T, U> dest) where T : INode<T, U> where U : INodeElement
        {
            var sourceElements = source.GetElements();
            var destElements = dest.GetElements();

            var differences = GetDifferences(sourceElements, destElements, (s, d) => s.IsEqual(d));

            foreach (var (sourceElement, destElement, differenceType) in differences)
            {
                switch (differenceType)
                {
                    case DifferenceType.SourceMissing:
                        yield return new SynchronizeAction<T,U> { DestinationNodeElement = destElement, ActionType = ActionType.DeleteDestNodeElement };
                        break;
                    case DifferenceType.DestinationMissing:
                        yield return new SynchronizeAction<T, U> { SourceNodeElement = sourceElement, DestinationNode = dest, ActionType = ActionType.CopySourceNodeElementToDestination };
                        break;
                    case DifferenceType.Same:
                        //identical elements shouldn't be copied over
                        break;
                    default:
                        throw new Exception("We shouldn't get here.");
                }
            }
        }

        private IEnumerable<SynchronizeAction<T,U>> SynchronizeFolders<T, U>(INode<T, U> source, INode<T, U> dest) where T : INode<T, U> where U : INodeElement
        {
            var sourceNodes = source.GetSubnodes();
            var destNodes = dest.GetSubnodes();

            var differences = GetDifferences(sourceNodes, destNodes, (s, d) => s.IsEqual(d));

            foreach (var (sourceNode, destNode, differenceType) in differences)
            {
                switch (differenceType)
                {
                    case DifferenceType.SourceMissing:
                        yield return new SynchronizeAction<T, U> { DestinationNode = destNode, ActionType = ActionType.DeleteDestNode };
                        break;
                    case DifferenceType.DestinationMissing:
                        yield return new SynchronizeAction<T, U> { SourceNode = sourceNode, DestinationNode = dest, ActionType = ActionType.CopySourceNodeToDestination };
                        break;
                    case DifferenceType.Same:
                        foreach (var dif in Synchronize(sourceNode, destNode))
                            yield return dif;
                        break;
                }
            }
        }

        enum DifferenceType
        {
            Same,
            SourceMissing,
            DestinationMissing,
        }

        static IEnumerable<(X, X, DifferenceType)> GetDifferences<X>(IEnumerable<X> source, IEnumerable<X> dest, Func<X, X, bool> areEqual)
        {
            source = source.ToArray();
            dest = dest.ToArray();

            var annotatedSource = source.Select(s => new { source = s, dest = dest.FirstOrDefault(d => areEqual(s, d)) });

            var onlyInSource = annotatedSource.Where(a => a.dest == null).Select(a => (a.source, default(X), DifferenceType.DestinationMissing));
            var inBoth = annotatedSource.Where(a => a.dest != null).Select(a => (a.source, a.dest, DifferenceType.Same));
            var onlyInDest = dest.Select(d => new { source = source.FirstOrDefault(s => areEqual(s, d)), dest = d }).Where(a => a.source == null).Select(a => (default(X), a.dest, DifferenceType.SourceMissing));

            return onlyInDest.Concat(onlyInSource).Concat(inBoth);
        }
    }

    public enum ActionType
    {
        DeleteDestNodeElement,
        CopySourceNodeElementToDestination,

        DeleteDestNode,
        CopySourceNodeToDestination
    }

    public class SynchronizeAction<T, U> where T : INode<T, U> where U : INodeElement
    {
        public INode<T, U> SourceNode;
        public INode<T, U> DestinationNode;
        public U SourceNodeElement;
        public U DestinationNodeElement;
        public ActionType ActionType;
    }
}
