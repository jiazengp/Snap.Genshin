using DGP.Genshin.DataModel.Characters;
using DGP.Genshin.DataModel.Helpers;
using DGP.Genshin.Services;
using DGP.Genshin.YoungMoeAPI.Collocation;
using System.Linq;

namespace DGP.Genshin.DataModel.YoungMoe2
{
    public class CollocationAvatar2 : CollocationAvatar
    {
        public string? Source => Character.Source;
        public string? StarUrl => Character.Star;
        public string? Element => Character.Element;

        private Character? character;
        private Character Character
        {
            get
            {
                if (character == null)
                {
                    character = Name == "旅行者"
                        ? new Character()
                        {
                            Star = StarHelper.FromRank(5),
                            Source = @"https://genshin.honeyhunterworld.com/img/char/traveler_boy_anemo_face_70.png"
                        }
                        : MetadataViewModel.Instance.Characters?.First(c => c.Name == Name) ?? new();
                }
                return character;
            }
        }
    }
}
