using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;

namespace Diagnostics.Helpers.Analyzer
{
    /// <summary>
    /// It is very expensive to enumerate roots, and relatively cheap to store them in memory.
    /// This service is a cache of roots to use instead of calling ClrHeap.EnumerateRoots.
    /// </summary>
    public class RootCacheService
    {
        private List<(ulong Source, ulong Target)> _dependentHandles;
        private ReadOnlyCollection<ClrRoot> _handleRoots;
        private ReadOnlyCollection<ClrRoot> _finalizerRoots;
        private ReadOnlyCollection<ClrRoot> _stackRoots;
        private bool _printedWarning;
        private bool _printedStackWarning;

        public RootCacheService(ClrRuntime runtime, CancellationToken cancellationToken)
        {
            Runtime = runtime;
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public ClrRuntime Runtime { get; }

        public ReadOnlyCollection<(ulong Source, ulong Target)> GetDependentHandles()
        {
            InitializeHandleRoots();

            // We keep _dependentHandles as a List instead of ReadOnlyCollection so we can use
            // List<>.BinarySearch.
            return _dependentHandles.AsReadOnly();
        }

        public bool IsDependentHandleLink(ulong source, ulong target)
        {
            int i = _dependentHandles.BinarySearch((source, target));
            return i >= 0;
        }

        public IEnumerable<ClrRoot> EnumerateRoots(bool includeFinalizer = true, TextWriter? writer = null)
        {
            PrintWarning();

            foreach (ClrRoot root in GetHandleRoots(writer))
            {
                CancellationToken.ThrowIfCancellationRequested();
                yield return root;
            }

            if (includeFinalizer)
            {
                foreach (ClrRoot root in GetFinalizerQueueRoots(writer))
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    yield return root;
                }
            }

            // If we made it here without the user breaking out of the enumeration
            // then we've already printed a warning on this command run, we don't
            // need to also print the stack warning.
            _printedStackWarning = true;
            foreach (ClrRoot root in GetStackRoots(writer))
            {
                CancellationToken.ThrowIfCancellationRequested();
                yield return root;
            }
        }

        public ReadOnlyCollection<ClrRoot> GetHandleRoots(TextWriter? writer = null)
        {
            InitializeHandleRoots(writer);
            return _handleRoots;
        }


        private void InitializeHandleRoots(TextWriter? writer = null)
        {
            if (_handleRoots is not null && _dependentHandles is not null)
            {
                return;
            }

            PrintWarning(writer);
            List<(ulong Source, ulong Target)> dependentHandles = new();
            List<ClrRoot> handleRoots = new();

            foreach (ClrHandle handle in Runtime.EnumerateHandles())
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (handle.HandleKind == ClrHandleKind.Dependent)
                {
                    dependentHandles.Add((handle.Object, handle.Dependent));
                }

                if (!handle.IsStrong)
                {
                    continue;
                }

                handleRoots.Add(handle);
            }

            // Sort dependentHandles so it can be binary searched
            dependentHandles.Sort();

            _handleRoots = handleRoots.AsReadOnly();
            _dependentHandles = dependentHandles;
        }

        public ReadOnlyCollection<ClrRoot> GetFinalizerQueueRoots(TextWriter? writer = null)
        {
            if (_finalizerRoots is not null)
            {
                return _finalizerRoots;
            }

            PrintWarning(writer);

            // This should be fast, there's rarely many FQ roots
            _finalizerRoots = Runtime.Heap.EnumerateFinalizerRoots().ToList().AsReadOnly();
            return _finalizerRoots;
        }

        private bool PrintWarning(TextWriter? writer = null)
        {
            if (!_printedWarning)
            {
                writer?.WriteLine("Caching GC roots, this may take a while.");
                writer?.WriteLine("Subsequent runs of this command will be faster.");
                writer?.WriteLine();
                _printedWarning = true;
            }
            return !_printedWarning;
        }

        public ReadOnlyCollection<ClrRoot> GetStackRoots(TextWriter? writer=null)
        {
            if (_stackRoots is not null)
            {
                return _stackRoots;
            }

            // Stack roots can take an extra long time to walk, and one mode of !gcroot skips enumerating stack roots.  If the user
            // calls "!gcroot -nostack" they will get a warning the first time, but if they call it again without "-nostack" they
            // may be surprised by a very long pause.  We skip this second message if the user is calling EnumerateRoots().
            if (!_printedStackWarning)
            {
                writer?.WriteLine("Caching GC stack roots, this may take a while.");
                writer?.WriteLine("Subsequent runs of this command will be faster.");
                Console.WriteLine();
                _printedStackWarning = true;
            }

            List<ClrRoot> stackRoots = new();
            foreach (ClrThread thread in Runtime.Threads.Where(thread => thread.IsAlive))
            {
                CancellationToken.ThrowIfCancellationRequested();

                foreach (ClrRoot root in thread.EnumerateStackRoots())
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    stackRoots.Add(root);
                }
            }

            _stackRoots = stackRoots.AsReadOnly();
            return _stackRoots;
        }
    }
}
