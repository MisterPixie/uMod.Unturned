using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using SDG.Unturned;

namespace Oxide.Game.Unturned
{
    /// <summary>
    /// Game hooks and wrappers for the core Unturned plugin
    /// </summary>
    public partial class UnturnedCore
    {
        #region Player Hooks

        /// <summary>
        /// Called when the player has connected
        /// </summary>
        /// <param name="steamPlayer"></param>
        [HookMethod("OnPlayerConnected")]
        private void OnPlayerConnected(SteamPlayer steamPlayer)
        {
            string id = steamPlayer.playerID.steamID.ToString();

            // Update player's permissions group and name
            if (permission.IsLoaded)
            {
                permission.UpdateNickname(id, steamPlayer.playerID.playerName);
                OxideConfig.DefaultGroups defaultGroups = Interface.Oxide.Config.Options.DefaultGroups;
                if (!permission.UserHasGroup(id, defaultGroups.Players))
                {
                    permission.AddUserGroup(id, defaultGroups.Players);
                }

                if (steamPlayer.isAdmin && !permission.UserHasGroup(id, defaultGroups.Administrators))
                {
                    permission.AddUserGroup(id, defaultGroups.Administrators);
                }
            }

            // Let covalence know
            Covalence.PlayerManager.PlayerJoin(steamPlayer.playerID.steamID.m_SteamID, steamPlayer.playerID.playerName); // TODO: Move to OnUserApprove hook once available
            Covalence.PlayerManager.PlayerConnected(steamPlayer);

            IPlayer player = Covalence.PlayerManager.FindPlayerById(id);
            if (player != null)
            {
                // Set IPlayer object on SteamPlayer
                steamPlayer.IPlayer = player;

                // Call universal hook
                Interface.Call("OnUserConnected", player);
            }
        }

        /// <summary>
        /// Called when the player has disconnected
        /// </summary>
        /// <param name="index"></param>
        [HookMethod("IOnPlayerDisconnected")]
        private void IOnPlayerDisconnected(byte index)
        {
            SteamPlayer steamPlayer = Provider.clients[index];

            // Call hook for plugins
            Interface.Call("OnPlayerDisconnected", steamPlayer);

            IPlayer player = steamPlayer.IPlayer;
            if (player != null)
            {
                // Call universal hook
                Interface.Call("OnUserDisconnected", player);
            }

            // Let covalence know
            Covalence.PlayerManager.PlayerDisconnected(steamPlayer);
        }

        #endregion Player Hooks
    }
}
