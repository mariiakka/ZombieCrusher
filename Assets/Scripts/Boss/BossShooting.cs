using System.Collections;
using UnityEngine;

/// <summary>
/// Стрільба Boss-а. Прикріпи на той самий GameObject що й BossController.
/// FirePoint — дочірній порожній GameObject на носі машини.
/// </summary>
public class BossShooting : MonoBehaviour
{
    [Header("Посилання")]
    public Transform  firePoint;     // Дочірній Empty на носі Boss-а
    public GameObject bulletPrefab;  // Prefab кулі (має BossBullet + Rigidbody2D + Collider2D)

    // Поточна фаза береться з BossController через SetPhase()
    private BossPhase currentPhase;
    private float     shootTimer;
    private bool      isShooting;

    void Awake()
    {
        // Ініціалізуємо дефолтну фазу, щоб не було null
        currentPhase = new BossPhase();
    }

    void Update()
    {
        if (isShooting) return;

        shootTimer += Time.deltaTime;
        if (shootTimer >= currentPhase.shootCooldown)
        {
            shootTimer = 0f;
            StartCoroutine(ShootBurst());
        }
    }

    /// <summary>Викликається з BossController при зміні фази.</summary>
    public void SetPhase(BossPhase phase)
    {
        currentPhase = phase;
        shootTimer   = 0f;
    }

    IEnumerator ShootBurst()
    {
        isShooting = true;

        for (int i = 0; i < currentPhase.burstCount; i++)
        {
            FireSpread();
            yield return new WaitForSeconds(currentPhase.burstInterval);
        }

        isShooting = false;
    }

    void FireSpread()
    {
        if (bulletPrefab == null || firePoint == null) return;

        int   count = currentPhase.spreadCount;
        float total = currentPhase.spreadAngle;

        for (int i = 0; i < count; i++)
        {
            float offset = count > 1
                ? Mathf.Lerp(-total * 0.5f, total * 0.5f, (float)i / (count - 1))
                : 0f;

            Quaternion rot    = firePoint.rotation * Quaternion.Euler(0f, 0f, offset);
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rot);

            // Швидкість
            Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
            if (bRb != null)
                bRb.velocity = rot * Vector2.up * currentPhase.bulletSpeed;

            // Пошкодження
            BossBullet bb = bullet.GetComponent<BossBullet>();
            if (bb != null)
                bb.damage = currentPhase.bulletDamage;
        }
    }
}