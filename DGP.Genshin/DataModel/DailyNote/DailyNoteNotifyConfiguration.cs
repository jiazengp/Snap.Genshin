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
            get => keepNotificationFront;

            set => Set(ref keepNotificationFront, value);
        }
        public bool NotifyOnResinReach20
        {
            get => notifyOnResinReach20;

            set => Set(ref notifyOnResinReach20, value);
        }
        public bool NotifyOnResinReach40
        {
            get => notifyOnResinReach40;

            set => Set(ref notifyOnResinReach40, value);
        }
        public bool NotifyOnResinReach80
        {
            get => notifyOnResinReach80;

            set => Set(ref notifyOnResinReach80, value);
        }
        public bool NotifyOnResinReach120
        {
            get => notifyOnResinReach120;

            set => Set(ref notifyOnResinReach120, value);
        }
        public bool NotifyOnResinReach155
        {
            get => notifyOnResinReach155;

            set => Set(ref notifyOnResinReach155, value);
        }
        public bool NotifyOnHomeCoinReach80Percent
        {
            get => notifyOnHomeCoinReach80Percent;

            set => Set(ref notifyOnHomeCoinReach80Percent, value);
        }
        public bool NotifyOnDailyTasksIncomplete
        {
            get => notifyOnDailyTasksIncomplete;

            set => Set(ref notifyOnDailyTasksIncomplete, value);
        }
        public bool NotifyOnExpeditionsComplete
        {
            get => notifyOnExpeditionsComplete;

            set => Set(ref notifyOnExpeditionsComplete, value);
        }
    }
}
