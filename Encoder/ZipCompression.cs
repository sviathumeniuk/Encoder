using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

public class ZipCompression
{
    public void CreateZip(string sourcePath)
    {
        string destinationFile;

        if (File.Exists(sourcePath))
        {
            destinationFile = Path.Combine(Path.GetDirectoryName(sourcePath), Path.GetFileNameWithoutExtension(sourcePath) + ".zip");
            if (File.Exists(destinationFile))
            {
                System.Windows.MessageBox.Show("Such archive already exists", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            using FileStream zipToOpen = new(destinationFile, FileMode.Create);
            using ZipArchive archive = new(zipToOpen, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(sourcePath, Path.GetFileName(sourcePath));
            System.Windows.MessageBox.Show("Successful archivation.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else if (Directory.Exists(sourcePath))
        {
            destinationFile = Path.Combine(Path.GetDirectoryName(sourcePath), new DirectoryInfo(sourcePath).Name + ".zip");
            if (File.Exists(destinationFile))
            {
                System.Windows.MessageBox.Show("Such archive already exists", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ZipFile.CreateFromDirectory(sourcePath, destinationFile);
            System.Windows.MessageBox.Show("Successful archivation.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        else
        {
            System.Windows.MessageBox.Show("Invalid path provided.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
    }

    public void ExtractZip(string sourceZipFile, string outputDirectory = null)
    {
        if (!File.Exists(sourceZipFile))
        {
            System.Windows.MessageBox.Show("The specified archive does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string destinationDirectory = Path.GetDirectoryName(sourceZipFile);
        using (var archive = ZipFile.OpenRead(sourceZipFile))
        {
            foreach (var entry in archive.Entries)
            {
                string destinationPath = Path.Combine(destinationDirectory, entry.FullName);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                if (!entry.FullName.EndsWith("/"))
                {
                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
            }
        }

        System.Windows.MessageBox.Show("Extraction completed in the same folder as the ZIP file.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}