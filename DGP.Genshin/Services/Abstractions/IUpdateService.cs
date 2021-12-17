using DGP.Genshin.Services.Updating;
using Octokit;
using System;
using System.Threading.Tasks;

namespace DGP.Genshin.Services.Abstratcions
{
    public interface IUpdateService
    {
        Version? CurrentVersion { get; }
        Version? NewVersion { get; set; }
        Uri? PackageUri { get; set; }
        Release? Release { get; set; }
        Task<UpdateState> CheckUpdateStateAsync();
        Task DownloadAndInstallPackageAsync();
    }
}
