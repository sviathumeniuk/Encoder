using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.Windows.Forms;

public class FileViewModel : INotifyPropertyChanged
{
    private string _selectedFile = string.Empty;
    public string SelectedFile
    {
        get { return _selectedFile; }
        set
        {
            _selectedFile = value;
            OnPropertyChanged(nameof(SelectedFile));
        }
    }

    private string _selectedEncoding;
    public string SelectedEncoding
    {
        get => _selectedEncoding;
        set
        {
            _selectedEncoding = value;
            OnPropertyChanged(nameof(SelectedEncoding));
        }
    }

    private string _selectedCompression;
    public string SelectedCompression
    {
        get => _selectedCompression;
        set
        {
            _selectedCompression = value;
            OnPropertyChanged(nameof(SelectedCompression));
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged(nameof(Password));
        }
    }

    private bool _isAddFolderMode;
    public bool IsAddFolderMode
    {
        get => _isAddFolderMode;
        set
        {
            _isAddFolderMode = value;
            OnPropertyChanged(nameof(IsAddFolderMode));
        }
    }

    public ObservableCollection<string> Files { get; set; }
    public List<string> Encodings { get; set; }
    public List<string> Compressions { get; set; }
    public ICommand RemoveEntityCommand { get; }
    public ICommand ShowMetadataCommand { get; }
    public ICommand EncryptFileCommand { get; }
    public ICommand DecryptFileCommand { get; }
    public ICommand CompressFileCommand { get; }
    public ICommand DecompressFileCommand { get; }
    public ICommand AddEntityCommand { get; }

    private readonly AesEncryption _aesEncryption;
    private readonly BlowfishEncryption _blowfishEncryption;
    private readonly ZipCompression _zipCompression;

    public FileViewModel()
    {
        _aesEncryption = new AesEncryption();
        _blowfishEncryption = new BlowfishEncryption();
        _zipCompression = new ZipCompression();

        Files = new ObservableCollection<string>();
        Encodings = new List<string> { "AES", "BlowFish" };
        Compressions = new List<string> { "Zip" };

        RemoveEntityCommand = new RelayCommand(RemoveEntity, CanRemoveEntity);
        ShowMetadataCommand = new RelayCommand(ShowMetadata, CanShowMetadata);
        EncryptFileCommand = new RelayCommand(EncryptFile, CanEncryptOrDecrypt);
        DecryptFileCommand = new RelayCommand(DecryptFile, CanEncryptOrDecrypt);
        CompressFileCommand = new RelayCommand(CompressFile, CanCompressOrDecompress);
        DecompressFileCommand = new RelayCommand(DecompressFile, CanCompressOrDecompress);
        AddEntityCommand = new RelayCommand(AddEntity, CanAddEntity);
    }

    private void ShowMessage(string message, string title = "Information", MessageBoxImage icon = MessageBoxImage.Information)
    {
        System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, icon);
    }

    private void AddEntity(object parameter)
    {
        if (IsAddFolderMode)
        {
            AddFolder();
        }
        else
        {
            AddFile();
        }
    }

    public void AddFile()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog();
        if (openFileDialog.ShowDialog() == true)
        {
            string selectedFilePath = openFileDialog.FileName;
            if (!Files.Contains(selectedFilePath))
            {
                Files.Add(selectedFilePath);
            }
        }
    }

    public void AddFolder()
    {
        using var folderBrowserDialog = new FolderBrowserDialog { Description = "Select Folder" };
        if (folderBrowserDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
        {
            string selectedFolderPath = folderBrowserDialog.SelectedPath;
            if (!Files.Contains(selectedFolderPath))
            {
                Files.Add(selectedFolderPath);
            }
        }
    }

    private bool CanAddEntity(object parameter) => !Files.Contains((string)parameter);

    private void RemoveEntity(object parameter) => Files.Remove((string)parameter);

    private bool CanRemoveEntity(object parameter) => parameter is string file && Files.Contains(file);

    private void ShowMetadata(object parameter)
    {
        ExecuteWithExceptionHandling(() =>
        {
            string path = (string)parameter;
            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);
                string metadata = $"Directory: {dirInfo.Name}\nCreated: {dirInfo.CreationTime}\nLast Access Time: {dirInfo.LastAccessTime}\nLast Write Time: {dirInfo.LastWriteTime}\nFile Count: {dirInfo.GetFiles().Length}\nDirectory Count: {dirInfo.GetDirectories().Length}";
                ShowMessage(metadata, "Directory Metadata");
            }
            else if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                string metadata = $"File: {fileInfo.Name}\nSize: {fileInfo.Length} bytes\nCreated: {fileInfo.CreationTime}\nLast Access Time: {fileInfo.LastAccessTime}\nLast Write Time: {fileInfo.LastWriteTime}\nIsReadOnly: {fileInfo.IsReadOnly}\nFileAttributes: {fileInfo.Attributes}\nExtension: {fileInfo.Extension}";
                ShowMessage(metadata, "File Metadata");
            }
        });
    }

    private bool CanShowMetadata(object parameter) => parameter is string path && (Directory.Exists(path) || File.Exists(path));

    private void EncryptFile(object parameter)
    {
        ExecuteWithExceptionHandling(() =>
        {
            if (File.Exists((string)parameter))
            {
                switch (SelectedEncoding)
                {
                    case "AES":
                        _aesEncryption.EncryptFile((string)parameter, Password);
                        break;
                    case "BlowFish":
                        _blowfishEncryption.EncryptFile((string)parameter, Password);
                        break;
                }
                ShowMessage($"File {(string)parameter} has been encrypted.", "Success");
            }
            else
            {
                ShowMessage("Encryption works only for files, not folders.", "Error", MessageBoxImage.Warning);
            }
        });
    }

    private void DecryptFile(object parameter)
    {
        ExecuteWithExceptionHandling(() =>
        {
            if (File.Exists((string)parameter))
            {
                switch (SelectedEncoding)
                {
                    case "AES":
                        _aesEncryption.DecryptFile((string)parameter, Password);
                        break;
                    case "BlowFish":
                        _blowfishEncryption.DecryptFile((string)parameter, Password);
                        break;
                }
                ShowMessage($"File {(string)parameter} successfully decrypted.", "Success");
            }
            else
            {
                ShowMessage("Decryption works only for files, not folders.", "Error", MessageBoxImage.Warning);
            }
        });
    }

    private bool CanEncryptOrDecrypt(object parameter) => !string.IsNullOrEmpty(SelectedFile);

    private void CompressFile(object parameter)
    {
        ExecuteWithExceptionHandling(() =>
        {
            string path = (string)parameter;
            if (Directory.Exists(path) || File.Exists(path))
            {
                _zipCompression.CreateZip(path);
            }
            else
            {
                ShowMessage("The specified path does not exist.", "Error", MessageBoxImage.Error);
            }
        });
    }

    private void DecompressFile(object parameter)
    {
        ExecuteWithExceptionHandling(() =>
        {
            string path = (string)parameter;
            if (File.Exists(path) && Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string outputDirectory = Path.GetDirectoryName(path); // Set to the same directory as the ZIP file
                _zipCompression.ExtractZip(path, outputDirectory);
            }
            else
            {
                ShowMessage("The specified file is not a valid ZIP archive.", "Error", MessageBoxImage.Error);
            }
        });
    }

    private bool CanCompressOrDecompress(object parameter) => !string.IsNullOrEmpty(SelectedFile);

    private void ExecuteWithExceptionHandling(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            ShowMessage($"An error occurred: {ex.Message}", "Error", MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}