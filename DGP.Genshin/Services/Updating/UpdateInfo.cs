using DGP.Snap.Framework.Data.Behavior;

namespace DGP.Genshin.Services.Updating
{
    public class UpdateInfo : Observable
    {
        private string title;
        private string detail;
        private string progressText;
        private double progress;

        public string Title { get => this.title; set => Set(ref this.title, value); }
        public string Detail { get => this.detail; set => Set(ref this.detail, value); }
        public string ProgressText { get => this.progressText; set => Set(ref this.progressText, value); }
        public double Progress { get => this.progress; set => Set(ref this.progress, value); }
    }
}
