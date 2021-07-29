using DGP.Genshin.Models.MiHoYo.Record;
using DGP.Genshin.Models.MiHoYo.Record.Avatar;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using DGP.Snap.Framework.NativeMethods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DGP.Genshin.Services
{
    internal class RecordService : Observable
    {
        private static readonly string CookieUrl = "https://user.mihoyo.com/";
        private static readonly string LoginTicketFile = "ticket.dat";
        private static readonly string QueryHistoryFile = "history.dat";

        private Record currentRecord;
        public Record CurrentRecord { get => this.currentRecord; set => this.Set(ref this.currentRecord, value); }

        private DetailedAvatar selectedAvatar;
        public DetailedAvatar SelectedAvatar
        {
            get => this.selectedAvatar; set
            {
                this.Set(ref this.selectedAvatar, value);
                this.SelectedReliquary = this.SelectedAvatar?.Reliquaries.Count() > 0 ? this.SelectedAvatar?.Reliquaries.First() : null;
            }
        }

        private Reliquary selectedReliquary;
        public Reliquary SelectedReliquary { get => this.selectedReliquary; set => this.Set(ref this.selectedReliquary, value); }

        public string LoginTicket { get; set; }
        public List<string> QueryHistory { get; set; } = new List<string>();
        public LoginWindow LoginWindow { get; set; }
        public void Login() => new LoginWindow().Show();
        public void OnLogin(bool isLoggedIn)
        {
            if (isLoggedIn)
            {
                this.LoginTicket = this.GetLoginTicket();
                File.WriteAllText(LoginTicketFile, this.LoginTicket);
                this.LoginWindow.Close();
            }
            else
            {
                if (NavigationService.Current.CanGoBack)
                    NavigationService.Current.GoBack();
                this.UnInitialize();
            }
        }
        public string GetLoginTicket()
        {
            StringBuilder loginTicket = new StringBuilder(1024);
            uint size = Convert.ToUInt32(loginTicket.Capacity + 1);
            WinInet.InternetGetCookieEx(CookieUrl, null, loginTicket, ref size, WinInet.COOKIE_HTTP_ONLY, IntPtr.Zero);
            return loginTicket.ToString();
        }
        internal void AddQueryHistory(string uid)
        {
            if (!this.QueryHistory.Contains(uid))
                this.QueryHistory.Add(uid);
        }

        #region 单例
        private static RecordService instance;
        private static readonly object _lock = new();
        private RecordService()
        {
            if (File.Exists(LoginTicketFile))
            {
                this.LoginTicket = File.ReadAllText(LoginTicketFile);
            }
            if (File.Exists(QueryHistoryFile))
            {
                try
                {
                    this.QueryHistory = Json.ToObject<List<string>>(File.ReadAllText(QueryHistoryFile));
                }
                catch
                {
                    this.Log("Failed to retrive query history.");
                }
            }
        }
        public void UnInitialize()
        {
            File.WriteAllText(QueryHistoryFile, Json.Stringify(this.QueryHistory));
            Environment.Exit(0);
        }
        public static RecordService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = new RecordService();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
    }
}
