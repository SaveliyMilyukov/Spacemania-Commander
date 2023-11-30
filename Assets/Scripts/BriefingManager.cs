using UnityEngine.UI;
using UnityEngine;

public class BriefingManager : MonoBehaviour
{
    [SerializeField] Image[] briefingPortraits;
    [Space(5)]
    [SerializeField] BriefingCommand[] commands;
    [SerializeField] int commandIndex;
    [SerializeField] float timeToNextCommand = 5f;
    [SerializeField] Text subtitles;

    void Awake()
    {
        if(commands.Length <= 0)
        {
            Debug.LogWarning(gameObject.name + " <BriefingManager>() is haven't commands!");
            enabled = false;
            return;
        }

        for (int i = 0; i < briefingPortraits.Length; i++)
        {
            briefingPortraits[i].color = Color.clear;
        }

        RepeatBriefing();      
    }

    
    void Update()
    {
        if(timeToNextCommand <= 0)
        {
            timeToNextCommand = 1f;
            SetNextCommand();
        }
        else
        {
            timeToNextCommand -= Time.deltaTime;
        }
    }

    void SetNextCommand()
    {
        commandIndex++;
        if (commandIndex >= commands.Length)
        {
            for(int i = 0; i < briefingPortraits.Length; i++)
            {
                briefingPortraits[i].color = Color.clear;
            }

            subtitles.text = "";

            Debug.Log("End of Briefing!");
            enabled = false;
            return;
        }

        BriefingCommand cmd = commands[commandIndex];

        timeToNextCommand = cmd.delayToNextCommand;
        switch(cmd.commandType)
        {
            case BriefingCommand.BriefingCommandType.PortraitTalk:
                subtitles.text = cmd.character.characterName + ": " + cmd.textOnScreen;
                break;
            case BriefingCommand.BriefingCommandType.ShowPortrait:
                briefingPortraits[cmd.portraitIndex].color = Color.white;
                briefingPortraits[cmd.portraitIndex].sprite = cmd.character.portrait;
                break;
            case BriefingCommand.BriefingCommandType.HidePortrait:
                briefingPortraits[cmd.portraitIndex].color = Color.clear;
                break;
        }
    }

    public void RepeatBriefing()
    {
        commandIndex = -1;
        timeToNextCommand = 1f;
        SetNextCommand();
    }
}

[System.Serializable]
public class BriefingCharacter
{
    public string characterName = "None";
    public Sprite portrait;
}

[System.Serializable]
public class BriefingCommand
{
    public enum BriefingCommandType
    {
        PortraitTalk, ShowPortrait, HidePortrait
    }

    [SerializeField] string commandName;
    public BriefingCommandType commandType;
    public int portraitIndex = 0;
    public BriefingCharacter character;
    public string textOnScreen;
    [Space(5)]
    public float delayToNextCommand = 0f;
}
