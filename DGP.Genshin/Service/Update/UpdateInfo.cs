using DGP.Snap.Framework.Data.Behavior;

namespace DGP.Genshin.Service.Update
{
    public class UpdateInfo : Observable
    {
        private string title;
        private string detail;
        private string progressText;
        private double progress;

        public string Title { get => this.title; set => this.Set(ref this.title, value); }
        public string Detail { get => this.detail; set => this.Set(ref this.detail, value); }
        public string ProgressText { get => this.progressText; set => this.Set(ref this.progressText, value); }
        public double Progress { get => this.progress; set => this.Set(ref this.progress, value); }
    }
}
