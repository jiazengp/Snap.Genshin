using DGP.Genshin.DataModel.Achievement;
using DGP.Genshin.DataModel.Achievement.CocoGoat;
using DGP.Genshin.Service.Abstraction.Achievement;
using Snap.Core.DependencyInjection;
using Snap.Data.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DGP.Genshin.Service
{
    /// <inheritdoc cref="IAchievementService"/>
    [Service(typeof(IAchievementService), InjectAs.Transient)]
    internal class AchievementService : IAchievementService
    {
        private const string AchievementsFileName = "achievements.json";

        /// <inheritdoc/>
        public List<IdTime> GetCompletedItems()
        {
            return Json.FromFileOrNew<List<IdTime>>(PathContext.Locate(AchievementsFileName));
        }

        /// <inheritdoc/>
        public void SaveCompletedItems(ObservableCollection<Achievement> achievements)
        {
            IEnumerable<IdTime> idTimes = achievements
                .Where(a => a.IsCompleted)
                .Select(a => new IdTime(a.Id, a.CompleteDateTime));
            Json.ToFile(PathContext.Locate(AchievementsFileName), idTimes);
        }

        /// <inheritdoc/>
        public IEnumerable<IdTime>? TryGetImportData(ImportAchievementSource source, string fileName)
        {
            try
            {
                switch (source)
                {
                    case ImportAchievementSource.Cocogoat:
                        {
                            CocoGoatUserData? userData = Json.FromFile<CocoGoatUserData>(fileName);
                            if (userData?.Value?.Achievements is List<CocoGoatAchievement> achievements)
                            {
                                return achievements
                                    .Select(a => new IdTime(a.Id, a.Date));
                            }

                            break;
                        }

                    default:
                        break;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IdTime>? TryGetImportData(string dataString)
        {
            IEnumerable<IdTimeStamp>? idTimeStamps = Json.ToObject<IEnumerable<IdTimeStamp>>(dataString);
            return idTimeStamps?
                .Select(ts => new IdTime(ts.Id, DateTime.UnixEpoch + TimeSpan.FromSeconds(ts.TimeStamp)));
        }
    }
}