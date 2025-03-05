using Halfling.IO;
using System.Reflection;

namespace CMod_Helper {
    static class Utils {
        /// <summary>
        /// Get path to the current mod root directory.
        /// </summary>
        /// <returns></returns>
        public static string GetPathToModRoot() {
            // this returns path to the directory, desipite the function saying "Name"
            string? path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if(path == null) {
                throw new NullReferenceException("failed to get the current assembly");
            }

            return path;
        }

        /// Checks whether given directory is a valid CMod directory.
        /// </summary>
        /// <returns></returns>
        public static bool IsCModModDirectory(AbsolutePath path) {
            return File.Exists(Path.Combine(path, Variables.cModDllPathFromWithinCModDirectory));
        }

        /// <summary>
        /// Returns the last segment of a path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPathLastBit(AbsolutePath path) {
            return path.ToString().Split(Path.DirectorySeparatorChar).Last();
        }

        /// <summary>
        /// Checks if the given path is a directory.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="DirectoryNotFoundException">When the given path doesn't exist.</exception>
        public static bool IsDirectory(string path) {
            if(!Path.Exists(path)) {
                throw new DirectoryNotFoundException(path);
            }

            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

    }
}