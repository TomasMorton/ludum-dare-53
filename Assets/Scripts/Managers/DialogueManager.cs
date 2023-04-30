using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[Serializable][Tooltip("The side the dialogue will appear on")]
public enum DialogueSides
{
    Left = 0,
    Right = 1,
        
}
[Serializable]
public struct Participant
{
    public string name;
    [Tooltip("0 = Left, 1 = Right")]
    public DialogueSides dialogueSide;
    public Sprite portrait;
}

[Serializable][Tooltip("A line of Dialogue made up of:" +
                       "\nA string line that is delivered" +
                       "\nThe participant saying the line.")]
public struct DialogueLine
{
    public bool IsEqual(DialogueLine lineToTestAgainst)
    {
        return line == lineToTestAgainst.line &&
               lineToTestAgainst.dialogueSide == dialogueSide &&
               lineToTestAgainst.participantSpeaking == participantSpeaking;
    }

    public string line;
    public DialogueSides dialogueSide;
    [Tooltip("-1 = All on side" +
             "\n0 = Left-most Participant" +
             "\n1 = 2nd, 2 = 3rd, etc...")]
    public int participantSpeaking;
}

[Serializable]
public struct DialogueStruct
{
    public List<DialogueLine> linesOfDialogue;
    [Tooltip("Element 0 is the left portrait. Element 1 is the right portrait.")]
    public List<Participant> participants;
}

[Serializable]
public struct DialogueGroup
{
    public DialogueStruct DialogueStart;
    public DialogueStruct DialogueMid;
    public DialogueStruct DialogueEnd; 
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    
    //TODO Expand dialogues to include event specific dialogues: e.g. Returning/Ferrying/Start/End
    public DialogueStruct DialogueStart;
    public DialogueStruct DialogueMid;
    public DialogueStruct DialogueEnd;
    public List<DialogueStruct> Dialogues;
    public List<DialogueGroup> DialogueGroups;
    public DialogueStruct currentDialogue = new DialogueStruct();
    public int currentDialogueLine;
    public bool isDialogueActive;
    public Color inactiveSpeakerColor;
    public float inactiveSpeakerSize;

    public KeyCode nextLine;

    public static event Action OnDialogueStart; 
    public static event Action OnDialogueEnd; 

    private void Awake()
    {
        if (!instance) instance = this;
        else Destroy(this);
    }
    
    private void OnEnable()
    {
        GameStateManager.OnStartEnter += SelectRandomDialogueGroup;
        GameStateManager.OnFerryingEnter += StartDialogue;
        GameStateManager.OnReturningEnter += StartDialogue;
        GameStateManager.OnEndEnter += StartDialogue;
    }

    private void OnDisable()
    {
        GameStateManager.OnStartEnter -= SelectRandomDialogueGroup;
        GameStateManager.OnFerryingEnter -= StartDialogue;
        GameStateManager.OnReturningEnter -= StartDialogue;
        GameStateManager.OnEndEnter -= StartDialogue;
    }

    private void SelectRandomDialogueGroup()
    {
        //Get a random dialogue group
        DialogueGroup randomGroup = DialogueGroups[Random.Range(0, DialogueGroups.Count)];
        DialogueStart = randomGroup.DialogueStart;
        DialogueMid  = randomGroup.DialogueMid;
        DialogueEnd = randomGroup.DialogueEnd;
        
        StartDialogue();
    }
    public void StartDialogue()
    {
        //Clear current dialogue.
            currentDialogue = new DialogueStruct();
        
        GameStateManager.GameStates previousState = GameStateManager.Instance.PreviousState;
        GameStateManager.GameStates currentState = GameStateManager.Instance.CurrentState;
        
        //Dont run dialogue if the game was just paused.
        if(previousState == GameStateManager.GameStates.Pause) return;

        //If the game is not ending
        if (currentState != GameStateManager.GameStates.End)
        {
            switch (previousState)
            {
                //Game just started
                case GameStateManager.GameStates.Start:
                    if (currentState == GameStateManager.GameStates.Ferrying) return;
                    //Play intro dialogue
                    currentDialogue = DialogueStart;
                    break;
            
                //Ferried souls successfully
                case GameStateManager.GameStates.Ferrying:
                    if (currentState != GameStateManager.GameStates.Returning) break;
                    //Only occurs on the first play through
                    //Only occurs once on first successful ferry
                    if (GameStateManager.Instance.firstFerryCompleted) return;
                    //Play mid-game dialogue
                    currentDialogue = DialogueMid;
                    break;
            }
        }
        else
        {
            //Play ending dialogue.
            currentDialogue = DialogueEnd;
        }

        //If a dialogue wasn't selected/IsEmpty, abort StartDialogue.
        if (currentDialogue.linesOfDialogue is null) return;

        //Set the current dialogue line to 0
        currentDialogueLine = 0;

        //Show UI/Update Text
        OnDialogueStart?.Invoke();
        isDialogueActive = true;
    }

    public List<Participant> GetCurrentParticipants()
    {
        return currentDialogue.participants;
    }

    public DialogueLine GetCurrentLine()
    {
        return (currentDialogue.linesOfDialogue[currentDialogueLine]);
    }
    public DialogueLine NextLine()
    {
        //Increment currentDialogueLine
        currentDialogueLine++;
        //Has it passed the last line?
        if (currentDialogueLine >= currentDialogue.linesOfDialogue.Count)
        {
            //Yes, dialogue exhausted. End Dialogue.
            EndDialogue();
            return new DialogueLine();
        }
        //return the current line
        return GetCurrentLine();
    }

    public void EndDialogue()
    {//Clear current dialogue.
        currentDialogue = new DialogueStruct();
        
        //Hide UI
        OnDialogueEnd?.Invoke();
        isDialogueActive = false;
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}
