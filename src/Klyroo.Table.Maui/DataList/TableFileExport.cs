using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;

namespace Klyroo.Table.Maui;

/// <summary>Writes an export where the system can share it from: MAUI has no file saver, so the file goes to the cache directory and the share sheet takes it from there.</summary>
internal static class TableFileExport
{
    /// <summary>Writes the file to the cache directory of the app and opens the share sheet for it.</summary>
    public static async Task ShareAsync(DataListExportResult result)
    {
        var path = await WriteAsync(result, FileSystem.CacheDirectory);
        await Share.Default.RequestAsync(new ShareFileRequest { Title = result.FileName, File = new ShareFile(path, result.MimeType) });
    }

    /// <summary>Writes the file in <paramref name="directory"/> (the UTF-8 bytes of the export, with its byte order mark when the options asked for one) and returns its path.</summary>
    public static async Task<string> WriteAsync(DataListExportResult result, string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, Path.GetFileName(result.FileName));
        await File.WriteAllBytesAsync(path, result.ToBytes());
        return path;
    }
}
