using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    internal class GithubService : Observable
    {
        private const string repoFile = "repoinfos.json";
        private readonly GitHubClient client;
        private List<RepoInfo> repoInfos;

        private ObservableCollection<Repository> repositories = new ObservableCollection<Repository>();
        private Repository selectedRepository;
        private List<Release> releases;
        private Release selectedRelease;

        public GithubService()
        {
            this.client = new GitHubClient(new ProductHeaderValue("SnapGenshin"));
            SelectedRepositoryChanged += OnSelectedRepositoryChanged;
        }

        private async void OnSelectedRepositoryChanged(long id) => this.Releases = new List<Release>(await GetRepositoryReleases(id));

        public async Task<IReadOnlyList<Release>> GetRepositoryReleases(long id) => await this.client.Repository.Release.GetAll(id);

        public async Task<Repository> GetRepository(string ownerAndName)
        {
            string[] ss = ownerAndName.Split('/');
            return await this.client.Repository.Get(ss[0], ss[1]);
        }

        public async Task AddRepository(string ownerAndName)
        {
            string[] ss = ownerAndName.Split('/');
            RepoInfo info = new RepoInfo(ss[0], ss[1]);
            if (!this.repoInfos.Contains(info))
            {
                this.repoInfos.Add(info);
                this.Repositories.Add(await GetRepository(ownerAndName));
            }
        }

        public ObservableCollection<Repository> Repositories { get => this.repositories; set => Set(ref this.repositories, value); }
        public Repository SelectedRepository
        {
            get => this.selectedRepository; set
            {
                Set(ref this.selectedRepository, value);
                SelectedRepositoryChanged?.Invoke(value.Id);
            }
        }
        public List<Release> Releases { get => this.releases; set => Set(ref this.releases, value); }
        public Release SelectedRelease { get => this.selectedRelease; set => Set(ref this.selectedRelease, value); }

        private bool isInitialized = false;

        public async Task InitializeAsync()
        {
            if (!this.isInitialized)
            {
                using (StreamReader reader = new StreamReader(File.Exists(repoFile) ? File.OpenRead(repoFile) : File.Create(repoFile)))
                {
                    this.repoInfos = Json.ToObject<List<RepoInfo>>(await reader.ReadToEndAsync());
                }
                if (this.repoInfos != null)
                {
                    foreach (RepoInfo repoInfo in this.repoInfos)
                    {
                        this.Repositories.Add(await this.client.Repository.Get(repoInfo.Owner, repoInfo.Name));
                    }
                }
                else
                {
                    this.repoInfos = new List<RepoInfo>();
                }
                this.isInitialized = true;
            }
        }
        public void UnInitialize()
        {
            using StreamWriter writer = new StreamWriter(File.Create(repoFile));
            writer.Write(Json.Stringify(this.repoInfos));
        }

        ~GithubService()
        {
            UnInitialize();
        }

        private event Action<long> SelectedRepositoryChanged;
    }

    public class RepoInfo : IEquatable<RepoInfo>
    {
        public RepoInfo(string owner, string name)
        {
            this.Owner = owner;
            this.Name = name;
        }

        public string Owner { get; set; }
        public string Name { get; set; }

        public bool Equals(RepoInfo other) =>
            other != null && other.Name == this.Name && other.Owner == this.Owner;
    }
}
