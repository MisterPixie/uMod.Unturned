using System;
using uMod.Plugins;

namespace uMod.Unturned
{
    /// <summary>
    /// Responsible for loading the core Unturned plugin
    /// </summary>
    public class UnturnedPluginLoader : PluginLoader
    {
        public override Type[] CorePlugins => new[] { typeof(Unturned) };
    }
}
