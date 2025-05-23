using System;
using System.Reflection;
using System.Threading.Tasks;

using static ArmaExtension.Logger;

namespace ArmaExtension;

public static partial class Extension {
    private static bool _initialized = false;
    
    private static bool InitializePlugins() {
        if (_initialized) return false;

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies) {
            foreach (var type in assembly.GetTypes()) {

                // Find public static method called "Main" with no params or string[] args
#pragma warning disable IL2075
                var method = type.GetMethod("Main", BindingFlags.Public | BindingFlags.Static);
#pragma warning restore IL2075

                if (method != null) {
                    var parameters = new string[] { Version, ExtensionName, AssemblyDirectory };
                    // TODO What parameters should we send?

                    try {
                        if (method.ReturnType == typeof(Task)) {
                            Task.Run(async () => {
                                try {
                                    var task = method.Invoke(null, parameters) as Task;
                                    if (task != null) await task;
                                } catch (Exception ex) {
                                    Log($"Exception in fire-and-forget async Main(): {ex}");
                                }
                            });
                        } else {
                            method.Invoke(null, parameters);
                        }
                    } catch (Exception ex) {
                        Log($"Exception while calling Main() on {type.FullName}: {ex}");
                    }
                }
            }
        }

        _initialized = true;

        return true;
    }
}