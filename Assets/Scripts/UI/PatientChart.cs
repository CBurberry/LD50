using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using DG.Tweening;

public enum UIState 
{
    Ready,
    Busy
};

public class ChartData
{
    public List<Objective> Objectives;
}

public class PatientChart : MonoBehaviour
{
    public bool IsShowing => gameObject.activeSelf;

    [BoxGroup("Transitions")]
    [SerializeField] Vector3 offScreenPosition = new Vector3(0f, -800f);
    [BoxGroup("Transitions")]
    [SerializeField] Vector3 visiblePosition = new Vector3(0f, 20f);
    [BoxGroup("Transitions")]
    [SerializeField] float showDuration = 0.5f;
    [BoxGroup("Transitions")]
    [SerializeField] float hideDuration = 0.5f;

    [BoxGroup("User Interface")]
    [SerializeField] GridLayoutGroup objectivesParent;
    [BoxGroup("User Interface")]
    [SerializeField] MedicineCounterUI medCounterPrefab;

    private List<MedicineCounterUI> objectivesUi;
    private List<PillSO> pills;

    UIState state;

    private void Start()
    {
        state = UIState.Ready;
    }

    public void Toggle() 
    {
        if (IsShowing)
        {
            StartCoroutine(Hide());
        }
        else 
        {
            gameObject.SetActive(true);
            StartCoroutine(Show());
        }
    }

    public void SetData(ChartData data)
    {
        //Dumb approach of just reinitializing the whole list.
        //Definitely not efficient but gets the job done
        objectivesUi = new List<MedicineCounterUI>();
        pills = new List<PillSO>();

        Transform parentTransform = objectivesParent.gameObject.transform;
        int children = parentTransform.childCount;
        for (int i = children - 1; i >= 0; i--)
        {
            parentTransform.GetChild(i).gameObject.SetActive(false);
            Destroy(parentTransform.GetChild(i).gameObject);
        }

        foreach (var objective in data.Objectives) 
        {
            var member = Instantiate(medCounterPrefab, parentTransform);
            member.Initialize(objective.Count, objective.Pill.PreviewIcon);
            objectivesUi.Add(member);
            pills.Add(objective.Pill);
        }
    }

    public void IncrementPillCount(PillSO pill) 
    {
        if (!pills.Contains(pill)) 
        {
            return;
        }

        int index = pills.FindIndex(x => x.colr == pill.colr && x.shape == pill.shape);
        objectivesUi[index].IncrementCount();
    }

    public void DecrementPillCount(PillSO pill) 
    {
        if (!pills.Contains(pill))
        {
            return;
        }

        int index = pills.FindIndex(x => x.colr == pill.colr && x.shape == pill.shape);
        objectivesUi[index].DecrementCount();
    }

    private IEnumerator Show()
    {
        if (state == UIState.Busy) 
        {
            yield break;
        }

        state = UIState.Busy;
        //Entry transition tween
        yield return transform.DOMove(visiblePosition, showDuration).WaitForCompletion();
        state = UIState.Ready;
    }


    private IEnumerator Hide() 
    {
        if (state == UIState.Busy)
        {
            yield break;
        }

        state = UIState.Busy;

        //Exit transition tween
        yield return transform.DOMove(offScreenPosition, hideDuration).WaitForCompletion();
        gameObject.SetActive(false);
        state = UIState.Ready;
    }
}
