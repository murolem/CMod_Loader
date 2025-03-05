using HarmonyLib;
using System.Reflection;

namespace CMod_Helper {
    static class Variables {
        public static List<(string dllAbsPath, Assembly? assembly, string? targetNamespace)> cModsToLoad = new();
        public static Harmony harmony;
        public static Assembly assembly;

        public static string cModDllPathFromWithinCModDirectory = Path.Combine("CMod", "Main.dll");
    }
}
