using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using TestableFileSystem.Analyzer.Tests.TestDataBuilders;
using TestableFileSystem.Interfaces;
using Xunit;

namespace TestableFileSystem.Analyzer.Tests
{
    public sealed class AnalyzerSpecs : TestableFileSystemAnalysisTestFixture
    {
        [Fact]
        private void When_missing_assembly_reference_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    var stream = File.Create(null);
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_non_fakeable_static_member_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    File.Encrypt(null);
                    Directory.GetAccessControl(null);
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_fakeable_static_member_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    var stream = File.[|Create|](null);
                    var files = Directory.[|GetFiles|](null);
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.File.Create' should be replaced by 'TestableFileSystem.Interfaces.IFile.Create'.",
                "Usage of 'System.IO.Directory.GetFiles' should be replaced by 'TestableFileSystem.Interfaces.IDirectory.GetFiles'.");
        }

        [Fact]
        private void When_invoking_special_static_member_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    var drives1 = Directory.[|GetLogicalDrives|]();
                    var drives2 = Environment.[|GetLogicalDrives|]();
                    var drives3 = DriveInfo.[|GetDrives|]();
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.Directory.GetLogicalDrives' should be replaced by 'TestableFileSystem.Interfaces.IDirectory.GetLogicalDrives'.",
                "Usage of 'System.Environment.GetLogicalDrives' should be replaced by 'TestableFileSystem.Interfaces.IDirectory.GetLogicalDrives'.",
                "Usage of 'System.IO.DriveInfo.GetDrives' should be replaced by 'TestableFileSystem.Interfaces.IFileSystem.GetDrives'.");
        }

        [Fact]
        private void When_invoking_non_fakeable_instance_member_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    FileInfo fileInfo = null;
                    fileInfo.Encrypt();

                    DirectoryInfo directoryInfo = null;
                    directoryInfo.GetAccessControl();

                    FileSystemWatcher watcher = null;
                    watcher.BeginInit();
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_fakeable_instance_member_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    void M1()
                    {
                        FileInfo fileInfo = null;

                        var name = fileInfo.[|Name|];
                        var directory = fileInfo.[|Directory|];
                        var text = fileInfo.[|ToString|]();
                    }

                    void M2()
                    {
                        DirectoryInfo directoryInfo = null;

                        var name = directoryInfo.[|Name|];
                        directoryInfo.[|Delete|](false);
                        var text = directoryInfo.[|ToString|]();
                    }

                    void M3()
                    {
                        DriveInfo driveInfo = null;

                        var name = driveInfo.[|Name|];
                        driveInfo.[|VolumeLabel|] = ""X"";
                        var text = driveInfo.[|ToString|]();
                    }

                    void M4()
                    {
                        FileSystemWatcher watcher = null;

                        var notifyFilter = watcher.[|NotifyFilter|];
                        watcher.[|IncludeSubdirectories|] = true;
                        var result = watcher.[|WaitForChanged|](WatcherChangeTypes.All);
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo.Name' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo.Name'.",
                "Usage of 'System.IO.FileInfo.Directory' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo.Directory'.",
                "Usage of 'System.IO.FileInfo.ToString' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo.ToString'.",
                "Usage of 'System.IO.DirectoryInfo.Name' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo.Name'.",
                "Usage of 'System.IO.DirectoryInfo.Delete' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo.Delete'.",
                "Usage of 'System.IO.DirectoryInfo.ToString' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo.ToString'.",
                "Usage of 'System.IO.DriveInfo.Name' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo.Name'.",
                "Usage of 'System.IO.DriveInfo.VolumeLabel' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo.VolumeLabel'.",
                "Usage of 'System.IO.DriveInfo.ToString' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo.ToString'.",
                "Usage of 'System.IO.FileSystemWatcher.NotifyFilter' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher.NotifyFilter'.",
                "Usage of 'System.IO.FileSystemWatcher.IncludeSubdirectories' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher.IncludeSubdirectories'.",
                "Usage of 'System.IO.FileSystemWatcher.WaitForChanged' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher.WaitForChanged'.");
        }

        [Fact]
        private void When_invoking_non_fakeable_constructor_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    var time = new DateTime(2000, 1, 1);
                    var text = new string('X', 100);
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_fakeable_constructor_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    var fileInfo = new [|FileInfo|](null);
                    var directoryInfo = new [|DirectoryInfo|](null);
                    var driveInfo = new [|DriveInfo|](null);
                    var stream = new [|FileStream|](null, FileMode.Create);
                    var watcher = new [|FileSystemWatcher|](null);
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Construction of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystem.ConstructFileInfo'.",
                "Construction of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystem.ConstructDirectoryInfo'.",
                "Construction of 'System.IO.DriveInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystem.ConstructDriveInfo'.",
                "Construction of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFile.Open'.",
                "Construction of 'System.IO.FileSystemWatcher' should be replaced by 'TestableFileSystem.Interfaces.IFileSystem.ConstructFileSystemWatcher'.");
        }

        [Fact]
        private void When_declaring_parameter_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    void M(MemoryStream ms, BinaryReader br, IOException ex)
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_parameter_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    void M(FileInfo [|fi|], DirectoryInfo [|di|], FileSystemInfo [|fsi|], DriveInfo [|dri|], FileStream [|fs|], FileSystemWatcher [|fw|])
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.DriveInfo' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.",
                "Usage of 'System.IO.FileSystemWatcher' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher'.");
        }

        [Fact]
        private void When_declaring_field_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    private MemoryStream ms;
                    public BinaryReader br;
                    internal IOException ex;
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_field_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    private FileInfo [|fi|];
                    public DirectoryInfo [|di|];
                    internal FileSystemInfo [|fsi|];
                    protected internal DriveInfo [|dri|];
                    protected FileStream [|fs|];
                    protected FileSystemWatcher [|fw|];
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.DriveInfo' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.",
                "Usage of 'System.IO.FileSystemWatcher' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher'.");
        }

        [Fact]
        private void When_declaring_property_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    private MemoryStream ms { get; set; }
                    public BinaryReader br { get; set; }
                    internal IOException ex { get; set; }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_property_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    private FileInfo [|fi|] { get; set; }
                    public DirectoryInfo [|di|] { get; set; }
                    internal FileSystemInfo [|fsi|] { get; set; }
                    protected internal DriveInfo [|dri|] { get; set; }
                    protected FileStream [|fs|] { get; set; }
                    protected FileSystemWatcher [|fw|] { get; set; }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.DriveInfo' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.",
                "Usage of 'System.IO.FileSystemWatcher' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher'.");
        }

        [Fact]
        private void When_declaring_method_return_value_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    private MemoryStream ms() { throw new System.NotImplementedException(); }
                    public BinaryReader br() { throw new System.NotImplementedException(); }
                    internal IOException ex() { throw new System.NotImplementedException(); }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_method_return_value_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new MemberSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultClass(@"
                    private FileInfo [|fi|]() { throw new System.NotImplementedException(); }
                    public DirectoryInfo [|di|]() { throw new System.NotImplementedException(); }
                    internal FileSystemInfo [|fsi|]() { throw new System.NotImplementedException(); }
                    protected internal DriveInfo [|dri|]() { throw new System.NotImplementedException(); }
                    protected FileStream [|fs|]() { throw new System.NotImplementedException(); }
                    protected FileSystemWatcher [|fw|]() { throw new System.NotImplementedException(); }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.DriveInfo' should be replaced by 'TestableFileSystem.Interfaces.IDriveInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.",
                "Usage of 'System.IO.FileSystemWatcher' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemWatcher'.");
        }

        [Fact]
        private void When_inheriting_from_IOException_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    public class CustomException : IOException
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_inheriting_directly_from_FileSystemInfo_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    public abstract class [|DirectInheritor|] : FileSystemInfo
                    {
                    }

                    public abstract class IndirectInheritor : DirectInheritor
                    {
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.");
        }

        [Fact]
        private void When_invoking_fakeable_extension_method_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InDefaultMethod(@"
                    var stream1 = File.[|OpenRead|](null);

                    FileInfo info = null;
                    var stream2 = info.[|OpenRead|]();
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.File.OpenRead' should be replaced by 'TestableFileSystem.Interfaces.FileExtensions.OpenRead'.",
                "Usage of 'System.IO.FileInfo.OpenRead' should be replaced by 'TestableFileSystem.Interfaces.FileInfoExtensions.OpenRead'.");
        }

        [Fact]
        private void When_invoking_external_constructor_with_stream_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(StreamReader).Namespace)
                .WithReference(typeof(XmlDocument).Assembly)
                .Using(typeof(XmlDocument).Namespace)
                .WithReference(typeof(XDocument).Assembly)
                .Using(typeof(XDocument).Namespace)
                .Using(typeof(Encoding).Namespace)
                .InDefaultMethod(@"
                    new StreamReader(new MemoryStream());

                    new StreamWriter(new MemoryStream());

                    new XmlDocument().Load(new MemoryStream());
                    new XmlDocument().Save(new MemoryStream());

                    XmlWriter.Create(new MemoryStream());

                    new XmlTextWriter(new MemoryStream(), Encoding.UTF8);
                    XmlTextWriter.Create(new MemoryStream());

                    new XDocument().Save(new MemoryStream());

                    new XElement(string.Empty).Save(new MemoryStream());

                    new XStreamingElement(string.Empty).Save(new MemoryStream());
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_external_constructor_with_path_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new BlockSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(StreamReader).Namespace)
                .WithReference(typeof(XmlDocument).Assembly)
                .Using(typeof(XmlDocument).Namespace)
                .WithReference(typeof(XDocument).Assembly)
                .Using(typeof(XDocument).Namespace)
                .Using(typeof(Encoding).Namespace)
                .InDefaultMethod(@"
                    new [|StreamReader|](string.Empty);
                    new [|StreamReader|](string.Empty, false);
                    new [|StreamReader|](string.Empty, Encoding.UTF8);
                    new [|StreamReader|](string.Empty, Encoding.UTF8, false);
                    new [|StreamReader|](string.Empty, Encoding.UTF8, false, 1024);

                    new [|StreamWriter|](string.Empty);
                    new [|StreamWriter|](string.Empty, true);
                    new [|StreamWriter|](string.Empty, true, Encoding.UTF8);
                    new [|StreamWriter|](string.Empty, true, Encoding.UTF8, 1024);

                    new XmlDocument().[|Load|](string.Empty);
                    new XmlDocument().[|Save|](string.Empty);

                    XmlWriter.[|Create|](string.Empty);
                    XmlWriter.[|Create|](string.Empty, new XmlWriterSettings());

                    new [|XmlTextWriter|](string.Empty, Encoding.UTF8);
                    XmlTextWriter.[|Create|](string.Empty);
                    XmlTextWriter.[|Create|](string.Empty, new XmlWriterSettings());

                    new XDocument().[|Save|](string.Empty);
                    new XDocument().[|Save|](string.Empty, SaveOptions.None);

                    new XElement(string.Empty).[|Save|](string.Empty);
                    new XElement(string.Empty).[|Save|](string.Empty, SaveOptions.None);

                    new XStreamingElement(string.Empty).[|Save|](string.Empty);
                    new XStreamingElement(string.Empty).[|Save|](string.Empty, SaveOptions.None);
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Constructor of 'System.IO.StreamReader' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamReader' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamReader' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamReader' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamReader' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamWriter' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamWriter' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamWriter' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.IO.StreamWriter' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.XmlDocument.Load' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.XmlDocument.Save' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.XmlWriter.Create' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.XmlWriter.Create' should be passed a 'System.IO.Stream' instead of a file path.",
                "Constructor of 'System.Xml.XmlTextWriter' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.XmlWriter.Create' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.XmlWriter.Create' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.Linq.XDocument.Save' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.Linq.XDocument.Save' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.Linq.XElement.Save' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.Linq.XElement.Save' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.Linq.XStreamingElement.Save' should be passed a 'System.IO.Stream' instead of a file path.",
                "Member 'System.Xml.Linq.XStreamingElement.Save' should be passed a 'System.IO.Stream' instead of a file path.");
        }
    }
}
