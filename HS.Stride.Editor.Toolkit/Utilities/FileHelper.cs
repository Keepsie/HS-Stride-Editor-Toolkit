// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

namespace HS.Stride.Editor.Toolkit.Utilities
{
    /// <summary>
    /// Helper methods for common file operations with built-in error handling
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Saves text content to a file, creating directories if needed
        /// </summary>
        public static bool SaveFile(string content, string filePath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
                File.WriteAllText(filePath, content);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a directory if it doesn't exist
        /// </summary>
        public static bool EnsureDirectoryExists(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Copies a file from source to destination, creating directories if needed
        /// </summary>
        public static bool CopyFile(string sourcePath, string destinationPath, bool overwrite = true)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? ".");
                File.Copy(sourcePath, destinationPath, overwrite);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Computes the SHA256 hash of a file
        /// </summary>
        public static string GetFileHash(string filePath)
        {
            try
            {
                using var sha256 = System.Security.Cryptography.SHA256.Create();
                using var stream = File.OpenRead(filePath);
                var hashBytes = sha256.ComputeHash(stream);
                return Convert.ToHexString(hashBytes);
            }
            catch (IOException)
            {
                return string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
        }
    }
}
