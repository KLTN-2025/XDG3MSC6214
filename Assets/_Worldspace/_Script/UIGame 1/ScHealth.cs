using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Workspace._Scripts.UIGame
{
    public class ScHealth : MonoBehaviour
    {
        [Header("setting")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private int currentHealth;

        [Header("UI")]
        [SerializeField] private Image[] healthFrames;    
        [SerializeField] private Image[] healthIcons;     

    
        public event Action<int> OnHealthChanged;
        public event Action OnHealthDepleted;

        private void Awake()
        {
            InitializeHealth();
        }


        private void InitializeHealth()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }

    
        private void UpdateHealthUI()
        {
        
            for (int i = 0; i < healthIcons.Length; i++)
            {
                if (healthIcons[i] != null)
                {
                
                    healthIcons[i].gameObject.SetActive(i < currentHealth);
                }

                if (healthFrames[i] != null)
                {
               
                    healthFrames[i].gameObject.SetActive(true);
                }
            }

        
            OnHealthChanged?.Invoke(currentHealth);
        }

    
        public void TakeDamage(int damageAmount = 1)
        {
            if (currentHealth <= 0) return;

            currentHealth -= damageAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            UpdateHealthUI();

        
            if (currentHealth <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }

    
        public void Heal(int healAmount = 1)
        {
            currentHealth += healAmount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthUI();
        }

    
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            UpdateHealthUI();
        }

   
        public void SetHealth(int health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            UpdateHealthUI();
        }

    
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsFullHealth => currentHealth >= maxHealth;
        public bool IsHealthDepleted => currentHealth <= 0;
    }
}