using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ApiInspector.Infrastructure
{
    /// <summary>
    ///     The embedded compressed assembly references resolver
    /// </summary>
    class EmbeddedCompressedAssemblyReferencesResolver
    {
        #region Fields
        /// <summary>
        ///     The application domain
        /// </summary>
        readonly AppDomain appDomain;

        /// <summary>
        ///     The embedded zip file name
        /// </summary>
        readonly string embeddedZipFileName;

        /// <summary>
        ///     The located assembly
        /// </summary>
        readonly Assembly locatedAssembly;

        /// <summary>
        ///     The temporary directory
        /// </summary>
        readonly string temporaryDirectory;

        /// <summary>
        ///     The temporary zip file path
        /// </summary>
        readonly string temporaryZipFilePath;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedCompressedAssemblyReferencesResolver" /> class.
        /// </summary>
        public EmbeddedCompressedAssemblyReferencesResolver(AppDomain appDomain, Assembly locatedAssembly, string embeddedZipFileName)
        {
            this.appDomain           = appDomain;
            this.locatedAssembly     = locatedAssembly;
            this.embeddedZipFileName = embeddedZipFileName;

            temporaryDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                                 Path.DirectorySeparatorChar +
                                 Path.GetFileNameWithoutExtension(locatedAssembly.Location) + "-" + Path.GetFileNameWithoutExtension(embeddedZipFileName) +
                                 Path.DirectorySeparatorChar;

            temporaryZipFilePath = temporaryDirectory + embeddedZipFileName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbeddedCompressedAssemblyReferencesResolver" /> class.
        /// </summary>
        public EmbeddedCompressedAssemblyReferencesResolver(Assembly locatedAssembly, string assemblyFullName) : this(AppDomain.CurrentDomain, locatedAssembly, assemblyFullName)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets or sets the trace.
        /// </summary>
        public Action<string> Trace { get; set; } = Console.WriteLine;
        #endregion

        #region Public Methods
        /// <summary>
        ///     Resolves the specified assembly full name.
        /// </summary>
        public static void Resolve(string embeddedZipFileName)
        {
            new EmbeddedCompressedAssemblyReferencesResolver(typeof(EmbeddedCompressedAssemblyReferencesResolver).Assembly, embeddedZipFileName).Resolve();
        }

        /// <summary>
        ///     Resolves this instance.
        /// </summary>
        public void Resolve()
        {
            ExtractZipFile();

            appDomain.AssemblyResolve += Resolve;
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Reads the resource.
        /// </summary>
        static byte[] ReadResource(Assembly locatedAssembly, string resourceSuffix)
        {
            var matchedResourceNames = locatedAssembly.GetManifestResourceNames().Where(x => x.EndsWith(resourceSuffix)).ToList();

            if (!matchedResourceNames.Any())
            {
                throw new MissingManifestResourceException(resourceSuffix);
            }

            var resourceName = matchedResourceNames[0];
            if (resourceName == null)
            {
                throw new MissingManifestResourceException(resourceSuffix);
            }

            using (var manifestResourceStream = locatedAssembly.GetManifestResourceStream(resourceName))
            {
                if (manifestResourceStream == null)
                {
                    throw new MissingManifestResourceException(resourceName);
                }

                var data = new byte[manifestResourceStream.Length];

                manifestResourceStream.Read(data, 0, data.Length);

                return data;
            }
        }

        /// <summary>
        ///     Extracts the zip file.
        /// </summary>
        void ExtractZipFile()
        {
            if (File.Exists(temporaryZipFilePath))
            {
                Trace($"Deleting file {temporaryZipFilePath}");
                File.Delete(temporaryZipFilePath);
            }

            var targetDirectory = Path.GetDirectoryName(temporaryZipFilePath);
            if (targetDirectory == null)
            {
                throw new ArgumentNullException(nameof(targetDirectory));
            }

            if (File.Exists(targetDirectory))
            {
                Trace($"Deleting file {targetDirectory}");
                File.Delete(targetDirectory);
            }

            if (Directory.Exists(targetDirectory))
            {
                Trace($"Deleting directory {targetDirectory}");
                Directory.Delete(targetDirectory, true);
            }

            Trace($"Creating directory {targetDirectory}");
            Directory.CreateDirectory(targetDirectory);

            File.WriteAllBytes(temporaryZipFilePath, ReadResource(locatedAssembly, "." + embeddedZipFileName));

            ZipFile.ExtractToDirectory(temporaryZipFilePath, temporaryDirectory);
        }

        /// <summary>
        ///     Resolves the specified sender.
        /// </summary>
        Assembly Resolve(object sender, ResolveEventArgs args)
        {
            // Sample:
            // args.Name => YamlDotNet, Version=8.0.0.0, Culture=neutral, PublicKeyToken=ec19458f3c15af5e
            // new AssemblyName(args.Name).Name => YamlDotNet
            // searchFileName => YamlDotNet.dll

            var searchFileName = new AssemblyName(args.Name).Name + ".dll";

            if (!File.Exists(temporaryDirectory + searchFileName))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(temporaryDirectory + searchFileName);

            return Assembly.Load(bytes);
        }
        #endregion
    }
}