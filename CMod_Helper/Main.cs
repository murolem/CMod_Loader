using Cosmoteer;
using Cosmoteer.Mods;
using Halfling.Collections;
using Halfling.IO;
using HarmonyLib;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyVersion("0.0.2")]
[assembly: IgnoresAccessChecksTo("Cosmoteer")]

namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute {
        public IgnoresAccessChecksToAttribute(string assemblyName) {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}

namespace CMod_Helper {
    enum HelperLocation {
        LocalMods,
        WorkshopMods
    }

    public class Main {

        [UnmanagedCallersOnly]
        public static async void InitializePatches() {
            FileLogger.LogInfo("Beep boop");

            Variables.harmony = new Harmony("cmod_core.aliser.helper");
            Variables.assembly = Assembly.GetExecutingAssembly();

            Variables.harmony.PatchAll();
        }

        /// <summary>
        /// Scans for CMods.
        /// Meant to be called after Cosmoteer settings are loaded.
        /// </summary>
        public static void DiscoverCMods() {
            if(GameApp.IsNoModsMode) {
                FileLogger.LogInfo("Game is launched with mods disabled. No CMods will be loaded.");
                return;
            }

            FileLogger.LogInfo("Discovering Mods:");

            foreach((AbsolutePath absolutePath, ModInstallSource modInstallSource, string workshopId) in ModInfo.GetAllModFolders()) {
                string modInstallSourceStr;
                switch(modInstallSource) {
                    case ModInstallSource.BuiltIn:
                        modInstallSourceStr = "Built-In";
                        break;
                    case ModInstallSource.User:
                        modInstallSourceStr = "Local";
                        break;
                    case ModInstallSource.Workshop:
                        modInstallSourceStr = "Workshop";
                        break;
                    default:
                        modInstallSourceStr = "UNKNOWN";
                        break;
                }

                string modDirname = Utils.GetPathLastBit(absolutePath);

                if(!Utils.IsCModModDirectory(absolutePath)) {
                    FileLogger.LogInfo($"\t{modDirname} - Regular mod [{modInstallSourceStr}]");
                    continue;
                }

                if(!Settings.EnabledMods.Contains(absolutePath)) {
                    FileLogger.LogInfo($"\t{modDirname} - CMod, Disabled [{modInstallSourceStr}]");
                    continue;
                }

                FileLogger.LogInfo($"\t{modDirname} - CMod, TO BE LOADED [{modInstallSourceStr}]");

                string dllPath = Path.Combine(absolutePath, Variables.cModDllPathFromWithinCModDirectory);

                Variables.cModsToLoad.Add((dllPath, null, null));
            }

            FileLogger.Separator();
        }

        /// <summary>
        /// Loads CMods defined in the list of cmods to load and calling a specified method in 'Main' class inside of the predefined mod assembly.
        /// </summary>
        /// <param name="hookName"></param>
        /// <exception cref="Exception"></exception>
        public static void InvokeHookInActiveCMods(string hookName) {
            for(int i = 0; i < Variables.cModsToLoad.Count; i++) {
                (string dllAbsPath, Assembly? assembly, string? targetNamespace) = Variables.cModsToLoad[i];

                FileLogger.LogInfo("Processing CMod: " + dllAbsPath);

                // load mod assembly and find target namespace on first load call, save for further uses.
                if(assembly == null || targetNamespace == null) {
                    // using this thing instal of simple Assembly.Load* due to
                    // cmods having the same dll filename which one assembly method doesn't like, and second one doesn't load dependencies for.
                    // this creates a separate context that loads the mods and a custom dependency loader loads them in.
                    // custom dep. loader also allowed for the loading of Harmony dll from the Helper dir, which is pretty cool I must say.
                    ModAssemblyLoadContext assemblyLoadCtx = new ModAssemblyLoadContext();
                    assemblyLoadCtx.LoadFromAssemblyPath(dllAbsPath);
                    assemblyLoadCtx.Resolving += ModAssemblyLoadContext.ResolveDependencies;
                    assembly = ModAssemblyLoadContext.GetMainAssembly(assemblyLoadCtx);

                    string[] namespaceMatches = assembly.GetTypes()
                         .Select(t => t.Namespace)
                         .OfType<string>()
                         .Where(n => n.StartsWith("CModEntrypoint"))
                         .Distinct()
                         .ToArray();

                    if(namespaceMatches.Length == 0) {
                        string msg = $"Entrypoint namespace 'CModEntrypoint*' not found for CMod: {dllAbsPath}. Define a namespace starting with 'CModEntrypoint' and a public 'Main' class to supress the error.";
                        FileLogger.LogFatal(msg);
                        throw new Exception(msg);
                    } else if(namespaceMatches.Length > 1) {
                        string msg = $"Found multiple entrypoint namespaces 'CModEntrypoint*' for CMod: {dllAbsPath}. Make sure only one such namespace is defined.";
                        FileLogger.LogFatal(msg);
                        throw new Exception(msg);
                    }

                    targetNamespace = namespaceMatches[0];

                    Variables.cModsToLoad[i] = (dllAbsPath, assembly, targetNamespace);
                }

                Type? classType = assembly.GetType($"{targetNamespace}.Main");
                if(classType == null) {
                    string msg = $"'Main' class not found in CMod: {dllAbsPath}. Make sure the class is defined and public.";
                    FileLogger.LogFatal(msg);
                    throw new Exception(msg);
                }

                MethodInfo? methodInfo = classType.GetMethod(hookName, BindingFlags.Static | BindingFlags.Public);
                if(methodInfo == null) {
                    FileLogger.LogDebug($"Hook not defined. Skipping.");
                    continue;
                }

                FileLogger.LogInfo($"Invoking {hookName}()");

                methodInfo.Invoke(null, null);
            }
        }
    }
}

