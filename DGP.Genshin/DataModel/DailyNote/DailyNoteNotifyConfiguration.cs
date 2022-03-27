using Snap.Data.Primitive;

namespace DGP.Genshin.DataModel.DailyNote
{
    public class DailyNoteNotifyConfiguration : Observable
    {
        private bool notifyOnResinReach20;
        private bool notifyOnResinReach40;
        private bool notifyOnResinReach80;
        private bool notifyOnResinReach120;
        private bool notifyOnResinReach155;
        private bool notifyOnHomeCoinReach80Percent;
        private bool notifyOnDailyTasksIncomplete;
        private bool notifyOnExpeditionsComplete;
        private bool keepNotificationFront;

        public bool KeepNotificationFront
        {
            get => this.keepNotificationFront;

            set => this.Set(ref this.keepNotificationFront, value);
        }
        public bool NotifyOnResinReach20
        {
            get => this.notifyOnResinReach20;

            set => this.Set(ref this.notifyOnResinReach20, value);
        }
        public bool NotifyOnResinReach40
        {
            get => this.notifyOnResinReach40;

            set => this.Set(ref this.notifyOnResinReach40, value);
        }
        public bool NotifyOnResinReach80
        {
            get => this.notifyOnResinReach80;

            set => this.Set(ref this.notifyOnResinReach80, value);
        }
        public bool NotifyOnResinReach120
        {
            get => this.notifyOnResinReach120;

            set => this.Set(ref this.notifyOnResinReach120, value);
        }
        public bool NotifyOnResinReach155
        {
            get => this.notifyOnResinReach155;

            set => this.Set(ref this.notifyOnResinReach155, value);
        }
        public bool NotifyOnHomeCoinReach80Percent
        {
            get => this.notifyOnHomeCoinReach80Percent;

            set => this.Set(ref this.notifyOnHomeCoinReach80Percent, value);
        }
        public bool NotifyOnDailyTasksIncomplete
        {
            get => this.notifyOnDailyTasksIncomplete;

            set => this.Set(ref this.notifyOnDailyTasksIncomplete, value);
        }
        public bool NotifyOnExpeditionsComplete
        {
            get => this.notifyOnExpeditionsComplete;

            set => this.Set(ref this.notifyOnExpeditionsComplete, value);
        }
    }
}
