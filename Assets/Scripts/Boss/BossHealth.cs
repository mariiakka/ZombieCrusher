using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Здоров'я Boss-а. Прикріпи на той самий GameObject що й BossController.
/// Викликай TakeDamage() з куль або зіткнень гравця.
/// </summary>
public class BossHealth : MonoBehaviour
{
    [Header("Налаштування")]
    public float maxHp = 500f;

    [Header("Події (UnityEvent)")]
    public UnityEvent<float> onDamaged;      // передає поточний % HP (0..1)
    public UnityEvent<int>   onPhaseChanged; // передає номер нової фази (0,1,2)
    public UnityEvent        onDeath;

    public float CurrentHp { get; private set; }
    public float HpRatio    => CurrentHp / maxHp;
    public bool  IsDead     => CurrentHp <= 0f;

    private BossController controller;

    void Awake()
    {
        CurrentHp  = maxHp;
        controller = GetComponent<BossController>();
    }

    /// <summary>Викликається з BossBulletReceiver або будь-якого іншого місця.</summary>
    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHp = Mathf.Max(0f, CurrentHp - amount);
        onDamaged?.Invoke(HpRatio);

        // Повідомляємо контролер — він перевірить зміну фази
        controller?.OnHealthChanged(HpRatio);

        if (CurrentHp <= 0f)
            Die();
    }

    void Die()
    {
        Debug.Log("[BossHealth] Boss знищено!");
        onDeath?.Invoke();

        // Зупинити фізику
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb) rb.velocity = Vector2.zero;

        // Вимкнути AI
        if (controller) controller.enabled = false;

        // Тут: активуй VFX, дай нагороду, заблокуй колайдер тощо
        // GetComponent<Collider2D>().enabled = false;
        // Destroy(gameObject, 2f);
    }
}