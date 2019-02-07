#if !NETSTANDARD1_3
using System;
using System.IO;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using TestableFileSystem.Interfaces;

namespace TestableFileSystem.Wrappers
{
    [Serializable]
    public sealed class DriveInfoWrapper : IDriveInfo, ISerializable
    {
        [NotNull]
        private readonly DriveInfo source;

        public string Name => source.Name;

        public bool IsReady => source.IsReady;

        public long AvailableFreeSpace => source.AvailableFreeSpace;
        public long TotalFreeSpace => source.TotalFreeSpace;
        public long TotalSize => source.TotalSize;

        public DriveType DriveType => source.DriveType;

        public string DriveFormat => source.DriveFormat;

        public string VolumeLabel
        {
            get => source.VolumeLabel;
            set => source.VolumeLabel = value;
        }

        public IDirectoryInfo RootDirectory => new DirectoryInfoWrapper(source.RootDirectory);

        private DriveInfoWrapper([NotNull] SerializationInfo info, StreamingContext context)
        {
            string name = (string)info.GetValue("_name", typeof(string));
            source = new DriveInfo(name);
        }

        public DriveInfoWrapper([NotNull] DriveInfo source)
        {
            Guard.NotNull(source, nameof(source));
            this.source = source;
        }

        public override string ToString()
        {
            return source.ToString();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_name", source.Name, typeof(string));
        }
    }
}
#endif
