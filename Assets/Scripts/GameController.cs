using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using Utility.DeveloperConsole;

public class GameController : MonoBehaviour
{
    [BoxGroup("User Interface")]
    [SerializeField] Canvas patientChartCanvas;
    [BoxGroup("User Interface")]
    [SerializeField] GridLayoutGroup medicinesList;
    [BoxGroup("User Interface")]
    [SerializeField] Text countdownText;

    [BoxGroup("Gameplay")]
    [SerializeField] List<RoundData> Rounds;
    [BoxGroup("Gameplay")]
    [SerializeField] float roundDuration = 60f;             //N.B. We may want to consider this as a difficulty parameter
    [BoxGroup("Gameplay")]
    [SerializeField] ConsumeZone scoringZone;

    private int currentRound;
    private RoundData activeRound;
    private float roundElapsedTime;

    //Debug
    [Header("Debug")]
    [SerializeField] DeveloperConsoleBehaviour devConsole;

    //User Collected Items
    private Dictionary<PillSO, int> collectedMedication;

    public static GameController Instance { get; private set; }

    private void Awake()
    {
        currentRound = 0;

        if (Instance != null)
        {
            Destroy(this);
        }
        else 
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        StartNewRound();
    }

    private void Update()
    {
        if (activeRound != null) 
        {
            roundElapsedTime += Time.deltaTime;
            float timeRemaining = Mathf.Clamp(roundDuration - roundElapsedTime, 0, roundDuration);
            countdownText.text = Mathf.FloorToInt(timeRemaining).ToString();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(devConsole.ConsoleToggleKey)) 
        {
            devConsole?.Toggle();
        }

        if (devConsole != null && devConsole.IsOpen) 
        {
            if (Input.GetKeyDown(devConsole.ConsoleInputKey)) 
            {
                devConsole.ProcessCommand();
            }
        }
#endif
    }

    public void StartNewRound() 
    {
        currentRound++;
        activeRound = Rounds[currentRound];
        roundElapsedTime = 0f;
        countdownText.text = roundDuration.ToString("F0", CultureInfo.InvariantCulture);

        /* TODO
         * - Clear gameplay scene of any leftover pills / medication thats no longer relevant.
         * - Reset UI with new round data
         * - Configure difficulty data
         * - Initialize spawner with new data (/ StartCoroutine for active round logic here which instructs spawns).
         */

        StartCoroutine(RoundLogic());
    }

    public IEnumerator RoundLogic() 
    {
        while (!IsRoundComplete()) 
        {
            yield return new WaitForSeconds(1f);
        }

        yield break;
    }

    public void EndRound() 
    {
        activeRound = null;

        /* TODO
         * - Update UI with any end of round stats
         * - Show UI
         * - Run any end of round events (e.g dialog events, wait for user input etc.)
         * - Trigger next round (if we're not handling this event via other UI)
         */
    }

    private bool IsRoundComplete() 
    {
        return roundElapsedTime >= roundDuration;
    }

    private bool IsObjectiveAchieved() 
    {
        bool isComplete = true;
        foreach (var objective in activeRound.Objectives) 
        {
            if (collectedMedication.ContainsKey(objective.Pill))
            {
                //N.B.  Setting greater than or equal to condition for success
                //      This may not be what we want for overdosing scenarios
                isComplete &= collectedMedication[objective.Pill] >= objective.Count;
            }
            else 
            {
                return false;
            }
        }
        return isComplete;
    }
}
