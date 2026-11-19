using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    [SerializeField] int maxHealth = 10;
    [SerializeField] Slider healthBar;

    int currentHealth;
    private bool _isDefeated;
    public event System.Action Defeated;

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }

    public void TakeDamage(int amount)
    {
        if (_isDefeated || amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        healthBar.value = currentHealth;

        if (currentHealth == 0)
        {
            _isDefeated = true;
            Defeated?.Invoke();
        }

        if (currentHealth == 0)
            Debug.Log("�Jefe vencido!"); // M�s adelante: pasa a la siguiente fase
    }
}