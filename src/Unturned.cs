using uMod.Libraries;
using uMod.Libraries.Universal;
using uMod.Logging;
using uMod.Plugins;

namespace uMod.Unturned
{
    /// <summary>
    /// The core Unturned plugin
    /// </summary>
    public partial class Unturned : CSPlugin
    {
        #region Initialization

        /// <summary>
        /// Initializes a new instance of the Unturned class
        /// </summary>
        public Unturned()
        {
            // Set plugin info attributes
            Title = "Unturned";
            Author = UnturnedExtension.AssemblyAuthors;
            Version = UnturnedExtension.AssemblyVersion;
        }

        // Instances
        internal static readonly UnturnedProvider Universal = UnturnedProvider.Instance;
        internal readonly IServer Server = Universal.CreateServer();

        // Libraries
        internal readonly Lang lang = Interface.uMod.GetLibrary<Lang>();
        internal readonly Permission permission = Interface.uMod.GetLibrary<Permission>();

        private bool serverInitialized;

        #endregion Initialization

        #region Core Hooks

        /// <summary>
        /// Called when the plugin is initializing
        /// </summary>
        [HookMethod("Init")]
        private void Init()
        {
            // Configure remote error logging
            RemoteLogger.SetTag("game", Title.ToLower());
            RemoteLogger.SetTag("game version", Server.Version);
        }

        /// <summary>
        /// Called when another plugin has been loaded
        /// </summary>
        /// <param name="plugin"></param>
        [HookMethod("OnPluginLoaded")]
        private void OnPluginLoaded(Plugin plugin)
        {
            if (serverInitialized)
            {
                // Call OnServerInitialized for hotloaded plugins
                plugin.CallHook("OnServerInitialized", false);
            }
        }

        /// <summary>
        /// Called when the server is first initialized
        /// </summary>
        [HookMethod("IOnServerInitialized")]
        private void IOnServerInitialized()
        {
            if (!serverInitialized)
            {
                Analytics.Collect();

                // Show the server console, if enabled
                UnturnedExtension.ServerConsole();

                serverInitialized = true;

                // Let plugins know server startup is complete
                Interface.CallHook("OnServerInitialized", serverInitialized);
            }
        }

        /// <summary>
        /// Called when the server is saved
        /// </summary>
        [HookMethod("OnServerSave")]
        private void OnServerSave()
        {
            Interface.uMod.OnSave();

            // Save groups, users, and other data
            Universal.PlayerManager.SavePlayerData();
        }

        /// <summary>
        /// Called when the server is shutting down
        /// </summary>
        [HookMethod("OnServerShutdown")]
        private void OnServerShutdown()
        {
            Interface.uMod.OnShutdown();

            // Save groups, users, and other data
            Universal.PlayerManager.SavePlayerData();
        }

        #endregion Core Hooks
    }
}
