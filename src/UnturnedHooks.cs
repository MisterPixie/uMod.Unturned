using SDG.Unturned;
using uMod.Configuration;
using uMod.Libraries.Universal;
using uMod.Plugins;

namespace uMod.Unturned
{
    /// <summary>
    /// Game hooks and wrappers for the core Unturned plugin
    /// </summary>
    public partial class Unturned
    {
        #region Player Hooks

        /// <summary>
        /// Called when the player has connected
        /// </summary>
        /// <param name="steamPlayer"></param>
        [HookMethod("OnPlayerConnected")]
        private void OnPlayerConnected(SteamPlayer steamPlayer)
        {
            string userId = steamPlayer.playerID.steamID.ToString();

            if (permission.IsLoaded)
            {
                // Update player's stored username
                permission.UpdateNickname(userId, steamPlayer.playerID.playerName);

                // Set default groups, if necessary
                uModConfig.DefaultGroups defaultGroups = Interface.uMod.Config.Options.DefaultGroups;
                if (!permission.UserHasGroup(userId, defaultGroups.Players))
                {
                    permission.AddUserGroup(userId, defaultGroups.Players);
                }
                if (steamPlayer.isAdmin && !permission.UserHasGroup(userId, defaultGroups.Administrators))
                {
                    permission.AddUserGroup(userId, defaultGroups.Administrators);
                }
            }

            // Let universal know
            Universal.PlayerManager.PlayerJoin(steamPlayer.playerID.steamID.m_SteamID, steamPlayer.playerID.playerName); // TODO: Move to OnUserApprove hook once available
            Universal.PlayerManager.PlayerConnected(steamPlayer);

            IPlayer player = Universal.PlayerManager.FindPlayerById(userId);
            if (player != null)
            {
                // Set IPlayer object on SteamPlayer
                steamPlayer.IPlayer = player;

                // Call universal hook
                Interface.Call("OnPlayerConnected", player);
            }
        }

        /// <summary>
        /// Called when the player has disconnected
        /// </summary>
        /// <param name="index"></param>
        [HookMethod("IOnPlayerDisconnected")]
        private void IOnPlayerDisconnected(byte index)
        {
            // Get SteamPlayer object
            SteamPlayer steamPlayer = Provider.clients[index];

            // Let universal know
            Universal.PlayerManager.PlayerDisconnected(steamPlayer);

            // Call game-specific hook
            Interface.Call("OnPlayerDisconnected", steamPlayer);

            IPlayer player = steamPlayer.IPlayer;
            if (player != null)
            {
                // Call universal hook
                Interface.Call("OnPlayerDisconnected", player);
            }
        }

        #endregion Player Hooks
    }
}
