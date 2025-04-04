/*
 * PlayFab Unity SDK
 *
 * Copyright (c) Microsoft Corporation
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this
 * software and associated documentation files (the "Software"), to deal in the Software
 * without restriction, including without limitation the rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
 * to whom the Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
 * FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

namespace PlayFab.Multiplayer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Lobby
    {
        private static Dictionary<IntPtr, Lobby> lobbyCache = new Dictionary<IntPtr, Lobby>();

        internal Lobby(InteropWrapper.PFLobbyHandle lobbyHandle)
        {
            this.Handle = lobbyHandle;
        }

        /// <summary>
        /// Gets the ID of the Lobby.
        /// </summary>
        /// <remarks>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.CreateAndJoinLobby" /> or
        /// <see cref="PlayFabMultiplayer.JoinLobby" />, this method will return null until
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a successful
        /// <see cref="PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted" /> or <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />
        /// respectively.
        /// </remarks>
        /// <returns>
        /// Gets the ID of the Lobby.
        /// </returns>
        public string Id
        {
            get
            {
                string id;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetLobbyId(this.Handle, out id));
                return id;
            }
        }

        public object Context
        {
            /// <summary>
            /// Retrieves the app's private, custom pointer-sized context value previously associated with this lobby object.
            /// </summary>
            /// <remarks>
            /// If no custom context has been set yet, the value pointed to by <paramref name="customContext" /> is set to nullptr.
            /// </remarks>
            /// <returns>
            /// Custom context or null. />.
            /// </returns>
            get
            {
                object context;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetCustomContext(this.Handle, out context));
                if (context != null)
                {
                    return context;
                }
                else
                {
                    return null;
                }
            }

            /// <summary>
            /// Configures an optional, custom pointer-sized context value with this lobby object.
            /// </summary>
            /// <remarks>
            /// The custom context is typically used as a "shortcut" that simplifies accessing local, title-specific memory
            /// associated with the lobby without requiring a mapping lookup. The value is retrieved using the
            /// <see cref="Lobby.GetCustomContext()" /> method.
            /// <para>
            /// Any configured value is treated as opaque by the library, and is only valid on the local device; it is not
            /// transmitted over the network.
            /// </para>
            /// </remarks>
            set
            {
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbySetCustomContext(this.Handle, value));
            }
        }

        /// <summary>
        /// Gets the max member count of the lobby.
        /// </summary>
        /// <remarks>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.JoinLobby" />, this method will return
        /// 0 until <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> with the <c>OnLobbyUpdated maxMembersUpdated</c> set to
        /// true. If joining the lobby succeeds, this field is guaranteed to be populated by the time
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />.
        /// </remarks>
        /// <returns>
        /// The max member count of the lobby.
        /// </returns>
        public uint MaxMemberCount
        {
            get
            {
                uint maxMemberCount;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetMaxMemberCount(this.Handle, out maxMemberCount));
                return maxMemberCount;
            }
        }

        /// <summary>
        /// Gets the owner migration policy of the lobby.
        /// </summary>
        /// <remarks>
        /// The owner migration policy can't change for the lifetime of the lobby.
        /// <para>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.JoinLobby" />, this method returns
        /// <c>LobbyOwnerMigrationPolicy.None</c> until <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> with the <c>OnLobbyUpdated ownerUpdated</c> set to
        /// true. If joining the lobby succeeds, this field is guaranteed to be populated by the time
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The owner migration policy of the lobby.
        /// </returns>
        public LobbyOwnerMigrationPolicy OwnerMigrationPolicy
        {
            get
            {
                InteropWrapper.PFLobbyOwnerMigrationPolicy ownerMigrationPolicy;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetOwnerMigrationPolicy(this.Handle, out ownerMigrationPolicy));
                return (LobbyOwnerMigrationPolicy)ownerMigrationPolicy;
            }
        }

        /// <summary>
        /// Gets the access policy of the lobby.
        /// </summary>
        /// <remarks>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.JoinLobby" />, this method returns
        /// <c>LobbyAccessPolicy.Public</c> until <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> with the <c>OnLobbyUpdated accessPolicyUpdated</c> set to
        /// true. If joining the lobby succeeds, this field is guaranteed to be populated by the time
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />.
        /// </remarks>
        /// <returns>
        /// The access policy of the lobby.
        /// </returns>
        public LobbyAccessPolicy AccessPolicy
        {
            get
            {
                InteropWrapper.PFLobbyAccessPolicy accessPolicy;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetAccessPolicy(this.Handle, out accessPolicy));
                return (LobbyAccessPolicy)accessPolicy;
            }
        }

        /// <summary>
        /// Gets whether the lobby's membership is locked.
        /// </summary>
        /// <remarks>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.JoinLobby" />, this method returns
        /// false until <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> with the <c>OnLobbyUpdated membershipLockUpdated</c> set to
        /// true. If joining the lobby succeeds, this field is guaranteed to be populated by the time
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />.
        /// </remarks>
        /// <returns>
        /// Returns whether the membership of the lobby is locked.
        /// </returns>
        public LobbyMembershipLock MembershipLock
        {
            get
            {
                InteropWrapper.PFLobbyMembershipLock lockState;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetMembershipLock(this.Handle, out lockState));
                return (LobbyMembershipLock)lockState;
            }
        }

        /// <summary>
        /// Gets the default connection string associated with the lobby.
        /// </summary>
        /// <remarks>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.CreateAndJoinLobby" />, this method will
        /// return null until <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a successful
        /// <see cref="PlayFabMultiplayer.OnLobbyCreateAndJoinCompleted" />. If this lobby object was created by calling
        /// <see cref="PlayFabMultiplayer.JoinLobby" />, this method will return null until
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a successful
        /// <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />.
        /// </remarks>
        /// <returns>
        /// The default connection string associated with the lobby.
        /// </returns>
        public string ConnectionString
        {
            get
            {
                string connectionString;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetConnectionString(this.Handle, out connectionString));
                return connectionString;
            }
        }

        internal InteropWrapper.PFLobbyHandle Handle { get; set; }

        /// <summary>
        /// Tries to get the current owner of the lobby.  Returns false and sets owner to null if there is no owner on this Lobby
        /// </summary>
        /// <remarks>
        /// If this lobby object was created by calling <see cref="PlayFabMultiplayer.JoinLobby" />, this method returns false and 
        /// the owner is null until <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> with the <c>OnLobbyUpdated OwnerUpdated</c> field set to true.
        /// If joining the lobby succeeds, this field is guaranteed to be populated by the time
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" /> provides a <see cref="PlayFabMultiplayer.OnLobbyJoinCompleted" />.
        /// </remarks>
        /// <param name="owner">
        /// The output owner. This value may be null if the owner has left or disconnected from the lobby while the owner
        /// migration policy is <see cref="LobbyOwnerMigrationPolicy.Manual" /> or
        /// <see cref="LobbyOwnerMigrationPolicy.None" />.
        /// </param>
        /// <returns>
        /// true if an owner is found or false otherwise
        /// </returns>
        public bool TryGetOwner(out PFEntityKey owner)
        {
            InteropWrapper.PFEntityKey userHandle;
            if (PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetOwner(this.Handle, out userHandle)) && userHandle != null)
            {
                PFEntityKey lobbyUser = new PFEntityKey(userHandle);
                owner = lobbyUser;
                return true;
            }
            else
            {
                owner = null;
                return false;
            }
        }

        /// <summary>
        /// Gets the list of PlayFab entities currently joined to the lobby as members.
        /// </summary>
        /// <remarks>
        /// If this lobby object is still in the process of asynchronously being created or joined, via a call to either
        /// <see cref="PlayFabMultiplayer.CreateAndJoinLobby" /> or <see cref="PlayFabMultiplayer.JoinLobby" /> respectively, this
        /// method will return no members.
        /// </remarks>
        /// <returns>
        /// The list of PlayFab entities currently joined to the lobby as members.
        /// </returns>
        public IList<PFEntityKey> GetMembers()
        {
            List<PFEntityKey> userList = new List<PFEntityKey>();
            InteropWrapper.PFEntityKey[] userHandles;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetMembers(this.Handle, out userHandles));
            foreach (var userHandle in userHandles)
            {
                PFEntityKey user = new PFEntityKey(userHandle);
                userList.Add(user);
            }

            return userList;
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Request one local user to leave the lobby.
        /// </summary>
        /// <remarks>
        /// This operation should only fail if the client is experiencing persistent internet connectivity issues. Under these
        /// circumstances, the client loses their active connection to the lobby and remote lobby members see their
        /// <see cref="LobbyMemberConnectionStatus" /> as <c>LobbyMemberConnectionStatus.Disconnected</c>. The members
        /// experiencing connectivity issues remain as members of the lobby unless the lobby owner forcibly removes them.
        /// <para>
        /// This is an asynchronous operation. The local users removed from the lobby via this method won't be removed in the
        /// lists returned by <see cref="Lobby.GetMembers" /> until the asynchronous operation successfully completes and a
        /// <c>Multiplayer.OnLobbyMemberRemoved</c> is provided by
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" />.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// A value to indicate if a specific local user should leave the lobby. 
        /// </param>
        public void Leave(PlayFab.PlayFabAuthenticationContext localUser)
        {
            this.Leave(new PFEntityKey(localUser));
        }
#endif

        /// <summary>
        /// Request one local user to leave the lobby.
        /// </summary>
        /// <remarks>
        /// This operation should only fail if the client is experiencing persistent internet connectivity issues. Under these
        /// circumstances, the client loses their active connection to the lobby and remote lobby members see their
        /// <see cref="LobbyMemberConnectionStatus" /> as <c>LobbyMemberConnectionStatus.Disconnected</c>. The members
        /// experiencing connectivity issues remain as members of the lobby unless the lobby owner forcibly removes them.
        /// <para>
        /// This is an asynchronous operation. The local users removed from the lobby via this method won't be removed in the
        /// lists returned by <see cref="Lobby.GetMembers" /> until the asynchronous operation successfully completes and a
        /// <c>Multiplayer.OnLobbyMemberRemoved</c> is provided by
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" />.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// A value to indicate if a specific local user should leave the lobby. 
        /// </param>
        public void Leave(PFEntityKey localUser)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyLeave(this.Handle, localUser.EntityKey, null));
        }

        /// <summary>
        /// Request all local users to leave the lobby.
        /// </summary>
        /// <remarks>
        /// This operation should only fail if the client is experiencing persistent internet connectivity issues. Under these
        /// circumstances, the client loses their active connection to the lobby and remote lobby members see their
        /// <see cref="LobbyMemberConnectionStatus" /> as <c>LobbyMemberConnectionStatus.Disconnected</c>. The members
        /// experiencing connectivity issues remain as members of the lobby unless the lobby owner forcibly removes them.
        /// <para>
        /// This is an asynchronous operation. The local users removed from the lobby via this method won't be removed in the
        /// lists returned by <see cref="Lobby.GetMembers" /> until the asynchronous operation successfully completes and a
        /// <c>Multiplayer.OnLobbyMemberRemoved</c> is provided by
        /// <see cref="PlayFabMultiplayer.ProcessLobbyStateChanges" />.
        /// </para>
        /// </remarks>
        public void LeaveAllLocalUsers()
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyLeave(this.Handle, null, null));
        }

        /// <summary>
        /// Get the dictionary of search property keys and values
        /// </summary>
        /// <remarks>
        /// Search properties are visible to nonmembers of the lobby as metadata, which can be used to filter and sort lobby
        /// search results.
        /// <para>
        /// This constructs a new Dictionary upon each API call so it
        /// shouldn't be called with high frequency
        /// </para>
        /// <para>
        /// If this lobby object is still in the process of asynchronously being created or joined via a call to
        /// <see cref="PlayFabMultiplayer.CreateAndJoinLobby" /> or <see cref="PlayFabMultiplayer.JoinLobby" />, this method returns
        /// no keys.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The dictionary of search property keys and values
        /// </returns>
        public IDictionary<string, string> GetSearchProperties()
        {
            string[] keys;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetSearchPropertyKeys(this.Handle, out keys));

            string[] values = new string[keys.Length];
            int index = 0;
            foreach (var key in keys)
            {
                string value;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetSearchProperty(this.Handle, key, out value));
                values[index++] = value;
            }

            Dictionary<string, string> properties = Enumerable.Range(0, keys.Length).ToDictionary(
                i => keys[i],
                i => values[i]);

            return properties;
        }

        /// <summary>
        /// Get the dictionary of lobby property keys and values
        /// </summary>
        /// <remarks>
        /// Lobby properties are only visible to members of the lobby.
        /// <para>
        /// This constructs a new Dictionary upon each API call so it
        /// shouldn't be called with high frequency
        /// </para>
        /// <para>
        /// If this lobby object is still in the process of asynchronously being created or joined via a call to
        /// <see cref="PlayFabMultiplayer.CreateAndJoinLobby" /> or <see cref="PlayFabMultiplayer.JoinLobby" />, this method returns
        /// no keys.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The dictionary of lobby property keys and values
        /// </returns>
        public IDictionary<string, string> GetLobbyProperties()
        {
            string[] keys;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetLobbyPropertyKeys(this.Handle, out keys));

            string[] values = new string[keys.Length];
            int index = 0;
            foreach (var key in keys)
            {
                string value;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetLobbyProperty(this.Handle, key, out value));
                values[index++] = value;
            }

            Dictionary<string, string> properties = Enumerable.Range(0, keys.Length).ToDictionary(
                i => keys[i],
                i => values[i]);

            return properties;
        }

        /// <summary>
        /// Get the dictionary of member property keys and values for a specific member
        /// </summary>
        /// <remarks>
        /// Per-member properties are only visible to members of the lobby.
        /// <para>
        /// This constructs a new Dictionary upon each API call so it
        /// should not be called with high frequency
        /// </para>
        /// <para>
        /// If the member is still in the process of asynchronously joining this lobby either via
        /// <see cref="PlayFabMultiplayer.CreateAndJoinLobby" />, <see cref="PlayFabMultiplayer.JoinLobby" />, or
        /// <see cref="Lobby.AddMember" />, this method returns no keys.
        /// </para>
        /// </remarks>
        /// <param name="member">
        /// The member being queried.
        /// </param>
        /// <returns>
        /// The dictionary of member property keys and values for a specific member
        /// </returns>
        public IDictionary<string, string> GetMemberProperties(PFEntityKey member)
        {
            string[] keys;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetMemberPropertyKeys(this.Handle, member.EntityKey, out keys));

            string[] values = new string[keys.Length];
            int index = 0;
            foreach (var key in keys)
            {
                string value;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetMemberProperty(this.Handle, member.EntityKey, key, out value));
                values[index++] = value;
            }

            Dictionary<string, string> properties = Enumerable.Range(0, keys.Length).ToDictionary(
                i => keys[i],
                i => values[i]);

            return properties;
        }

        /// <summary>
        /// Determines a member's connection status to the notification service.
        /// </summary>
        /// <remarks>
        /// When joining a lobby, the library establishes a WebSocket connection to the PlayFab PubSub notification service.
        /// This connection is used to provide real-time updates to the library about the lobby. This method can be used to
        /// determine a member's connection status, which is useful for diagnosing a member's ability to receive updates
        /// about the lobby.
        /// <para>
        /// A local member which is still in the process of asychronously joining the lobby, via a call to any of
        /// <see cref="PlayFabMultiplayer.CreateAndJoinLobby" />, <see cref="PlayFabMultiplayer.JoinLobby" />,
        /// or <see cref="Lobby.AddMember" /> will see their connection
        /// status as <see cref="LobbyMemberConnectionStatus.NotConnected" /> until the connection is established.
        /// When a user's connection status changes from <see cref="LobbyMemberConnectionStatus.Connected" /> to
        /// <see cref="LobbyMemberConnectionStatus.NotConnected" />, they may be experiencing connectivity issues - or their game may
        /// have crashed. The lobby owner can remove such users from the lobby via <see cref="Lobby.ForceRemoveMember" />
        /// </para>
        /// </remarks>
        /// <param name="member">
        /// The member being queried.
        /// </param>
        /// <returns>
        /// The connection status of a member of the lobby.
        /// </returns>
        public LobbyMemberConnectionStatus GetMemberConnectionStatus(PFEntityKey member)
        {
            InteropWrapper.PFLobbyMemberConnectionStatus memberConnectionStatus;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetMemberConnectionStatus(this.Handle, member.EntityKey, out memberConnectionStatus));
            return (LobbyMemberConnectionStatus)memberConnectionStatus;
        }

        /// <summary>
        /// Tries to get the current server of the lobby.  Returns false and sets server to null if there is no server on this Lobby
        /// </summary>
        /// <remarks>
        /// If this lobby object was joined by calling <see cref="PlayFabMultiplayerServer.JoinLobbyAsServer" />, this method returns false and 
        /// the server is null until <see cref="PlayFabMultiplayerServer.ProcessServerLobbyStateChanges" /> provides a
        /// <see cref="PlayFabMultiplayerServer.OnJoinLobbyAsServerCompleted" /> with the <c>OnJoinLobbyAsServerCompleted result</c> field set to true.
        /// If joining the lobby as server succeeds, this field is guaranteed to be populated by the time
        /// <see cref="PlayFabMultiplayerServer.ProcessServerLobbyStateChanges" /> provides a <see cref="PlayFabMultiplayerServer.OnJoinLobbyAsServerCompleted" />.
        /// </remarks>
        /// <param name="server">
        /// The output lobby server entity.
        /// </param>
        /// <returns>
        /// true if an server is found or false otherwise
        /// </returns>
        public bool TryGetServer(out PFEntityKey server)
        {
            InteropWrapper.PFEntityKey serverHandle;
            if (PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetServer(this.Handle, out serverHandle)) && serverHandle != null)
            {
                server = new PFEntityKey(serverHandle);
                return true;
            }
            else
            {
                server = null;
                return false;
            }
        }

        /// <summary>
        /// Get the dictionary of server property keys and values
        /// </summary>
        /// <remarks>
        /// If this lobby isn't a client-owned lobby, no property value is returned.
        /// <para>
        /// This constructs a new Dictionary upon each API call so it
        /// shouldn't be called with high frequency
        /// </para>
        /// <para>
        /// If this lobby object is still in the process of asynchronously being joined via a call to
        /// <see cref="PlayFabMultiplayer.JoinLobbyAsServer" />, this method returns no keys.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The dictionary of lobby property keys and values
        /// </returns>
        public IDictionary<string, string> GetServerProperties()
        {
            string[] keys;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetServerPropertyKeys(this.Handle, out keys));

            string[] values = new string[keys.Length];
            int index = 0;
            foreach (var key in keys)
            {
                string value;
                PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetServerProperty(this.Handle, key, out value));
                values[index++] = value;
            }

            Dictionary<string, string> properties = Enumerable.Range(0, keys.Length).ToDictionary(
                i => keys[i],
                i => values[i]);

            return properties;
        }

        /// <summary>
        /// Retrieves the lobby server's connection status to the notification service.
        /// </summary>
        /// <remarks>
        /// When joining a lobby, the library establishes a WebSocket connection to the PlayFab PubSub notification service.
        /// This connection is used to provide real-time updates to the library about the lobby. This method can be used to
        /// determine the lobby server's connection status, which is useful for diagnosing the lobby server's ability to receive
        /// updates about the lobby.
        /// <para>
        /// Non-owning Lobby servers can only join client-owned lobbies. If no server is joined to the client-owned lobby, this
        /// method returns an appropriate error code.
        /// </para>
        /// <para>
        /// A lobby server, which is still in the process of asynchronously joining the lobby, via a call to
        /// <see cref="PlayFabMultiplayer.JoinLobbyAsServer()" /> sees its connection status as
        /// <see cref="LobbyServerConnectionStatus.NotConnected" /> until the connection is established.
        /// </para>
        /// <para>
        /// A change to a lobby server's connection status is indicated to the title via a
        /// <see cref="LobbyUpdatedStateChange" /> with the <c>LobbyUpdatedStateChange.serverConnectionStatusUpdated</c>
        /// field set to true.
        /// </para>
        /// </remarks>
        /// <returns>
        /// The connection status of the lobby.
        /// </returns>
        public LobbyServerConnectionStatus GetServerConnectionStatus()
        {
            InteropWrapper.PFLobbyServerConnectionStatus serverConnectionStatus;
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyGetServerConnectionStatus(this.Handle, out serverConnectionStatus));
            return (LobbyServerConnectionStatus)serverConnectionStatus;
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Post an update to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title
        /// is provided a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the Lobby state won't reflect this update until a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is
        /// provided to the title with the updated state.
        /// </para>
        /// <para>  
        /// The <paramref name="lobbyUpdate" /> contains fields that can only be modified by the owner of the lobby. This method
        /// fails and <see cref="PlayFabMultiplayer.OnError" /> is called if one of those fields is specified and <paramref name="localUser" /> 
        /// is not the owner of the lobby.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The local user posting the update.
        /// </param>
        /// <param name="lobbyUpdate">
        /// An update to apply to the shared portion of the lobby on behalf of <paramref name="localUser" />.
        /// </param>
        /// <param name="memberProperties">
        /// The member properties to update for the updating member.
        /// </param>        
        public void PostUpdate(
            PlayFab.PlayFabAuthenticationContext localUser,
            LobbyDataUpdate lobbyUpdate,
            IDictionary<string, string> memberProperties)
        {
            PlayFabMultiplayer.SetEntityToken(localUser);
            this.PostUpdate(new PFEntityKey(localUser), lobbyUpdate, memberProperties);
        }
#endif

        /// <summary>
        /// Post an update to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title
        /// is provided a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the Lobby state won't reflect this update until a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is
        /// provided to the title with the updated state.
        /// </para>
        /// <para>
        /// The <paramref name="lobbyUpdate" /> contains fields that can only be modified by the owner of the lobby. This method
        /// fails and <see cref="PlayFabMultiplayer.OnError" /> is called if one of those fields is specified and <paramref name="localUser" /> 
        /// is not the owner of the lobby.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The local user posting the update.
        /// </param>
        /// <param name="lobbyUpdate">
        /// An update to apply to the shared portion of the lobby on behalf of <paramref name="localUser" />.
        /// </param>
        /// <param name="memberProperties">
        /// The member properties to update for the updating member.
        /// </param>        
        public void PostUpdate(
            PFEntityKey localUser,
            LobbyDataUpdate lobbyUpdate,
            IDictionary<string, string> memberProperties)
        {
            InteropWrapper.PFLobbyMemberDataUpdate memberDataUpdate = new InteropWrapper.PFLobbyMemberDataUpdate(memberProperties);
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyPostUpdate(this.Handle, localUser.EntityKey, lobbyUpdate.Update, memberDataUpdate, null));
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Post an update to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title is
        /// provided a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the Lobby state won't reflect this update until a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is
        /// provided to the title with the updated state.
        /// </para>
        /// <para>
        /// The <paramref name="lobbyUpdate" /> contains fields that can only be modified by the owner of the lobby. This method
        /// fails and <see cref="PlayFabMultiplayer.OnError" /> is called if one of those fields is specified and <paramref name="localUser" /> is not the owner of the
        /// lobby.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The local user posting the update.
        /// </param>
        /// <param name="lobbyUpdate">
        /// An optional update to apply to the shared portion of the lobby on behalf of <paramref name="localUser" />.
        /// </param>
        public void PostUpdate(
            PlayFab.PlayFabAuthenticationContext localUser,
            LobbyDataUpdate lobbyUpdate)
        {
            PlayFabMultiplayer.SetEntityToken(localUser);
            this.PostUpdate(new PFEntityKey(localUser), lobbyUpdate);
        }
#endif

        /// <summary>
        /// Post an update to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title
        /// is provided a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the Lobby state won't reflect this update until a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is
        /// provided to the title with the updated state.
        /// </para>
        /// <para>
        /// The <paramref name="lobbyUpdate" /> contains fields that can only be modified by the owner of the lobby. This method
        /// fails and <see cref="PlayFabMultiplayer.OnError" /> is called if one of those fields is specified and <paramref name="localUser" /> is not the owner of the
        /// lobby.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The local user posting the update.
        /// </param>
        /// <param name="lobbyUpdate">
        /// An optional update to apply to the shared portion of the lobby on behalf of <paramref name="localUser" />.
        /// </param>
        public void PostUpdate(
            PFEntityKey localUser,
            LobbyDataUpdate lobbyUpdate)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyPostUpdate(this.Handle, localUser.EntityKey, lobbyUpdate.Update, null, null));
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Post an update to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title
        /// is provided a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the Lobby state won't reflect this update until a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is
        /// provided to the title with the updated state.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The local user posting the update.
        /// </param>
        /// <param name="memberProperties">
        /// The member properties to update for the updating member.
        /// </param>        
        public void PostUpdate(
            PlayFab.PlayFabAuthenticationContext localUser,
            IDictionary<string, string> memberProperties)
        {
            PlayFabMultiplayer.SetEntityToken(localUser);
            this.PostUpdate(new PFEntityKey(localUser), memberProperties);
        }
#endif

        /// <summary>
        /// Post an update to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> with the
        /// <c>OnLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title
        /// is provided a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the Lobby state won't reflect this update until a <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is
        /// provided to the title with the updated state.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The local user posting the update.
        /// </param>
        /// <param name="memberProperties">
        /// The member properties to update for the updating member.
        /// </param>        
        public void PostUpdate(
            PFEntityKey localUser,
            IDictionary<string, string> memberProperties)
        {
            InteropWrapper.PFLobbyMemberDataUpdate memberDataUpdate = new InteropWrapper.PFLobbyMemberDataUpdate(memberProperties);
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyPostUpdate(this.Handle, localUser.EntityKey, null, memberDataUpdate, null));
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Send an invite to this lobby from the local user to the invited entity.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title will be provided a
        /// <see cref="PlayFabMultiplayer.OnLobbySendInviteCompleted" /> with the
        /// <c>OnLobbySendInviteCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title will be provided a
        /// <see cref="PlayFabMultiplayer.OnLobbySendInviteCompleted" /> with the
        /// <c>OnLobbySendInviteCompleted result</c> field set to a failed error code.
        /// <para>
        /// The <paramref name="sender" /> must be a local user of this lobby which joined from this client.
        /// </para>
        /// </remarks>
        /// <param name="sender">
        /// The local user sending the invite.
        /// </param>
        /// <param name="invitee">
        /// The invited entity.
        /// </param>
        public void SendInvite(
            PlayFab.PlayFabAuthenticationContext sender,
            PFEntityKey invitee)
        {
            PlayFabMultiplayer.SetEntityToken(sender);
            this.SendInvite(new PFEntityKey(sender), invitee);
        }
#endif

        /// <summary>
        /// Send an invite to this lobby from the local user to the invited entity.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title will be provided a
        /// <see cref="PlayFabMultiplayer.OnLobbySendInviteCompleted" /> with the
        /// <c>OnLobbySendInviteCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title will be provided a
        /// <see cref="PlayFabMultiplayer.OnLobbySendInviteCompleted" /> with the
        /// <c>OnLobbySendInviteCompleted result</c> field set to a failed error code.
        /// <para>
        /// The <paramref name="sender" /> must be a local user of this lobby which joined from this client.
        /// </para>
        /// </remarks>
        /// <param name="sender">
        /// The local user sending the invite.
        /// </param>
        /// <param name="invitee">
        /// The invited entity.
        /// </param>
        public void SendInvite(
            PFEntityKey sender,
            PFEntityKey invitee)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbySendInvite(this.Handle, sender.EntityKey, invitee.EntityKey, null));
        }

#if UNITY_2017_1_OR_NEWER
        /// <summary>
        /// Add a local user as a member to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyMemberAdded" /> event followed by a <see cref="PlayFabMultiplayer.OnAddMemberCompleted" /> event with
        /// the <c>OnAddMemberCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnAddMemberCompleted" /> with the
        /// <c>OnAddMemberCompleted result</c> field set to a failed error code.
        /// <para>
        /// This method is used to add an extra local PlayFab entity to a pre-existing lobby object. Because the lobby
        /// object must have already been created either via a call to <see cref="PlayFabMultiplayer.CreateAndJoinLobby" /> or
        /// <see cref="PlayFabMultiplayer.JoinLobby" />, this method is primarily useful for multiple local user scenarios.
        /// </para>
        /// <para>
        /// This is an asynchronous operation. The member added via this method will not be reflected in the lists returned by
        /// <see cref="Lobby.GetMembers" /> until the asynchronous operation successfully completes.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The PlayFab Entity Key of the local user to add to the lobby as a member.
        /// </param>
        /// <param name="memberProperties">
        /// The initial member properties to set for this user when they join the lobby.
        /// </param>
        public void AddMember(
           PlayFab.PlayFabAuthenticationContext localUser,
           IDictionary<string, string> memberProperties)
        {
            PlayFabMultiplayer.SetEntityToken(localUser);
            this.AddMember(new PFEntityKey(localUser), memberProperties);
        }
#endif

        /// <summary>
        /// Add a local user as a member to the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyMemberAdded" /> event followed by a <see cref="PlayFabMultiplayer.OnAddMemberCompleted" /> event with
        /// the <c>OnAddMemberCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnAddMemberCompleted" /> with the
        /// <c>OnAddMemberCompleted result</c> field set to a failed error code.
        /// <para>
        /// This method is used to add an extra local PlayFab entity to a pre-existing lobby object. Because the lobby
        /// object must have already been created either via a call to <see cref="PlayFabMultiplayer.CreateAndJoinLobby" /> or
        /// <see cref="PlayFabMultiplayer.JoinLobby" />, this method is primarily useful for multiple local user scenarios.
        /// </para>
        /// <para>
        /// This is an asynchronous operation. The member added via this method will not be reflected in the lists returned by
        /// <see cref="Lobby.GetMembers" /> until the asynchronous operation successfully completes.
        /// </para>
        /// </remarks>
        /// <param name="localUser">
        /// The PlayFab Entity Key of the local user to add to the lobby as a member.
        /// </param>
        /// <param name="memberProperties">
        /// The initial member properties to set for this user when they join the lobby.
        /// </param>
        public void AddMember(
           PFEntityKey localUser,
           IDictionary<string, string> memberProperties)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyAddMember(this.Handle, localUser.EntityKey, memberProperties, null));
        }

        /// <summary>
        /// Forcibly remove an entity from the lobby.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyMemberRemoved" /> event followed by a
        /// <see cref="PlayFabMultiplayer.OnForceRemoveMemberCompleted" /> event with the
        /// <c>OnForceRemoveMemberCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnForceRemoveMemberCompleted" /> event with the
        /// <c>OnForceRemoveMemberCompleted result</c> field set to a failed error code.
        /// <para>
        /// One of the local PlayFab entities present in this lobby must be the owner for this operation to succeed. If the
        /// local owning entity who initiated this operation loses their ownership status while the operation is in progress,
        /// the operation fails asynchronously.
        /// </para>
        /// <para>
        /// This is an asynchronous operation. The member removed via this method will not be removed from the lists returned by
        /// <see cref="Lobby.GetMembers" /> until the asynchronous operation successfully completes and a
        /// <see cref="PlayFabMultiplayer.OnLobbyMemberRemoved" /> event is generated.
        /// </para>
        /// </remarks>
        /// <param name="targetMember">
        /// The member to forcibly remove.
        /// </param>
        /// <param name="preventRejoin">
        /// A flag indicating whether <paramref name="targetMember" /> will be prevented from rejoining the lobby after being
        /// removed.
        /// </param>
        public void ForceRemoveMember(
            PFEntityKey targetMember,
            bool preventRejoin)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayer.PFLobbyForceRemoveMember(this.Handle, targetMember.EntityKey, preventRejoin, null));
        }

        /// <summary>
        /// Post an update to the lobby as the server-owner.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerLobbyPostUpdateCompleted" /> with the
        /// <c>OnServerLobbyPostUpdateCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerLobbyPostUpdateCompleted" /> with the
        /// <c>OnServerLobbyPostUpdateCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the lobby state won't reflect this update until a
        /// <see cref="PlayFabMultiplayer.OnLobbyPostUpdateCompleted" /> is provided to the title with the updated state.
        /// </para>
        /// <para>
        /// While this method is present in the unified, cross-platform header, it is only implemented for Windows and, Xbox.
        /// The method returns errors on other platforms.
        /// </para>
        /// </remarks>
        /// <param name="lobbyUpdate">
        /// An update to apply to the shared portion of the lobby on behalf of the server owner.
        /// </param>
        public void ServerPostUpdate(
            LobbyDataUpdate lobbyUpdate)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayerServer.PFLobbyServerPostUpdate(this.Handle, lobbyUpdate.Update, null));
        }

        /// <summary>
        /// Delete a lobby on behalf of the game_server entity that owns the lobby.
        /// </summary>
        /// <remarks>
        /// This method queues an asynchronous operation to delete the lobby on behalf of the game_server entity. On completion,
        /// a <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerLobbyDeleteCompleted" />
        /// is provided indicating that the operation has completed.
        /// <para>
        /// This method does not guarantee the delete succeeds. The operation may fail due to networking or service errors.
        /// If the delete attempt fails but is retriable, the library continues to retry the delete operation. Once the
        /// operation can no longer be retried, the operation completes and a
        /// <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerLobbyDeleteCompleted" /> is provided.
        /// </para>
        /// <para>
        /// While this method is present in the unified, cross-platform header, it is only implemented for Windows and, Xbox.
        /// The method returns errors on other platforms.
        /// </para>
        /// </remarks>
        public void ServerDeleteLobby()
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayerServer.PFLobbyServerDeleteLobby(this.Handle, null));
        }

        /// <summary>
        /// Post an update to a client-owned lobby as a joined server.
        /// </summary>
        /// <remarks>
        /// This is an asynchronous operation. Upon successful completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerPostUpdateAsServerCompleted" /> with the
        /// <c>OnServerPostUpdateAsServerCompleted result</c> field set to <see cref="LobbyError.Success" />.
        /// Upon a failed completion, the title is provided a
        /// <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerPostUpdateAsServerCompleted" /> with the
        /// <c>OnServerPostUpdateAsServerCompleted result</c> field set to a failed error code.
        /// If applying the update would change the state of the lobby, the title is provided a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> sometime afterwards.
        /// <para>
        /// This operation completing successfully only indicates that the Lobby service has accepted the update. The title's
        /// local view of the lobby state won't reflect this update until a
        /// <see cref="PlayFabMultiplayer.OnLobbyUpdated" /> is provided to the title with the updated state.
        /// </para>
        /// <para>
        /// This operation is restricted to client-owned lobbies that are using connections.
        /// </para>
        /// <para>
        /// While this method is present in the unified, cross-platform header, it is only implemented for Windows and, Xbox.
        /// The method returns errors on other platforms.
        /// </para>
        /// </remarks>
        /// <param name="lobbyServerUpdate">
        /// An update to apply to the portion of the lobby data owned by the joined server.
        /// </param>
        public void PostUpdateAsServer(
            LobbyServerDataUpdate lobbyServerUpdate)
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayerServer.PFLobbyServerPostUpdateAsServer(this.Handle, lobbyServerUpdate.Update, null));
        }

        /// <summary>
        /// Requests that the server leave the client-owned lobby it's currently in.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method queues an asynchronous operation to exit the client-owned lobby. On completion, a
        /// <see cref="PlayFabMultiplayer.PlayFabMultiplayerServer.OnServerLeaveLobbyAsServerCompleted" /> is provided indicating that the operation
        /// completed.
        /// </para>
        /// <para>
        /// This operation is restricted to client-owned lobbies that are using connections.
        /// </para>
        /// <para>
        /// Any server-owned data previously added to the lobby is NOT automatically deleted when the server leaves the lobby.
        /// If there's a desire to also delete the serverData, it should be explicitly done by calling
        /// <see cref="PlayFabMultiplayer.PostUpdateAsServer" /> before leaving the lobby.
        /// </para>
        /// </remarks>
        public void LeaveAsServer()
        {
            PlayFabMultiplayer.Succeeded(InteropWrapper.PFMultiplayerServer.PFLobbyServerLeaveAsServer(this.Handle, null));
        }

        internal static Lobby GetLobbyUsingCache(InteropWrapper.PFLobbyHandle handle)
        {
            Lobby lobby;
            bool found = lobbyCache.TryGetValue(handle.InteropHandleIntPtr, out lobby);
            if (found)
            {
                return lobby;
            }
            else
            {
                lobby = new Lobby(handle);
                lobbyCache[handle.InteropHandleIntPtr] = lobby;
                return lobby;
            }
        }

        internal static void ClearLobbyFromCache(InteropWrapper.PFLobbyHandle handle)
        {
            if (lobbyCache.ContainsKey(handle.InteropHandleIntPtr))
            {
                lobbyCache.Remove(handle.InteropHandleIntPtr);
            }
        }
    }
}
