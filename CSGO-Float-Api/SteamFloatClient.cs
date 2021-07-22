using System;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.CSGO.Internal;
using SteamAuth;
using SteamKit2.Internal;
using System.Threading;
using System.Security.Cryptography;
using CSGO_Float_Api.Utils;
using CSGO_Float_Api.Models;
using CSGO_Float_Api.Database.Repositories;

namespace CSGO_Float_Api
{
    public class SteamFloatClient
    {
        public SteamAccount UserInfo;
        private readonly ISteamAccountRepository _steamAccountRepository;
        public SteamFloatClient(SteamAccount userInfo, ISteamAccountRepository steamAccountRepository)
        {
            UserInfo = userInfo;
            _steamAccountRepository = steamAccountRepository;
        }

        [Flags]
        public enum SteamClientState
        {
            TryingToConnect = 0,
            ErrorWhileConnecting = 1,
            ConnectedButNotPlaying = 2,
            ReadyToReceiveRequests = 3,
            WaitingForCallback = 4,
            WaitingForRequestCooldown = 5
        }

        public SteamClientState CurrentState;
        private Thread UsedThread;

        public string TwoFactorAuthCode;


        #region Variables 
        private CallbackManager callbackManager;
        private SteamGameCoordinator steamGameCoordinator;
        private SteamClient steamClient;
        private SteamUser steamUser;
        private SteamFriends steamFriends;
        private bool isRunning;
        private string twoFactorAuth = null;
        private byte[] sentryHash = null;
        private string WebAPIUserNonce = null;
        private string loginKey = null;
        private EOSType OSType = EOSType.Unknown;
        #endregion

        private const int TimeBetweenFloatRequests = 1400;
        private const int TimeoutSeconds = 1;
        private const uint APPID = 730;

        public bool GuardCodeInput { get; private set; }

        public void SteamConnect()
        {
            this.CurrentState = SteamClientState.TryingToConnect;
            // create our steamclient instance
            this.steamClient = new SteamClient();

            // create the callback manager which will route callbacks to function calls
            this.callbackManager = new CallbackManager(steamClient);

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            this.callbackManager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            this.callbackManager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            // get the steamuser handler, which is used for logging on after successfully connecting
            this.steamUser = steamClient.GetHandler<SteamUser>();
            this.callbackManager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            this.callbackManager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);
            this.callbackManager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);
            this.callbackManager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);

            this.steamFriends = steamClient.GetHandler<SteamFriends>();
            this.callbackManager.Subscribe<SteamUser.AccountInfoCallback>((cb) => this.steamFriends.SetPersonaState(EPersonaState.Online));

            this.steamGameCoordinator = steamClient.GetHandler<SteamGameCoordinator>();
            this.callbackManager.Subscribe<SteamGameCoordinator.MessageCallback>(OnMessage);

            Log.info($"<{UserInfo.Username}> Connecting to Steam...");

            isRunning = true;

            steamClient.Connect();

            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                this.callbackManager.RunWaitCallbacks(TimeSpan.FromSeconds(TimeoutSeconds));
            }

            return;
        }


        private void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (UserInfo.SentryFileBase64 != null)
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = Convert.FromBase64String(UserInfo.SentryFileBase64);
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            if (!string.IsNullOrWhiteSpace(UserInfo.LoginKey)) loginKey = UserInfo.LoginKey;

            if (sentryHash == null && loginKey == null && !string.IsNullOrWhiteSpace(UserInfo.Shared_secret))
            {
                var code = new SteamGuardAccount();
                code.SharedSecret = UserInfo.Shared_secret;
                twoFactorAuth = code.GenerateSteamGuardCode();
                Log.info($"<{UserInfo.Username}> Guard Code: {twoFactorAuth}", ConsoleColor.Magenta);
                GuardCodeInput = true;
            }

            SteamUser.LogOnDetails logOnDetails = new SteamUser.LogOnDetails()
            {
                //CellID = ASF.GlobalDatabase?.CellID,
                LoginID = 1998,
                LoginKey = loginKey,
                Password = UserInfo.Password,
                SentryFileHash = sentryHash,
                ShouldRememberPassword = true,
                TwoFactorCode = twoFactorAuth,
                Username = UserInfo.Username
            };

            if (OSType == EOSType.Unknown)
            {
                OSType = logOnDetails.ClientOSType;
            }

            steamUser.LogOn(logOnDetails);
        }

        private void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                if (is2FA)
                {
                    var code = new SteamGuardAccount();
                    code.SharedSecret = UserInfo.Shared_secret;
                    twoFactorAuth = code.GenerateSteamGuardCode();
                    Log.info($"<{UserInfo.Username}> Guard Code: {twoFactorAuth}", ConsoleColor.Magenta);
                    GuardCodeInput = true;
                }

                return;
            }

            if (callback.Result != EResult.OK)
            {
                Log.error($"<{UserInfo.Username}> Unable to logon to Steam: {callback.Result} / {callback.ExtendedResult}");

                if (callback.Result == EResult.RateLimitExceeded)
                {
                    Log.error($"{EResult.RateLimitExceeded} o Thread sera congelado por 30 minutos");
                    Thread.Sleep(TimeSpan.FromMinutes(60));
                }


                if (callback.Result == EResult.InvalidPassword)
                {
                    loginKey = null;
                    if (!string.IsNullOrWhiteSpace(UserInfo.LoginKey))
                    {
                        UserInfo.LoginKey = null;
                        UserInfo.SentryFileBase64 = null;
                        _steamAccountRepository.Update(UserInfo);
                        Log.info($"<{UserInfo.Username}> LoginKey Deleted.");
                    }
                    GuardCodeInput = true;
                    return;
                }

                if (callback.Result != EResult.NoConnection)
                {
                    isRunning = false;
                }

                return;
            }

            if (string.IsNullOrEmpty(callback.WebAPIUserNonce))
            {
                throw new ArgumentNullException(nameof(callback.WebAPIUserNonce));
            }

            WebAPIUserNonce = callback.WebAPIUserNonce;
            Log.info($"<{UserInfo.Username}> WebAPIUserNonce: {WebAPIUserNonce}", ConsoleColor.Cyan);

            this.SetReadyToReceiveRequests();
        }

        private void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Log.info($"<{UserInfo.Username}> MachineAuth - Updating sentryfile...");

            byte[] sentryHash;
            using (SHA1Managed sha = new SHA1Managed())
            {
                sentryHash = sha.ComputeHash(callback.Data);
            }

            UserInfo.SentryFileBase64 = Convert.ToBase64String(callback.Data);
            _steamAccountRepository.Update(UserInfo);

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = callback.BytesToWrite,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Log.info($"<{UserInfo.Username}> MachineAuth - Done!");
        }

        private void OnLoginKey(SteamUser.LoginKeyCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (string.IsNullOrEmpty(callback.LoginKey))
            {
                throw new ArgumentNullException(nameof(callback));
            }

            UserInfo.LoginKey = callback.LoginKey;
            _steamAccountRepository.Update(UserInfo);

            Log.info($"<{UserInfo.Username}> LoginKey: {callback.LoginKey}", ConsoleColor.Cyan);
            steamUser.AcceptNewLoginKey(callback);
        }

        private void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            isRunning = false;

            Log.info($"<{this.UserInfo.Username}> Logged off of Steam: " + callback.Result);

            this.CurrentState = SteamClientState.ErrorWhileConnecting;
        }

        private void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            isRunning = false;

            Log.error($"<{this.UserInfo.Username}> Disconnected from Steam");

            this.CurrentState = SteamClientState.ErrorWhileConnecting;

            Log.error($"<{this.UserInfo.Username}> {this.CurrentState.ToString()}");
        }

        public void SetReadyToReceiveRequests()
        {
            var StartPlayingGameMessage = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            StartPlayingGameMessage.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(APPID),
            });

            this.steamClient.Send(StartPlayingGameMessage);

            var ClientHello = new ClientGCMsgProtobuf<CMsgClientHello>((uint)EGCBaseClientMsg.k_EMsgGCClientHello);
            this.steamGameCoordinator.Send(ClientHello, APPID);

            this.CurrentState = SteamClientState.ReadyToReceiveRequests;

            UsedThread = new Thread(WaitForFloats);
            UsedThread.Start();

            Log.info($"<{this.UserInfo.Username}> Ready to receive requests");
        }

        public void SetNotReadyToReceiveRequests()
        {
            isRunning = false;

            this.CurrentState = SteamClientState.ConnectedButNotPlaying;

            var StopPlayingGameMessage = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            StopPlayingGameMessage.Body.games_played.Add(new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(0),
            });

            this.steamClient.Send(StopPlayingGameMessage);

            Log.error($"<{this.UserInfo.Username}> NOT ready to receive requests");
        }

        public void WaitForFloats()
        {
            while (true)
            {
                Server.QueueGetsAnElementEvent.WaitOne();
                while (this.TryRequestNextFloat())
                {

                }
            }
        }

        public bool TryRequestNextFloat()
        {
            ulong AssetIDOfSkinToRequest;
            Skin SkinToRequest;
            if (Server.SkinsQueue.TryDequeue(out AssetIDOfSkinToRequest) && Server.SkinsDatabase.TryGetValue(AssetIDOfSkinToRequest, out SkinToRequest))
            {
                this.CurrentState = SteamClientState.WaitingForCallback;

                var SkinDataRequest = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_Client2GCEconPreviewDataBlockRequest>((uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_Client2GCEconPreviewDataBlockRequest);

                SkinDataRequest.Body.param_s = SkinToRequest.param_s;
                SkinDataRequest.Body.param_a = SkinToRequest.param_a;
                SkinDataRequest.Body.param_d = SkinToRequest.param_d;
                SkinDataRequest.Body.param_m = SkinToRequest.param_m;

                int NumberOfAttemptsRemaining = 2;
                DateTime RequestDateTime = DateTime.UtcNow;

                while (this.CurrentState == SteamClientState.WaitingForCallback && NumberOfAttemptsRemaining != 0)
                {

                    if (NumberOfAttemptsRemaining == 0)
                    {
                        //Server.SendSkinErrorMessage(SkinToRequest);

                        this.CurrentState = SteamClientState.ReadyToReceiveRequests;
                    }
                    else
                    {
                        steamGameCoordinator.Send(SkinDataRequest, APPID);

                        while (this.CurrentState == SteamClientState.WaitingForCallback && DateTime.UtcNow - RequestDateTime < TimeSpan.FromSeconds(TimeoutSeconds))
                        {
                            this.callbackManager.RunWaitAllCallbacks(TimeSpan.FromSeconds(TimeoutSeconds) - (DateTime.UtcNow - RequestDateTime));
                        }

                        NumberOfAttemptsRemaining--;
                    }
                }

                return true;
            }
            else
            {
                this.CurrentState = SteamClientState.ReadyToReceiveRequests;

                return false;
            }
        }

        private void OnMessage(SteamGameCoordinator.MessageCallback callback)
        {
            if (callback.EMsg == (uint)ECsgoGCMsg.k_EMsgGCCStrike15_v2_Client2GCEconPreviewDataBlockResponse)
            {
                var ReceivedMessage = new ClientGCMsgProtobuf<CMsgGCCStrike15_v2_Client2GCEconPreviewDataBlockResponse>(callback.Message);

                ulong ItemID = Convert.ToUInt64(ReceivedMessage.Body.iteminfo.itemid);

                var NewSkin = Server.SkinsDatabase[ItemID];

                NewSkin.Float = BitConverter.ToSingle(BitConverter.GetBytes(ReceivedMessage.Body.iteminfo.paintwear), 0);

                var sucess = Server.SkinsDatabase.TryUpdate(NewSkin.param_a, NewSkin, NewSkin);

                if (!sucess)
                {
                    Log.error($"<{UserInfo.Username}> SkinsDatabase.TryUpdate Failed: AssetID:{NewSkin.param_a}");
                }

                this.CurrentState = SteamClientState.WaitingForRequestCooldown;
                Thread.Sleep(TimeBetweenFloatRequests);
            }
        }

    }
}
