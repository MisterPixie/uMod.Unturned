using SDG.Unturned;
using Steamworks;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using uMod.Libraries.Universal;
using uMod.Logging;

namespace uMod.Unturned
{
    /// <summary>
    /// Represents the server hosting the game instance
    /// </summary>
    public class UnturnedServer : IServer
    {
        #region Information

        /// <summary>
        /// Gets/sets the public-facing name of the server
        /// </summary>
        public string Name
        {
            get => Provider.serverName;
            set => Provider.serverName = value;
        }

        private static IPAddress address;
        private static IPAddress localAddress;

        /// <summary>
        /// Gets the public-facing IP address of the server, if known
        /// </summary>
        public IPAddress Address
        {
            get
            {
                try
                {
                    if (address == null)
                    {
                        string providerIp = Parser.getIPFromUInt32(Provider.ip);
                        if (Utility.ValidateIPv4(providerIp) && !Utility.IsLocalIP(providerIp))
                        {
                            IPAddress.TryParse(providerIp, out address);
                            Interface.uMod.LogInfo($"IP address from command-line: {address}");
                        }
                        else if (SteamGameServer.GetPublicIP() > 0)
                        {
                            IPAddress.TryParse(Parser.getIPFromUInt32(SteamGameServer.GetPublicIP()), out address);
                            Interface.uMod.LogInfo($"IP address from Steam query: {address}");
                        }
                        else
                        {
                            WebClient webClient = new WebClient();
                            IPAddress.TryParse(webClient.DownloadString("http://api.ipify.org"), out address);
                            Interface.uMod.LogInfo($"IP address from external API: {address}");
                        }
                    }

                    return address;
                }
                catch (Exception ex)
                {
                    RemoteLogger.Exception("Couldn't get server's public IP address", ex);
                    return IPAddress.Any;
                }
            }
        }

        /// <summary>
        /// Gets the local IP address of the server, if known
        /// </summary>
        public IPAddress LocalAddress
        {
            get
            {
                try
                {
                    return localAddress ?? (localAddress = Utility.GetLocalIP());
                }
                catch (Exception ex)
                {
                    RemoteLogger.Exception("Couldn't get server's local IP address", ex);
                    return IPAddress.Any;
                }
            }
        }

        /// <summary>
        /// Gets the public-facing network port of the server, if known
        /// </summary>
        public ushort Port => Provider.port;

        /// <summary>
        /// Gets the version or build number of the server
        /// </summary>
        public string Version => Provider.APP_VERSION;

        /// <summary>
        /// Gets the network protocol version of the server
        /// </summary>
        public string Protocol => Version;

        /// <summary>
        /// Gets the language set by the server
        /// </summary>
        public CultureInfo Language => CultureInfo.InstalledUICulture;

        /// <summary>
        /// Gets the total of players currently on the server
        /// </summary>
        public int Players => Provider.clients.Count;

        /// <summary>
        /// Gets/sets the maximum players allowed on the server
        /// </summary>
        public int MaxPlayers
        {
            get => Provider.maxPlayers;
            set => Provider.maxPlayers = (byte)value;
        }

        /// <summary>
        /// Gets/sets the current in-game time on the server
        /// </summary>
        public DateTime Time
        {
            get => DateTime.Today.AddSeconds(LightingManager.time * 120);
            set => LightingManager.time = (uint)(value.Second / 120);
        }

        /// <summary>
        /// Gets information on the currently loaded save file
        /// </summary>
        public SaveInfo SaveInfo => null;

        #endregion Information

        #region Administration

        /// <summary>
        /// Bans the player for the specified reason and duration
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        /// <param name="duration"></param>
        public void Ban(string id, string reason, TimeSpan duration = default(TimeSpan))
        {
            // Check if already banned
            if (!IsBanned(id))
            {
                // Ban and kick user
                Provider.ban(new CSteamID(ulong.Parse(id)), reason, (uint)duration.TotalSeconds);
            }
        }

        /// <summary>
        /// Gets the amount of time remaining on the player's ban
        /// </summary>
        /// <param name="id"></param>
        public TimeSpan BanTimeRemaining(string id)
        {
            SteamBlacklistID blacklistId = SteamBlacklist.list.First(e => e.playerID.ToString() == id);
            return TimeSpan.FromSeconds(blacklistId.duration);
        }

        /// <summary>
        /// Gets if the player is banned
        /// </summary>
        /// <param name="id"></param>
        public bool IsBanned(string id) => SteamBlacklist.checkBanned(new CSteamID(ulong.Parse(id)), 0, out SteamBlacklistID _);

        /// <summary>
        /// Saves the server and any related information
        /// </summary>
        public void Save() => SaveManager.save();

        /// <summary>
        /// Unbans the player
        /// </summary>
        /// <param name="id"></param>
        public void Unban(string id)
        {
            // Check if unbanned already
            if (IsBanned(id))
            {
                // Set to unbanned
                SteamBlacklist.unban(new CSteamID(ulong.Parse(id)));
            }
        }

        #endregion Administration

        #region Chat and Commands

        /// <summary>
        /// Broadcasts the specified chat message and prefix to all players
        /// </summary>
        /// <param name="message"></param>
        /// <param name="prefix"></param>
        /// <param name="args"></param>
        public void Broadcast(string message, string prefix, params object[] args)
        {
            message = args.Length > 0 ? string.Format(Formatter.ToUnity(message), args) : Formatter.ToUnity(message);
            string formatted = prefix != null ? $"{prefix} {message}" : message;
            ChatManager.sendChat(EChatMode.GLOBAL, formatted);
        }

        /// <summary>
        /// Broadcasts the specified chat message to all players
        /// </summary>
        /// <param name="message"></param>
        public void Broadcast(string message) => Broadcast(message, null);

        /// <summary>
        /// Runs the specified server command
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        public void Command(string command, params object[] args)
        {
            Commander.execute(CSteamID.Nil, $"{command} {string.Join(" ", Array.ConvertAll(args, x => x.ToString()))}");
        }

        #endregion Chat and Commands
    }
}
