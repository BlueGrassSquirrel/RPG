﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Characters;
using UnityEngine.Playables;
using RPG.Saving;

public class QuestSystem : MonoBehaviour
{
    [Header("Dialogue Manager Reference")]
    [SerializeField] DialogueManager dialogueManager;

    [Header("Dialogue")]
    [SerializeField] Dialogue startDialogue;
    [SerializeField] Dialogue givingQuestDialogue;
    [SerializeField] Dialogue endQuestDialogue;


    [Header("Questing")]
    [SerializeField] string questTypeName;
    [SerializeField] GameObject currentQuests;
    [SerializeField] QuestUI questUI;
    [SerializeField] GameObject questExclemenation;
    [SerializeField] PlayableDirector playableDirector;

    private EnemyAI enemyAI;
    private Quest quest;
    private bool isQuestAssigned;
    private bool hasFinishedQuest;
    private bool hasRecivedReward;
    private bool hasSequencePlayed = false;
    

    public GameObject GetQuestsObject()
    {
        return currentQuests;
    }

    // Use this for initialization
    void Start ()
    {
        isQuestAssigned = false;
        hasFinishedQuest = false;
        hasRecivedReward = false;
        enemyAI = GetComponent<EnemyAI>();

        if(playableDirector)
        {
            dialogueManager.onFinishedDialogue += StartCinematicsSequence;
        }

    }
	
    public void Interact()
    {
        
        if(!isQuestAssigned && !hasFinishedQuest)
        {
            dialogueManager.StartDialogue(startDialogue);
            AssignQuest();
        }
        else if(isQuestAssigned && !hasFinishedQuest)
        {
            CheckQuest();
        }
        else
        {
        }
    }

	void AssignQuest()
    {
        questExclemenation.SetActive(false);
        dialogueManager.AddToDialogue(givingQuestDialogue);
        isQuestAssigned = true;
        quest = (Quest)currentQuests.AddComponent(System.Type.GetType(questTypeName));
        quest.questUI = questUI;
        questUI.SetQuestUIVisibility(true);
        questUI.SetCurrentQuest(quest);
    }

    void AssignQuestBackFromSaving()
    {
        isQuestAssigned = true;
        quest = (Quest)currentQuests.AddComponent(System.Type.GetType(questTypeName));
        questUI.SetQuestUIVisibility(true);
        questUI.SetCurrentQuest(quest);

    }

    public Quest GetQuest()
    {
        return quest;
    }


    void CheckQuest()
    {
        if(quest.completed)
        {
            questUI.SetCheckmarkVisibility(true);
            EndQuest();
        }
        else
        {
            // add Dialogue : you need to finish the quest !
            //Debug.Log("Adding comingBackToNPCNotFinishingQuest");
        }
    }

    void EndQuest()
    {
        quest.GiveReward();
        hasFinishedQuest = true;
        isQuestAssigned = false;
        hasRecivedReward = true;
        dialogueManager.StartDialogue(endQuestDialogue);
        ResetQuestUI();
    }

    void ResetQuestUI()
    {
        questUI.CleanQuestUI();
        questUI.SetCheckmarkVisibility(false);
        questUI.SetQuestUIVisibility(false);
    }

    void StartCinematicsSequence(bool start)
    {
        print("Starting Cinematics Sequence");
        if(playableDirector && !hasSequencePlayed)
        {
            playableDirector.Play();
            Debug.Log(playableDirector.name);
            hasSequencePlayed = true;
        }
    }


    /*// For the isaveable interface
    public object CaptureState()
    {
        return hasFinishedQuest;
    }

    public void RestoreState(object state)
    {
        
       
    }*/
}
