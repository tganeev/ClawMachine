using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    private class Mission
    {
        public string description;
        public bool isCompleted;
        public bool isFailed;
        public System.Action updateAction;
    }

    private List<Mission> activeMissions = new List<Mission>();
    private List<Text> missionTexts = new List<Text>();

    private float levelStartTime;
    private int toysCollectedThisLevel = 0;
    private int totalToysRequired = 0;
    private bool hasUsedChannel2 = false;
    private bool hasFailed = false;

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(Transform parentUI)
    {
        GameObject containerObj = new GameObject("MissionsContainer");
        containerObj.transform.SetParent(parentUI, false);
        RectTransform rt = containerObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -180);
        rt.sizeDelta = new Vector2(400, 300);

        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = false;

        for (int i = 0; i < 2; i++)
        {
            GameObject textObj = new GameObject("Mission_" + i);
            textObj.transform.SetParent(containerObj.transform, false);
            RectTransform trt = textObj.AddComponent<RectTransform>();
            trt.sizeDelta = new Vector2(400, 40);

            Text t = textObj.AddComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 24;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleLeft;
            missionTexts.Add(t);
        }
    }

    public void StartLevel(int level)
    {
        levelStartTime = Time.time;
        toysCollectedThisLevel = 0;
        hasUsedChannel2 = false;
        hasFailed = false;
        totalToysRequired = 1;

        activeMissions.Clear();

        List<Mission> pool = new List<Mission>();

        pool.Add(new Mission { description = "Собери все игрушки", updateAction = CheckCollectAll });
        pool.Add(new Mission { description = "Не отпускай захват в полете", updateAction = CheckHold });

        float timeLimit = 40f + (level * 10f);
        pool.Add(new Mission { description = $"Уложись в {timeLimit} сек", updateAction = () => CheckTime(timeLimit) });
        pool.Add(new Mission { description = "Не используй 2-й канал", updateAction = CheckIsolation });
        pool.Add(new Mission { description = "Не отпускай игрушку раньше отсека", updateAction = CheckAccuracy });

        var selected = pool.OrderBy(x => Random.value).Take(2).ToList();

        foreach (var m in selected)
        {
            activeMissions.Add(m);
        }

        UpdateMissionUI();
    }

    private void CheckCollectAll()
    {
        if (toysCollectedThisLevel >= totalToysRequired)
            activeMissions[0].isCompleted = true;
    }

    private void CheckHold()
    {
        if (hasFailed)
        {
            var m = activeMissions.Find(x => x.description.Contains("Удержи"));
            if (m != null) m.isFailed = true;
        }
    }

    private void CheckTime(float limit)
    {
        if (Time.time - levelStartTime > limit)
        {
            var m = activeMissions.Find(x => x.description.Contains("Уложись"));
            if (m != null) m.isFailed = true;
        }
    }

    private void CheckIsolation()
    {
        if (hasUsedChannel2)
        {
            var m = activeMissions.Find(x => x.description.Contains("канал"));
            if (m != null) m.isFailed = true;
        }
    }

    private void CheckAccuracy()
    {
        var m = activeMissions.Find(x => x.description.Contains("отсека"));
        if (m != null && hasFailed) m.isFailed = true;
    }

    public void OnToyCollected()
    {
        toysCollectedThisLevel++;
    }

    public void OnFailedAction(bool wasWrongChannel = false)
    {
        if (wasWrongChannel) hasUsedChannel2 = true;
        else hasFailed = true;
    }

    public void SetTotalToys(int count)
    {
        totalToysRequired = count;
    }

    void Update()
    {
        if (activeMissions.Count == 0) return;

        foreach (var m in activeMissions)
        {
            if (!m.isCompleted && !m.isFailed && m.updateAction != null)
            {
                m.updateAction.Invoke();
            }
        }

        UpdateMissionUI();
    }

    void UpdateMissionUI()
    {
        for (int i = 0; i < missionTexts.Count; i++)
        {
            if (i < activeMissions.Count)
            {
                var m = activeMissions[i];
                string statusIcon = m.isCompleted ? "✓ " : (m.isFailed ? "✗ " : "• ");
                string colorHex = m.isCompleted ? "00FF00" : (m.isFailed ? "FF0000" : "FFFFFF");

                missionTexts[i].text = $"<color=#{colorHex}>{statusIcon}{m.description}</color>";
                missionTexts[i].gameObject.SetActive(true);
            }
            else
            {
                missionTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
