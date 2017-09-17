using System.IO;
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            var stream = File.Create(null);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_non_fakeable_static_member_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            File.Encrypt(null);
                            Directory.GetAccessControl(null);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_fakeable_static_member_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            var stream = [|File.Create|](null);
                            var files = [|Directory.GetFiles|](null);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.File.Create' should be replaced by 'TestableFileSystem.Interfaces.IFile.Create'.",
                "Usage of 'System.IO.Directory.GetFiles' should be replaced by 'TestableFileSystem.Interfaces.IDirectory.GetFiles'.");
        }

        [Fact]
        private void When_invoking_non_fakeable_instance_member_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            FileInfo fileInfo = null;
                            fileInfo.Encrypt();

                            DirectoryInfo directoryInfo = null;
                            directoryInfo.GetAccessControl();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_fakeable_instance_member_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M1()
                        {
                            FileInfo fileInfo = null;

                            var name = [|fileInfo.Name|];
                            var directory = [|fileInfo.Directory|];
                            var text = [|fileInfo.ToString|]();
                        }

                        void M2()
                        {
                            DirectoryInfo directoryInfo = null;

                            var name = [|directoryInfo.Name|];
                            [|directoryInfo.Delete|](false);
                            var text = [|directoryInfo.ToString|]();
                        }
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
                "Usage of 'System.IO.DirectoryInfo.ToString' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo.ToString'.");
        }

        [Fact]
        private void When_invoking_non_fakeable_constructor_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            var time = new DateTime(2000, 1, 1);
                            var text = new string('X', 100);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_invoking_fakeable_constructor_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            var fileInfo = new [|FileInfo|](null);
                            var directoryInfo = new [|DirectoryInfo|](null);
                            var stream = new [|FileStream|](null, FileMode.Create);
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.");
        }

        [Fact]
        private void When_declaring_parameter_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M(MemoryStream ms, BinaryReader br, IOException ex)
                        {
                        }
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M(FileInfo [|fi|], DirectoryInfo [|di|], FileSystemInfo [|fsi|], FileStream [|fs|])
                        {
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.");
        }

        [Fact]
        private void When_declaring_field_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private MemoryStream ms;
                        public BinaryReader br;
                        internal IOException ex;
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_field_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private FileInfo [|fi|];
                        public DirectoryInfo [|di|];
                        internal FileSystemInfo [|fsi|];
                        protected FileStream [|fs|];
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.");
        }

        [Fact]
        private void When_declaring_property_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private MemoryStream ms { get; set; }
                        public BinaryReader br { get; set; }
                        internal IOException ex { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_property_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private FileInfo [|fi|] { get; set; }
                        public DirectoryInfo [|di|] { get; set; }
                        internal FileSystemInfo [|fsi|] { get; set; }
                        protected FileStream [|fs|] { get; set; }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.");
        }

        [Fact]
        private void When_declaring_method_return_value_of_non_fakeable_type_it_must_be_skipped()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private MemoryStream ms() { throw new System.NotImplementedException(); }
                        public BinaryReader br() { throw new System.NotImplementedException(); }
                        internal IOException ex() { throw new System.NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source);
        }

        [Fact]
        private void When_declaring_method_return_value_of_fakeable_type_it_must_be_reported()
        {
            // Arrange
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        private FileInfo [|fi|]() { throw new System.NotImplementedException(); }
                        public DirectoryInfo [|di|]() { throw new System.NotImplementedException(); }
                        internal FileSystemInfo [|fsi|]() { throw new System.NotImplementedException(); }
                        protected FileStream [|fs|]() { throw new System.NotImplementedException(); }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.FileInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileInfo'.",
                "Usage of 'System.IO.DirectoryInfo' should be replaced by 'TestableFileSystem.Interfaces.IDirectoryInfo'.",
                "Usage of 'System.IO.FileSystemInfo' should be replaced by 'TestableFileSystem.Interfaces.IFileSystemInfo'.",
                "Usage of 'System.IO.FileStream' should be replaced by 'TestableFileSystem.Interfaces.IFileStream'.");
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
            ParsedSourceCode source = new TypeSourceCodeBuilder()
                .WithReference(typeof(IFileSystem).Assembly)
                .Using(typeof(File).Namespace)
                .InGlobalScope(@"
                    class C
                    {
                        void M()
                        {
                            var stream1 = [|File.OpenRead|](null);

                            FileInfo info = null;
                            var stream2 = [|info.OpenRead|]();
                        }
                    }
                ")
                .Build();

            // Act and assert
            VerifyFileSystemDiagnostic(source,
                "Usage of 'System.IO.File.OpenRead' should be replaced by 'TestableFileSystem.Interfaces.FileExtensions.OpenRead'.",
                "Usage of 'System.IO.FileInfo.OpenRead' should be replaced by 'TestableFileSystem.Interfaces.FileInfoExtensions.OpenRead'.");
        }
    }
}
