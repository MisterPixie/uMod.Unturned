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
                permission.UpdateNickname(id, steamPlayer.player.name);
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

            Covalence.PlayerManager.PlayerJoin(steamPlayer.playerID.steamID.m_SteamID, steamPlayer.player.name); // TODO: Move to OnUserApprove hook once available
            Covalence.PlayerManager.PlayerConnected(steamPlayer);
            IPlayer iplayer = Covalence.PlayerManager.FindPlayerById(id);
            if (iplayer != null)
            {
                Interface.Call("OnUserConnected", iplayer);
            }
        }

        #endregion Player Hooks
    }
}
