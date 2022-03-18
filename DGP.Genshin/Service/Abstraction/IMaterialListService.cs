using DGP.Genshin.DataModel.Promotion;

namespace DGP.Genshin.Service.Abstraction
{
    public interface IMaterialListService
    {
        MaterialList Load();
        void Save(MaterialList? materialList);
    }
}
