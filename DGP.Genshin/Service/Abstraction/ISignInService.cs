using System.Threading.Tasks;

namespace DGP.Genshin.Service.Abstraction
{
    public interface ISignInService
    {
        Task TrySignAllAccountsRolesInAsync();
    }
}
