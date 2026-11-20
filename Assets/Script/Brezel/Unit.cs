using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    public static bool battle;
    public AudioSource attackSound;
    public string enemyTag = "Enemy";
    public float hp = 100f;
    public float damage = 10f;
    public float range = 1.5f;
    public float attackDelay = 1f;

    public float deathDelay = 1.6f;   // Death 애니메이션이 끝날 때까지 기다렸다가 삭제

    NavMeshAgent agent;
    Animator anim;
    float timer;
    bool dead;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!battle || dead) return;

        Unit target = FindNearest();
        if (target == null)
        {
            agent.isStopped = true;
            SetMoving(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > range)
        {
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
            SetMoving(true);
        }
        else
        {
            agent.isStopped = true;
            SetMoving(false);
            timer += Time.deltaTime;
            if (timer >= attackDelay)
            {
                timer = 0f;
                if (anim != null) anim.SetTrigger("Attack");
                target.TakeDamage(damage);
                if (attackSound != null) attackSound.Play();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (dead) return;
        hp -= amount;
        if (hp <= 0) Die();
        else if (anim != null) anim.SetTrigger("Hit");
    }

    void Die()
    {
        dead = true;
        gameObject.tag = "Untagged";          // 다른 유닛이 더 이상 노리지 않게
        if (agent != null) { agent.isStopped = true; agent.enabled = false; }
        if (anim != null) { anim.SetBool("Moving", false); anim.SetTrigger("Death"); }
        Destroy(gameObject, anim != null ? deathDelay : 0f);
    }

    void SetMoving(bool moving)
    {
        if (anim != null) anim.SetBool("Moving", moving);
    }

    Unit FindNearest()
    {
        Unit nearest = null;
        float best = Mathf.Infinity;

        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag(enemyTag))
        {
            float d = Vector3.Distance(transform.position, enemy.transform.position);
            if (d < best)
            {
                best = d;
                nearest = enemy.GetComponent<Unit>();
            }
        }
        return nearest;
    }
}