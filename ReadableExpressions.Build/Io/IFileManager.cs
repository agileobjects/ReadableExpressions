namespace AgileObjects.ReadableExpressions.Build.Io
{
    internal interface IFileManager
    {
        bool Exists(string filePath);

        string Read(string filePath);

        void Write(string filePath, string contents);
    }
}
