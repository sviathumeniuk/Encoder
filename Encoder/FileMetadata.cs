using System.IO;

public class FileMetadata
{
    public string FileName { get; set; } 
    public long FileSize { get; set; } 
    public DateTime CreationTime { get; set; } 
    public DateTime LastAccessTime { get; set; } 
    public DateTime LastWriteTime { get; set; }
    public bool IsReadOnly { get; set; }
    public FileAttributes Attributes { get; set; }
    public string Extension { get; set; }

    public static FileMetadata GetMetadata (string filePath)
    {
        var fileInfo = new FileInfo (filePath);
        return new FileMetadata
        {
            FileName = fileInfo.Name,
            FileSize = fileInfo.Length,
            CreationTime = fileInfo.CreationTime,
            LastAccessTime = fileInfo.LastAccessTime,
            LastWriteTime = fileInfo.LastWriteTime,
            IsReadOnly = fileInfo.IsReadOnly,
            Attributes = fileInfo.Attributes,
            Extension = fileInfo.Extension
        };
    }
}