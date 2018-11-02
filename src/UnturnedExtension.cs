using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using uMod.Extensions;
using uMod.Plugins;
using uMod.Unity;
using UnityEngine;

namespace uMod.Unturned
{
    /// <summary>
    /// The extension class that represents this extension
    /// </summary>
    public class UnturnedExtension : Extension
    {
        // Get assembly info
        internal static Assembly Assembly = Assembly.GetExecutingAssembly();
        internal static AssemblyName AssemblyName = Assembly.GetName();
        internal static VersionNumber AssemblyVersion = new VersionNumber(AssemblyName.Version.Major, AssemblyName.Version.Minor, AssemblyName.Version.Build);
        internal static string AssemblyAuthors = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly, typeof(AssemblyCompanyAttribute), false)).Company;

        /// <summary>
        /// Gets whether this extension is for a specific game
        /// </summary>
        public override bool IsGameExtension => true;

        /// <summary>
        /// Gets the name of this extension
        /// </summary>
        public override string Name => "Unturned";

        /// <summary>
        /// Gets the author of this extension
        /// </summary>
        public override string Author => AssemblyAuthors;

        /// <summary>
        /// Gets the version of this extension
        /// </summary>
        public override VersionNumber Version => AssemblyVersion;

        /// <summary>
        /// Gets the branch of this extension
        /// </summary>
        public override string Branch => "public"; // TODO: Handle this programmatically

        // Commands that a plugin can't override
        internal static IEnumerable<string> RestrictedCommands => new[]
        {
            ""
        };

        /// <summary>
        /// Default game-specific references for use in plugins
        /// </summary>
        public override string[] DefaultReferences => new[]
        {
            ""
        };

        /// <summary>
        /// List of assemblies allowed for use in plugins
        /// </summary>
        public override string[] WhitelistAssemblies => new[]
        {
            "Assembly-CSharp", "mscorlib", "uMod", "System", "System.Core", "UnityEngine"
        };

        /// <summary>
        /// List of namespaces allowed for use in plugins
        /// </summary>
        public override string[] WhitelistNamespaces => new[]
        {
            "ProtoBuf", "Steamworks", "System.Collections", "System.Security.Cryptography", "System.Text", "UnityEngine"
        };

        /// <summary>
        /// List of filter matches to apply to console output
        /// </summary>
        public static string[] Filter =
        {
            "The image effect Camera"
        };

        /// <summary>
        /// Initializes a new instance of the UnturnedExtension class
        /// </summary>
        /// <param name="manager"></param>
        public UnturnedExtension(ExtensionManager manager) : base(manager)
        {
        }

        /// <summary>
        /// Loads this extension
        /// </summary>
        public override void Load()
        {
            Manager.RegisterPluginLoader(new UnturnedPluginLoader());
        }

        /// <summary>
        /// Loads plugin watchers used by this extension
        /// </summary>
        /// <param name="directory"></param>
        public override void LoadPluginWatchers(string directory)
        {
        }

        /// <summary>
        /// Called when all other extensions have been loaded
        /// </summary>
        public override void OnModLoad()
        {
            CSharpPluginLoader.PluginReferences.UnionWith(DefaultReferences);

            // Limit FPS to reduce CPU usage
            Application.targetFrameRate = 256;

            if (Interface.uMod.EnableConsole())
            {
                Application.logMessageReceived += HandleLog;

                Interface.uMod.ServerConsole.Input += ServerConsoleOnInput;
            }
        }

        internal static void ServerConsole()
        {
            if (Interface.uMod.ServerConsole == null)
            {
                return;
            }

            Interface.uMod.ServerConsole.Title = () => $"{Provider.clients.Count} | {Provider.serverName}";

            Interface.uMod.ServerConsole.Status1Left = () => Provider.serverName;
            Interface.uMod.ServerConsole.Status1Right = () =>
            {
                TimeSpan time = TimeSpan.FromSeconds(Time.realtimeSinceStartup);
                string uptime = $"{time.TotalHours:00}h{time.Minutes:00}m{time.Seconds:00}s".TrimStart(' ', 'd', 'h', 'm', 's', '0');
                return $"{Mathf.RoundToInt(1f / Time.smoothDeltaTime)}fps, {uptime}";
            };

            Interface.uMod.ServerConsole.Status2Left = () => $"{Provider.clients.Count}/{Provider.maxPlayers} players";
            Interface.uMod.ServerConsole.Status2Right = () =>
            {
                string bytesReceived = Utility.FormatBytes(Provider.bytesReceived);
                string bytesSent = Utility.FormatBytes(Provider.bytesSent);
                return Provider.time <= 0 ? "0b/s in, 0b/s out" : $"{bytesReceived}/s in, {bytesSent}/s out";
            };

            Interface.uMod.ServerConsole.Status3Left = () =>
            {
                string gameTime = DateTime.Today.AddSeconds(LightingManager.time * 120).ToString("h:mm tt");
                return $"{gameTime.ToLower()}, {Provider.map ?? "Unknown"}";
            };
            Interface.uMod.ServerConsole.Status3Right = () => $"uMod.Unturned {AssemblyVersion}";
            Interface.uMod.ServerConsole.Status3RightColor = ConsoleColor.Yellow;
        }

        private static void ServerConsoleOnInput(string input)
        {
            input = input.Trim();
            if (!string.IsNullOrEmpty(input))
            {
                Commander.execute(CSteamID.Nil, input);
            }
        }

        private static void HandleLog(string message, string stackTrace, LogType logType)
        {
            if (!string.IsNullOrEmpty(message) && !Filter.Any(message.StartsWith))
            {
                Interface.uMod.RootLogger.HandleMessage(message, stackTrace, logType.ToLogType());
            }
        }
    }
}
