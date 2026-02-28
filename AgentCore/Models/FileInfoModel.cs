using System;
using System.Text;

namespace CefDotnetApp.AgentCore.Models
{
    public class FileInfoModel
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsReadOnly { get; set; }
        public bool Exists { get; set; }

        public FileInfoModel()
        {
        }

        public FileInfoModel(string path)
        {
            if (System.IO.File.Exists(path)) {
                var fileInfo = new System.IO.FileInfo(path);
                Path = fileInfo.FullName;
                Name = fileInfo.Name;
                Extension = fileInfo.Extension;
                Size = fileInfo.Length;
                CreationTime = fileInfo.CreationTime;
                LastWriteTime = fileInfo.LastWriteTime;
                LastAccessTime = fileInfo.LastAccessTime;
                IsDirectory = false;
                IsReadOnly = fileInfo.IsReadOnly;
                Exists = true;
            }
            else if (System.IO.Directory.Exists(path)) {
                var dirInfo = new System.IO.DirectoryInfo(path);
                Path = dirInfo.FullName;
                Name = dirInfo.Name;
                Extension = string.Empty;
                Size = 0;
                CreationTime = dirInfo.CreationTime;
                LastWriteTime = dirInfo.LastWriteTime;
                LastAccessTime = dirInfo.LastAccessTime;
                IsDirectory = true;
                IsReadOnly = false;
                Exists = true;
            }
            else {
                Path = path;
                Name = System.IO.Path.GetFileName(path);
                Extension = System.IO.Path.GetExtension(path);
                Size = 0;
                CreationTime = DateTime.MinValue;
                LastWriteTime = DateTime.MinValue;
                LastAccessTime = DateTime.MinValue;
                IsDirectory = false;
                IsReadOnly = false;
                Exists = false;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Path:");
            sb.Append(Path);
            sb.Append(' ');
            sb.Append("Name:");
            sb.Append(Name);
            sb.Append(' ');
            sb.Append("Extension:");
            sb.Append(Extension);
            sb.Append(' ');
            sb.Append("Size:");
            sb.Append(Size);
            sb.Append(' ');
            sb.Append("Create:");
            sb.Append(CreationTime);
            sb.Append(' ');
            sb.Append("LastWrite:");
            sb.Append(LastWriteTime);
            sb.Append(' ');
            sb.Append("LastAccess:");
            sb.Append(LastAccessTime);
            sb.Append(' ');
            sb.Append("IsDirectory:");
            sb.Append(IsDirectory);
            sb.Append(' ');
            sb.Append("IsReadOnly:");
            sb.Append(IsReadOnly);
            sb.Append(' ');
            sb.Append("Exists:");
            sb.Append(Exists);
            return sb.ToString();
        }
    }
}
