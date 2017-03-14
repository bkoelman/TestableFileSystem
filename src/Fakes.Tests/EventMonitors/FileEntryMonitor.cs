using System;
using JetBrains.Annotations;

namespace TestableFileSystem.Fakes.Tests.EventMonitors
{
    public sealed class FileEntryMonitor : IDisposable
    {
        [NotNull]
        private readonly FileEntry fileEntry;

        public int ContentChangedCount { get; private set; }

        public bool HasContentChanged => ContentChangedCount > 0;

        public FileEntryMonitor([NotNull] FileEntry fileEntry)
        {
            this.fileEntry = fileEntry;
            fileEntry.ContentChanged += FileEntryOnContentChanged;
        }

        public void Dispose()
        {
            fileEntry.ContentChanged -= FileEntryOnContentChanged;
        }

        private void FileEntryOnContentChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            ContentChangedCount++;
        }
    }
}
