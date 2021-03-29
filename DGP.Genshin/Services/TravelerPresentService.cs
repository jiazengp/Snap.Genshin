using DGP.Genshin.Data;
using DGP.Genshin.Data.Character;
using DGP.Snap.Framework.Core.LifeCycling;
using System;

namespace DGP.Genshin.Services
{
    public class TravelerPresentService : ILifeCycleManaged
    {
        public void SetPresentTraveler()
        {
            Element element =
                LifeCycle.InstanceOf<SettingService>().GetOrDefault(Setting.PresentTravelerElementType,
                                                     Element.Anemo,
                                                     n => (Element)Enum.Parse(typeof(Element), n.ToString()));
            Character TravelerSource = CharacterManager.Instance["Traveler" + element];

            CharacterManager.Instance["TravelerPresent"].ImageUri = TravelerSource.ImageUri;
            CharacterManager.Instance["TravelerPresent"].Element = TravelerSource.Element;
            CharacterManager.Instance["TravelerPresent"].AscensionBoss = TravelerSource.AscensionBoss;
            CharacterManager.Instance["TravelerPresent"].AscensionGemstone = TravelerSource.AscensionGemstone;
            CharacterManager.Instance["TravelerPresent"].AscensionLocal = TravelerSource.AscensionLocal;
            CharacterManager.Instance["TravelerPresent"].AscensionMonster = TravelerSource.AscensionMonster;
            CharacterManager.Instance["TravelerPresent"].TalentDaily = TravelerSource.TalentDaily;
            CharacterManager.Instance["TravelerPresent"].TalentWeekly = TravelerSource.TalentWeekly;
        }

        public void Initialize()
        {
            SetPresentTraveler();
        }

        public void UnInitialize()
        {

        }
    }
}
