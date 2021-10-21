using DGP.Genshin.Helpers;
using DGP.Snap.Framework.Data.Behavior;
using DGP.Snap.Framework.Data.Json;
using DGP.Snap.Framework.Extensions.System;
using Octokit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace DGP.Genshin.Services
{
    /// <summary>
    /// 开发者功能：Github储存库服务
    /// </summary>
    internal class GithubService : Observable
    {
        private const string repoFile = "repoinfos.json";
        private readonly GitHubClient client;
        private List<RepoInfo>? repoInfos;

        public GithubService()
        {
            this.client = new GitHubClient(new ProductHeaderValue("SnapGenshin"))
            {
                Credentials = new Credentials(TokenHelper.GetToken())
            };
            this.Log("initialized");
            SelectedRepositoryChanged += this.OnSelectedRepositoryChanged;
        }

        public async Task<IReadOnlyList<Release>?> GetRepositoryReleases(long? id)
        {
            return id is null ? null : await this.client.Repository.Release.GetAll(id.Value);

        }

        public async Task<Repository> GetRepository(string ownerAndName)
        {
            string[] ss = ownerAndName.Split('/');
            return await this.client.Repository.Get(ss[0], ss[1]);
        }

        public async Task AddRepository(string ownerAndName)
        {
            if (this.repoInfos is null)
            {
                return;
            }

            string[] ss = ownerAndName.Split('/');
            RepoInfo info = new RepoInfo(ss[0], ss[1]);
            
            if (!this.repoInfos.Contains(info))
            {
                this.repoInfos.Add(info);
                this.Repositories.Add(await this.GetRepository(ownerAndName));
            }
        }

        #region Observable
        private ObservableCollection<Repository> repositories = new ObservableCollection<Repository>();
        private Repository? selectedRepository;
        private List<Release>? releases;
        private Release? selectedRelease;
        public ObservableCollection<Repository> Repositories { get => this.repositories; set => this.Set(ref this.repositories, value); }
        public Repository? SelectedRepository
        {
            get => this.selectedRepository; set
            {
                this.Set(ref this.selectedRepository, value);
                SelectedRepositoryChanged?.Invoke(value?.Id);
            }
        }
        public List<Release>? Releases { get => this.releases; set => this.Set(ref this.releases, value); }
        public Release? SelectedRelease { get => this.selectedRelease; set => this.Set(ref this.selectedRelease, value); }
        #endregion

        #region LifeCycle
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
            this.UnInitialize();
        }
        #endregion

        private event Action<long?> SelectedRepositoryChanged;
        private async void OnSelectedRepositoryChanged(long? id)
        {
            IReadOnlyList<Release>? releases = await this.GetRepositoryReleases(id);
            if (releases is not null)
            {
                this.Releases = new List<Release>(releases);
            }
        }
    }
    /// <summary>
    /// 储存库信息
    /// </summary>
    public class RepoInfo : IEquatable<RepoInfo>
    {
        public RepoInfo(string owner, string name)
        {
            this.Owner = owner;
            this.Name = name;
        }

        public string Owner { get; set; }
        public string Name { get; set; }

        public bool Equals(RepoInfo? other) =>
            other is not null && other.Name == this.Name && other.Owner == this.Owner;

        public override bool Equals(object? obj) =>
            this.Equals(obj as RepoInfo);

        public override int GetHashCode() =>
            throw new NotImplementedException();
    }
}
