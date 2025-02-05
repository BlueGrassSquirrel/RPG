﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using RPG.Saving;
using RPG.SceneManagement;

namespace RPG.Characters
{
    public class HealthSystem : MonoBehaviour, ISaveable
    {
        // Serlized
        [SerializeField] float maxHealthPoint        = 100f;
        [SerializeField] float currentHealthPoint    = 100f;
        [SerializeField] float deathVanishSeconds    = 2.0f;
        [SerializeField] bool canSelfRegen = false;
        [SerializeField] Image healthBar;
        [SerializeField] AudioClip[] damageSounds;
        [SerializeField] AudioClip[] deathSounds;

        //Private
        const string DEATH_TRIGGER = "Death";
        Animator animator;
        EnemyAI enemyAI;
        AudioSource audioSource;
        Character characterMovement;
        public float healthAsPercentage { get { return currentHealthPoint / maxHealthPoint; } }

        public float GetMaxHealthPoint()
        {
            return maxHealthPoint;
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            characterMovement = GetComponent<Character>();
            enemyAI = GetComponent<EnemyAI>();
            //currentHealthPoint = maxHealthPoint;
        }

        void Update()
        {
            UpdateHealthBar();
            HealthRegen(); // Health Regen if canSelfRegen is True.
        }

        void HealthRegen()
        {
            if(canSelfRegen && enemyAI.GetCurrentState() != EnemyAI.State.attacking && currentHealthPoint<maxHealthPoint)
            {
                if(currentHealthPoint <= 0)
                {
                    return;
                }
                Heal(5f);
            }

        }

        void UpdateHealthBar()
        {
            if(healthBar)   // Enemies May not have health bars to update.
            {
                healthBar.fillAmount = healthAsPercentage;
            }
        }

        public void TakeDamage(float damage)
        {
            bool characterDies = (currentHealthPoint - damage <= 0);
            currentHealthPoint = Mathf.Clamp(currentHealthPoint - damage, 0f, maxHealthPoint);
            var clip = damageSounds[UnityEngine.Random.Range(0, damageSounds.Length)];
            if(clip)
            {
                audioSource.PlayOneShot(clip);
            }
            if (characterDies)
            {
                StartCoroutine(KillCharacter());
            }
        }

        public void Heal(float healAmount)
        {
            currentHealthPoint = Mathf.Clamp(currentHealthPoint + healAmount, 0f, maxHealthPoint);
        }

        IEnumerator KillCharacter()
        {
            characterMovement.Kill();
            characterMovement.GetCapsuleCollider().enabled = false; // Disabling the collider so when the enemy dies the player can path throgh thier dead bodys and not to have a glitch.
            animator.SetTrigger(DEATH_TRIGGER);
            var playerComponent = GetComponent<PlayerControl>();
            if(deathSounds.Length != 0)
            {
                audioSource.clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
                audioSource.Play();
            }
            characterMovement.GetNavMeshAgent().speed = 0;
            //yield return new WaitForSecondsRealtime(audioSource.clip.length);
            yield return null;
            if (playerComponent && playerComponent.isActiveAndEnabled) // relying on Lazy Evaluation (Google if you need help)
            {
                FindObjectOfType<SavingWrapper>().DeleteSave();
                SceneManager.LoadSceneAsync(0);
                //SceneManager.LoadScene(0); // TODO check for scene Error
            }
            else // Assume is enemy for now, reconsider other NPCs Later 
            {
                characterMovement.GetNavMeshAgent().speed = 0;
                //DestroyObject(gameObject, deathVanishSeconds + audioSource.clip.length);
                //Destroy(gameObject, deathVanishSeconds + audioSource.clip.length);
            }
        }

        void KillCharacterWithoutDestroy()
        {
            Character character = GetComponent<Character>();
            character.Kill();
            character.GetCapsuleCollider().enabled = false;
            GetComponent<Animator>().SetTrigger(DEATH_TRIGGER);
            gameObject.SetActive(false); // Getting around the idea of saving each destroied object. instead we set it to active so it won't be renderd.
        }

        public bool IsDead()
        {
            return currentHealthPoint <= 0;
        }

        public object CaptureState()
        {
            return currentHealthPoint;
        }

        public void RestoreState(object state)
        {
            // Restore health points
            currentHealthPoint = (float)state;
            
            if (currentHealthPoint <= 0.0f)
            {
                KillCharacterWithoutDestroy(); // TODO save death animation when the player is dead.
            }
        }
    }
}