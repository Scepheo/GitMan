using System.IO;

namespace GitMan
{
    internal class Repository
    {
        public string Name { get; }
        public string FullName { get; }

        private Repository(DirectoryInfo directoryInfo)
        {
            Name = directoryInfo.Name;
            FullName = directoryInfo.FullName;
        }

        public static Repository Load(DirectoryInfo directoryInfo)
        {
            return new Repository(directoryInfo);
        }
    }
}
