using System;
//using DotNetSerializationUtilities;

namespace ApiInspector.Bootstrapper
{
    class Config
    {
        #region Public Properties
        public string BatFileContent { get; set; }
        public string ExportDir      { get; set; }
        public string ProcessPath    { get; set; }
        public string ZipFilePath    { get; set; }
        public string ZipFileUrl     { get; set; }
        #endregion

        #region Public Methods
        public static Config CreateFromFile()
        {
            throw new NotImplementedException();
            // return YamlHelper.DeserializeFromFile<Config>(nameof(Config) + ".yaml");
        }
        #endregion
    }
}