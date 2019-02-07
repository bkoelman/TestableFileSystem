#if NETSTANDARD1_3
// ReSharper disable once CheckNamespace
namespace System.IO
{
    /// <summary>
    /// Defines constants for drive types, including CDRom, Fixed, Network, NoRootDirectory, Ram, Removable, and Unknown.
    /// </summary>
    public enum DriveType
    {
        /// <summary>
        /// The type of drive is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// The drive does not have a root directory.
        /// </summary>
        NoRootDirectory,

        /// <summary>
        /// The drive is a removable storage device, such as a floppy disk drive or a USB flash drive.
        /// </summary>
        Removable,

        /// <summary>
        /// The drive is a fixed disk.
        /// </summary>
        Fixed,

        /// <summary>
        /// The drive is a network drive.
        /// </summary>
        Network,

        /// <summary>
        /// The drive is an optical disc device, such as a CD or DVD-ROM.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        CDRom,

        /// <summary>
        /// The drive is a RAM disk.
        /// </summary>
        Ram
    }
}
#endif
