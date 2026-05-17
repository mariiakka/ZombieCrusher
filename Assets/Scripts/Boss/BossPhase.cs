using UnityEngine;

/// <summary>
/// Налаштування однієї фази Boss-а.
/// Редагується прямо в Inspector на компоненті BossController.
/// </summary>
[System.Serializable]
public class BossPhase
{
    [Header("Інфо")]
    public string phaseName = "Phase 1";

    [Header("Активація")]
    [Range(0f, 1f)]
    [Tooltip("При якому % HP активується фаза. Phase 1 = 1.0, Phase 2 = 0.6, Phase 3 = 0.3")]
    public float hpThreshold = 1f;

    [Header("Рух")]
    public float speed = 3f;
    [Tooltip("Швидкість повороту, радіан/сек")]
    public float rotationSpeed = 2f;

    [Header("Стрільба")]
    public float shootCooldown = 2f;
    [Tooltip("Кількість пострілів в одній серії")]
    public int burstCount = 1;
    [Tooltip("Пауза між пострілами серії")]
    public float burstInterval = 0.15f;
    [Tooltip("Кількість куль за один постріл (спред)")]
    public int spreadCount = 1;
    [Tooltip("Кут розльоту куль, градуси")]
    public float spreadAngle = 20f;
    public float bulletSpeed = 8f;
    public float bulletDamage = 10f;

    [Header("Часові рамки станів")]
    public float patrolStateTime = 5f;
    public float strafeSwitchTime = 2.5f;
    public float orbitStateTime = 3f;
    public float repositionTime = 2f;

    [Header("Дистанції")]
    [Tooltip("З якої відстані Boss помічає гравця")]
    public float chaseRange = 10f;
    [Tooltip("Дистанція для переходу в Orbit")]
    public float orbitRange = 3f;
    [Tooltip("Дистанція для переходу в Reposition")]
    public float repositionRange = 14f;
    [Tooltip("Максимальна дистанція для Dash-атаки")]
    public float dashTriggerRange = 7f;

    [Header("Dash")]
    public bool canDash = false;
}
