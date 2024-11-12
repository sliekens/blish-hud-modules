using GuildWars2.Hero.Achievements;
using System.Collections.Generic;
using System.Linq;

namespace MyBlishHUDModule;

public class ChatLinkCalculator(IEnumerable<Achievement> achievements)
{
	private readonly IReadOnlyDictionary<int, Achievement> _achievements = achievements.ToDictionary(achievement => achievement.Id);

	public IEnumerable<Achievement> Achievements => _achievements.Values;

	public string GetAchievementChatLink(int achievementId)
	{
		if (_achievements.TryGetValue(achievementId, out var achievement))
		{
			return $"[&todo] ({achievement.Name})";
		}

		return string.Empty;
	}
}