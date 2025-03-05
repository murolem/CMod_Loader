using Cosmoteer.Mods;
using HarmonyLib;
using System.Reflection;

namespace CMod_Helper {
    static class Patcher {
        /// <summary>
        /// Invokes a method hook inside CMods.
        /// Meant to be called from a Harmony patch.
        /// </summary>
        /// <param name="hookName">Name of the hook method to invoke inside CMods.</param>
        /// <param name="patchType">Patch type.</param>
        /// <param name="original">Original patched method.</param>
        /// <param name="targetType">A name identifying a specific method in the source assembly. This is only used for logging.</param>
        public static void InvokeCModHook(string hookName, MethodInfo original, HarmonyPatchType patchType, string targetType) {
            FileLogger.LogInfo($"Invoking CMod hook {targetType} [{patchType}]");

            Main.InvokeCModsHook(hookName);

            Variables.harmony.Unpatch(original, patchType);

            FileLogger.Separator();
        }

        /// <summary>
        /// Assert that a method info is not null and return it.
        /// Throw `NullReferenceException` on a failed assertion.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static MethodInfo NullCoalesceOriginalMethodInfoOrThrow(MethodInfo? methodInfo) {
            return methodInfo ?? throw new NullReferenceException("Original method info not found while patching hooks from Helper.");
        }
    }

    //[HarmonyPatch(typeof(Cosmoteer.GameApp), MethodType.Constructor)]
    //[HarmonyPatch("ApplyPreLoadMods")]
    //[HarmonyPatchCategory("Core")]
    //static class Patch_GameAppCtor {
    //    public static void Postfix() {
    //        // run hooks patches practically is soon is possible.
    //        // this runs right after the game settings are set, which is required for fetching
    //        // enabled/disabled mods

    //        Main.DiscoverCMods();

    //        FileLogger.LogInfo("Patching CMod hooks");

    //        Variables.harmony.PatchCategory(Variables.assembly, "Hooks");
    //    }
    //}


    [HarmonyPatch(typeof(ModInfo))]
    [HarmonyPatch("ApplyPreLoadMods")]
    [HarmonyPatchCategory("Hooks")]
    static class Patch_ApplyPreLoadMods {
        public static void Prefix() {
            FileLogger.LogInfo("Earliest entrypoint. Running leftover initialization before continuing with the patched code.");

            Main.DiscoverCMods();

            //Variables.harmony.PatchCategory(Variables.assembly, "Hooks");

            //Halfling.App.Director.FrameStarting -= Director_FrameStarting;

            Patcher.InvokeCModHook(
                "Pre_ApplyPreLoadMods", // hook name
                Patcher.NullCoalesceOriginalMethodInfoOrThrow(typeof(ModInfo).GetMethod("ApplyPreLoadMods")), // original
                HarmonyPatchType.Prefix, // patch type
                "Cosmoteer.Mods.ModInfo.ApplyPreLoadMods()" // target type (only for logging)
            );
        }

        public static void Postfix() {
            Patcher.InvokeCModHook(
                "Post_ApplyPreLoadMods", // hook name
                Patcher.NullCoalesceOriginalMethodInfoOrThrow(typeof(ModInfo).GetMethod("ApplyPreLoadMods")), // original
                HarmonyPatchType.Postfix, // patch type
                "Cosmoteer.Mods.ModInfo.ApplyPreLoadMods()" // target type (only for logging)
            );
        }
    }

    [HarmonyPatch(typeof(ModInfo))]
    [HarmonyPatch("ApplyPostLoadMods")]
    [HarmonyPatchCategory("Hooks")]
    static class Patch_ApplyPostLoadMods {
        public static void Prefix() {
            Patcher.InvokeCModHook(
                "Pre_ApplyPostLoadMods", // hook name
                Patcher.NullCoalesceOriginalMethodInfoOrThrow(typeof(ModInfo).GetMethod("ApplyPostLoadMods")), // original
                HarmonyPatchType.Prefix, // patch type
                "Cosmoteer.Mods.ModInfo.ApplyPostLoadMods()" // target type (only for logging)
            );
        }

        public static void Postfix() {
            Patcher.InvokeCModHook(
                "Post_ApplyPostLoadMods", // hook name
                Patcher.NullCoalesceOriginalMethodInfoOrThrow(typeof(ModInfo).GetMethod("ApplyPostLoadMods")), // original
                HarmonyPatchType.Postfix, // patch type
                "Cosmoteer.Mods.ModInfo.ApplyPostLoadMods()" // target type (only for logging)
            );
        }
    }
}
