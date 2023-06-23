using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementsPanel : MonoBehaviour
{
    public GameObject grid;
    public GameObject achievementPrefab;

    private Dictionary<string, AchievementDetails> achievementDetails;

    public AchievementDetails win3GamesAchievement;

    [System.Serializable]
    public class AchievementDetails
    {
        public string title;
        public string description;
        public Sprite image;
    }

    // Start is called before the first frame update
    void Start()
    {
        achievementDetails = new Dictionary<string, AchievementDetails>
        {
            ["win3games"] = win3GamesAchievement
        };

        foreach (var achievement in ServerSession.Achievements)
        {
            DisplayAchievement(achievement);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DisplayAchievement(ServerSession.Achievement achievement)
    {
        var newObject = Instantiate(achievementPrefab, grid.transform);
        newObject.GetComponent<AchievementView>().title.text = achievementDetails[achievement.id].title;
        newObject.GetComponent<AchievementView>().description.text = achievementDetails[achievement.id].description;
        newObject.GetComponent<AchievementView>().image.sprite = achievementDetails[achievement.id].image;
        newObject.GetComponent<AchievementView>().date.text = achievement.time;
    }
}
