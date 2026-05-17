using UnityEngine;

/// <summary>
/// Прикріпи на Prefab кулі Boss-а.
/// Prefab має мати: Rigidbody2D + Collider2D (Is Trigger = true) + цей скрипт.
/// </summary>
public class BossBullet : MonoBehaviour
{
    [Header("Налаштування")]
    [Tooltip("Встановлюється автоматично з BossShooting")]
    public float damage   = 10f;
    public float lifetime = 4f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // ── Підключи свій компонент здоров'я гравця ──
        // Варіант 1: через інтерфейс
        // other.GetComponent<IDamageable>()?.TakeDamage(damage);

        // Варіант 2: напряму
        // other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        Debug.Log($"[BossBullet] Влучив у гравця, пошкодження: {damage}");
        Destroy(gameObject);
    }
}
