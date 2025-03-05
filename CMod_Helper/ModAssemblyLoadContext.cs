using System.Reflection;
using System.Runtime.Loader;

namespace CMod_Helper {
    class ModAssemblyLoadContext : AssemblyLoadContext {
        /// <summary>
        /// TODO
        /// Libs shipped with the Helper mod.
        /// 
        /// Can be used as dependency fallback in CMods.
        /// </summary>
        //static readonly string[] cModHelperModDependencyFallbackDlls = ["0Harmony.dll"];

        public ModAssemblyLoadContext()
        : base("ModAssemblyLoadContext") { }

        /// <summary>
        /// Returns the main assembly (it should be loaded first though).
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static Assembly GetMainAssembly(AssemblyLoadContext ctx) {
            Assembly? mainAssembly = ctx.Assemblies.FirstOrDefault();
            if(mainAssembly == null) {
                string msg = $"Failed to extract the main assembly";
                FileLogger.LogFatal(msg);
                throw new NullReferenceException(msg);
            }

            return mainAssembly;
        }

        /// <summary>
        /// Resolves dependencies for a given assembly.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="depAssemblyName"></param>
        /// <returns></returns>
        /// <exception cref="DllNotFoundException"></exception>
        public static Assembly? ResolveDependencies(AssemblyLoadContext ctx, AssemblyName depAssemblyName) {
            string depAssemblyFilename = depAssemblyName.Name + ".dll";

            FileLogger.LogDebug("Resolving assembly dependency: " + depAssemblyFilename);

            Assembly mainAssembly = GetMainAssembly(ctx);
            string mainAssemblyLoadedFrom = Path.GetDirectoryName(mainAssembly.Location);

            // first, look through the mod libs
            string depAssemblyPathAlongMainAssembly = Path.Combine(mainAssemblyLoadedFrom, depAssemblyFilename);
            if(File.Exists(depAssemblyPathAlongMainAssembly)) {
                FileLogger.LogDebug("Found along with mod DLL at: " + depAssemblyPathAlongMainAssembly);
                return Assembly.LoadFrom(depAssemblyPathAlongMainAssembly);
            }

            // if not found, look through fallback libs shipped with Helper
            string cModHelperModDirPath = Utils.GetPathToModRoot();
            string depAssemblyPathAlongCModHelperModDir = Path.Combine(cModHelperModDirPath, depAssemblyFilename);
            if(File.Exists(depAssemblyPathAlongCModHelperModDir)) {
                FileLogger.LogDebug("Found in CMod Helper mod dir at: " + depAssemblyPathAlongCModHelperModDir);
                return Assembly.LoadFrom(depAssemblyPathAlongCModHelperModDir);
            }

            string msg = $"Failed to load CMod DLL at: {mainAssembly.Location}. Failed to locate a dependency. Tried paths: \n- {depAssemblyPathAlongMainAssembly} \n- {depAssemblyPathAlongCModHelperModDir}";
            FileLogger.LogFatal(msg);
            throw new DllNotFoundException(msg);
        }
    }
}
