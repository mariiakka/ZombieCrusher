using System.Collections;
using UnityEngine;

/// <summary>
/// Головний AI Boss-а: State Machine + Dash + рух.
/// Потребує на тому самому GameObject: Rigidbody2D, BossHealth, BossShooting.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BossHealth))]
[RequireComponent(typeof(BossShooting))]
public class BossController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  INSPECTOR
    // ─────────────────────────────────────────────

    [Header("Гравець")]
    public Transform playerTransform;

    [Header("Фази (3 штуки)")]
    public BossPhase[] phases = new BossPhase[3];

    [Header("Dash")]
    public float dashSpeed     = 12f;
    public float dashDuration  = 0.3f;
    public float dashCooldown  = 5f;

    [Header("Арена (межі руху)")]
    public Vector2 arenaMin = new Vector2(-8f, -8f);
    public Vector2 arenaMax = new Vector2( 8f,  8f);

    [Header("Debug")]
    public bool showGizmos = true;

    // ─────────────────────────────────────────────
    //  PUBLIC READ-ONLY
    // ─────────────────────────────────────────────

    public BossState  CurrentState  { get; private set; }
    public int        CurrentPhase  { get; private set; }

    // ─────────────────────────────────────────────
    //  PRIVATE
    // ─────────────────────────────────────────────

    private Rigidbody2D  rb;
    private BossShooting shooting;

    private float stateTimer;
    private float dashCooldownTimer;
    private bool  isDashing;

    private Vector2 patrolTarget;
    private float   flankSide = 1f;

    // ─────────────────────────────────────────────
    //  ENUM
    // ─────────────────────────────────────────────

    public enum BossState
    {
        Patrol,
        Chase,
        Strafe,
        Orbit,
        Reposition,
        Berserk
    }

    // ─────────────────────────────────────────────
    //  LIFECYCLE
    // ─────────────────────────────────────────────

    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        shooting = GetComponent<BossShooting>();

        CurrentPhase = 0;
        CurrentState = BossState.Patrol;
        patrolTarget = GetRandomArenaPoint();
    }

    void Update()
    {
        stateTimer        += Time.deltaTime;
        dashCooldownTimer += Time.deltaTime;

        if (!isDashing)
            RunStateMachine();
    }

    // ─────────────────────────────────────────────
    //  CALLED BY BossHealth
    // ─────────────────────────────────────────────

    /// <summary>BossHealth викликає це після кожного TakeDamage.</summary>
    public void OnHealthChanged(float hpRatio)
    {
        int newPhase = CalculatePhase(hpRatio);
        if (newPhase != CurrentPhase)
        {
            CurrentPhase = newPhase;
            OnPhaseChanged(newPhase);
        }

        // Якщо Boss у пасивному стані — прокинутись
        if (CurrentState == BossState.Patrol || CurrentState == BossState.Reposition)
            ChangeState(BossState.Chase);
    }

    // ─────────────────────────────────────────────
    //  PHASE
    // ─────────────────────────────────────────────

    int CalculatePhase(float hpRatio)
    {
        for (int i = phases.Length - 1; i >= 0; i--)
            if (hpRatio <= phases[i].hpThreshold)
                return i;
        return 0;
    }

    void OnPhaseChanged(int phase)
    {
        Debug.Log($"[Boss] ФАЗА {phase + 1} — {phases[phase].phaseName}");
        shooting.SetPhase(phases[phase]);

        if (phase == phases.Length - 1)
            ChangeState(BossState.Berserk);
        else
            ChangeState(BossState.Chase);
    }

    // ─────────────────────────────────────────────
    //  STATE MACHINE
    // ─────────────────────────────────────────────

    void RunStateMachine()
    {
        if (playerTransform == null) return;

        BossPhase ph      = phases[CurrentPhase];
        Vector2   toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        float     dist    = toPlayer.magnitude;
        float     angle   = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;

        switch (CurrentState)
        {
            case BossState.Patrol:      DoPatrol(ph, dist, angle);      break;
            case BossState.Chase:       DoChase(ph, dist, angle);        break;
            case BossState.Strafe:      DoStrafe(ph, dist, angle);       break;
            case BossState.Orbit:       DoOrbit(ph, dist, angle);        break;
            case BossState.Reposition:  DoReposition(ph, dist, angle);   break;
            case BossState.Berserk:     DoBerserk(ph, dist, angle);      break;
        }
    }

    // ── Patrol ──────────────────────────────────

    void DoPatrol(BossPhase ph, float dist, float angleToPlayer)
    {
        if (dist < ph.chaseRange)          { ChangeState(BossState.Chase);  return; }
        if (stateTimer > ph.patrolStateTime){ ChangeState(BossState.Strafe); return; }

        Vector2 dir = patrolTarget - (Vector2)transform.position;
        float   a   = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        RotateToward(a, ph.rotationSpeed);
        MoveForward(ph.speed * 0.65f);

        if (dir.magnitude < 1f)
            patrolTarget = GetRandomArenaPoint();
    }

    // ── Chase ────────────────────────────────────

    void DoChase(BossPhase ph, float dist, float angleToPlayer)
    {
        RotateToward(angleToPlayer, ph.rotationSpeed);
        MoveForward(ph.speed);

        if (dist < ph.orbitRange)
            ChangeState(BossState.Orbit);
        else if (dist > ph.repositionRange && stateTimer > 2f)
            ChangeState(BossState.Reposition);

        if (ph.canDash && dashCooldownTimer >= dashCooldown && dist < ph.dashTriggerRange)
            StartCoroutine(DoDash());
    }

    // ── Strafe ───────────────────────────────────

    void DoStrafe(BossPhase ph, float dist, float angleToPlayer)
    {
        RotateToward(angleToPlayer, ph.rotationSpeed * 1.3f);

        float rad    = (angleToPlayer + 90f * flankSide + 90f) * Mathf.Deg2Rad;
        Vector2 move = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        rb.velocity = Vector2.Lerp(rb.velocity, move * ph.speed, Time.deltaTime * 5f);

        if (stateTimer > ph.strafeSwitchTime) { flankSide *= -1f; ResetStateTimer(); }
        if (dist > ph.repositionRange)         ChangeState(BossState.Chase);
        else if (dist < ph.orbitRange)         ChangeState(BossState.Orbit);
    }

    // ── Orbit ────────────────────────────────────

    void DoOrbit(BossPhase ph, float dist, float angleToPlayer)
    {
        RotateToward(angleToPlayer, ph.rotationSpeed * 1.5f);

        float rad    = (angleToPlayer + 90f * flankSide + 90f) * Mathf.Deg2Rad;
        Vector2 move = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        rb.velocity = Vector2.Lerp(rb.velocity, move * ph.speed * 0.85f, Time.deltaTime * 5f);

        if (stateTimer > ph.orbitStateTime || dist > ph.orbitRange * 1.5f)
        {
            flankSide *= -1f;
            ChangeState(BossState.Strafe);
        }
    }

    // ── Reposition ───────────────────────────────

    void DoReposition(BossPhase ph, float dist, float angleToPlayer)
    {
        float rad    = (angleToPlayer + 180f + 90f) * Mathf.Deg2Rad;
        Vector2 move = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        RotateToward(angleToPlayer + 180f, ph.rotationSpeed);
        rb.velocity = Vector2.Lerp(rb.velocity, move * ph.speed * 0.8f, Time.deltaTime * 4f);

        if (stateTimer > ph.repositionTime || dist < ph.chaseRange)
            ChangeState(BossState.Chase);
    }

    // ── Berserk ──────────────────────────────────

    void DoBerserk(BossPhase ph, float dist, float angleToPlayer)
    {
        RotateToward(angleToPlayer, ph.rotationSpeed * 1.2f);
        MoveForward(ph.speed * 1.1f);

        if (ph.canDash && dashCooldownTimer >= dashCooldown * 0.6f)
            StartCoroutine(DoDash());
    }

    // ─────────────────────────────────────────────
    //  DASH
    // ─────────────────────────────────────────────

    IEnumerator DoDash()
    {
        isDashing         = true;
        dashCooldownTimer = 0f;
        Debug.Log("[Boss] DASH!");

        Vector2 dir     = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
        float   elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.velocity = dir * dashSpeed;
            elapsed          += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
    }

    // ─────────────────────────────────────────────
    //  HELPERS
    // ─────────────────────────────────────────────

    void ChangeState(BossState next)
    {
        if (CurrentState == next) return;
        CurrentState = next;
        ResetStateTimer();
        Debug.Log($"[Boss] → {next}");
    }

    void ResetStateTimer() => stateTimer = 0f;

    void RotateToward(float targetDeg, float speed)
    {
        float current = transform.eulerAngles.z;
        float angle   = Mathf.MoveTowardsAngle(current, targetDeg, speed * Time.deltaTime * Mathf.Rad2Deg);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void MoveForward(float speed)
    {
        rb.velocity = Vector2.Lerp(rb.velocity, (Vector2)transform.up * speed, Time.deltaTime * 5f);
    }

    Vector2 GetRandomArenaPoint()
    {
        return new Vector2(
            Random.Range(arenaMin.x, arenaMax.x),
            Random.Range(arenaMin.y, arenaMax.y)
        );
    }

    // ─────────────────────────────────────────────
    //  GIZMOS
    // ─────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (!showGizmos || phases == null || phases.Length == 0) return;
        int   idx = Mathf.Clamp(CurrentPhase, 0, phases.Length - 1);
        BossPhase ph = phases[idx];

        Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, ph.chaseRange);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, ph.orbitRange);

        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, ph.dashTriggerRange);

        // Арена
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((arenaMin.x + arenaMax.x) / 2f, (arenaMin.y + arenaMax.y) / 2f);
        Vector3 size   = new Vector3(arenaMax.x - arenaMin.x, arenaMax.y - arenaMin.y);
        Gizmos.DrawWireCube(center, size);
    }
}