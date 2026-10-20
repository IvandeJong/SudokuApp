using UnityEngine;
using System;
using System.Collections.Generic;

public class CampaignManager : MonoBehaviour
{
    public static CampaignManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindAnyObjectByType<CampaignManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CampaignManager");
                    _instance = go.AddComponent<CampaignManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    private static CampaignManager _instance;

    [Serializable]
    public class LevelProgress
    {
        public int levelIndex;
        public float bestTime;
        public int stars;
        public int bestMistakes;
    }

    [Serializable]
    public class CampaignLevel
    {
        public int index;
        public string gridSize;
        public string difficulty;
    }

    private List<CampaignLevel> levels = new List<CampaignLevel>();
    private Dictionary<int, LevelProgress> progress = new Dictionary<int, LevelProgress>();

    private const string ProgressKeyPrefix = "CampaignProgress_";

    [System.NonSerialized] private bool isInitialized = false;
    private void EnsureInitialized()
    {
        if (isInitialized && progress != null && progress.Count > 0) return;
        InitializeLevels();
        LoadAllProgress();
        isInitialized = true;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureInitialized();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public List<CampaignLevel> GetAllLevels()
    {
        EnsureInitialized();
        return levels;
    }

    public CampaignLevel GetLevel(int index)
    {
        EnsureInitialized();
        if (index >= 0 && index < levels.Count) return levels[index];
        return null;
    }

    [SerializeField] private int maxAvailableLevels = 2;
    public int MaxAvailableLevels
    {
        get => maxAvailableLevels;
        set => maxAvailableLevels = value;
    }

    public bool IsLevelUnlocked(int index)
    {
        EnsureInitialized();
        if (index < 0 || index >= levels.Count) return false;
        if (progress.ContainsKey(index)) return true;

        int incompleteLevelsBefore = 0;
        for (int i = 0; i < index; i++)
        {
            if (!progress.ContainsKey(i)) incompleteLevelsBefore++;
        }

        return incompleteLevelsBefore < maxAvailableLevels;
    }

    public CampaignLevel GetNextLevel(int index)
    {
        return GetLevel(index + 1);
    }

    public void SaveLevelResult(int index, float time, int mistakes)
    {
        EnsureInitialized();
        int stars = CalculateStars(index, time, mistakes);
        
        if (!progress.ContainsKey(index) || stars > progress[index].stars || (stars == progress[index].stars && time < progress[index].bestTime))
        {
            var lp = new LevelProgress
            {
                levelIndex = index,
                bestTime = time,
                stars = stars,
                bestMistakes = mistakes
            };
            progress[index] = lp;
            PlayerPrefs.SetString(ProgressKeyPrefix + index, JsonUtility.ToJson(lp));
            PlayerPrefs.Save();
        }
    }

    public LevelProgress GetProgress(int index)
    {
        EnsureInitialized();
        if (progress.TryGetValue(index, out var lp)) return lp;
        return null;
    }

    private void InitializeLevels()
    {
        levels.Clear();
        int index = 0;

        // 5 Nano (E, E, M, M, H)
        AddLevels("Nano", new[] { "Easy", "Easy", "Medium", "Medium", "Hard" }, ref index);

        // 5 Mini (E, E, M, M, H)
        AddLevels("Mini", new[] { "Easy", "Easy", "Medium", "Medium", "Hard" }, ref index);

        // 100 Classic (10E, 30M, 60H)
        string[] classicDiffs = new string[100];
        for (int i = 0; i < 10; i++) classicDiffs[i] = "Easy";
        for (int i = 10; i < 40; i++) classicDiffs[i] = "Medium";
        for (int i = 40; i < 100; i++) classicDiffs[i] = "Hard";
        AddLevels("Classic", classicDiffs, ref index);
    }

    private void AddLevels(string size, string[] difficulties, ref int index)
    {
        foreach (var diff in difficulties)
        {
            levels.Add(new CampaignLevel { index = index++, gridSize = size, difficulty = diff });
        }
    }

    private void LoadAllProgress()
    {
        for (int i = 0; i < levels.Count; i++)
        {
            string json = PlayerPrefs.GetString(ProgressKeyPrefix + i, "");
            if (!string.IsNullOrEmpty(json))
            {
                progress[i] = JsonUtility.FromJson<LevelProgress>(json);
            }
        }
    }

    public int CalculateStars(int levelIndex, float time, int mistakes)
    {
        var level = GetLevel(levelIndex);
        if (level == null) return 0;

        if (mistakes < 5) return 3;
        if (mistakes == 5) return 2;
        return 1;
    }
}
