using DGP.Genshin.EnkaAPI;
using DGP.Genshin.HutaoAPI.PostModel;
using DGP.Genshin.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace DGP.Genshin.DataModel.Showcase;

/// <summary>
/// 展柜
/// </summary>
public class Showcase
{
    /// <summary>
    /// 防止外部创建
    /// </summary>
    private Showcase()
    {
    }

    /// <summary>
    /// 更新时间
    /// </summary>
    public TimeSpan TimeToLive { get; private set; }

    /// <summary>
    /// 玩家信息
    /// </summary>
    public Player Player { get; private set; } = default!;

    /// <summary>
    /// 角色列表
    /// </summary>
    public IList<Avatar> Avatars { get; private set; } = default!;

    /// <summary>
    /// 构造一个新的角色橱窗
    /// </summary>
    /// <param name="response">Enka响应</param>
    /// <param name="avatarsMap">角色映射</param>
    /// <returns>新的角色橱窗</returns>
    internal static Showcase? Build(EnkaResponse? response,MetadataViewModel viewModel, IEnumerable<HutaoItem> avatarsMap)
    {
        if (response is not null && response.IsValid && response.HasDetail)
        {
            return new Showcase()
            {
                TimeToLive = TimeSpan.FromSeconds(response.Ttl!.Value),
                Player = new()
                {
                    NickName = response.PlayerInfo!.Nickname,
                    Level = response.PlayerInfo.Level,
                    Signature = response.PlayerInfo.Signature,
                    WorldLevel = response.PlayerInfo.WorldLevel,
                    AchievementNumber = response.PlayerInfo.FinishAchievementNum,
                    SpiralAbyss = $"{response.PlayerInfo.TowerFloorIndex}-{response.PlayerInfo.TowerLevelIndex}",
                    ProfilePicture = avatarsMap.First(a => a.Id == response.PlayerInfo.ProfilePicture.AvatarId).Url!,
                },
                Avatars = response.AvatarInfoList!.Select(info =>
                {
                    HutaoItem mapItem = avatarsMap.First(x => x.Id == info.AvatarId);
                    Character.Character baseCharacter = viewModel.Characters.First(x => x.Name == mapItem.Name);

                    return new Avatar()
                    {
                        BaseCharacter = baseCharacter,
                        FetterLevel = info.FetterInfo.ExpLevel,
                        Level = Convert.ToInt32(info.PropMap[Property.Level].Value),
                    };
                }).ToList(),
            };
        }

        return null;
    }
}

/// <summary>
/// 角色
/// </summary>
public class Avatar
{
    /// <summary>
    /// 基底角色信息
    /// </summary>
    public Character.Character BaseCharacter { get; set; } = default!;

    /// <summary>
    /// 等级
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    public int FetterLevel { get; set; } = default!;
}