namespace AgileObjects.ReadableExpressions.Build.Io
{
    using System.IO;

    internal class BclFileManager : IFileManager
    {
        public static readonly IFileManager Instance = new BclFileManager();

        public bool Exists(string filePath) => new FileInfo(filePath).Exists;

        public string Read(string filePath) => File.ReadAllText(filePath);

        public void Write(string filePath, string contents)
            => File.WriteAllText(filePath, contents);
    }
}