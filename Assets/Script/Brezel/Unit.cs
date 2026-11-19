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

    NavMeshAgent agent;
    float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!battle) return;

        Unit target = FindNearest();
        if (target == null)
        {
            agent.isStopped = true;
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > range)
        {
            agent.isStopped = false;
            agent.SetDestination(target.transform.position);
        }
        else
        {
            agent.isStopped = true;
            timer += Time.deltaTime;
            if (timer >= attackDelay)
            {
                timer = 0f;
                target.TakeDamage(damage);
                if (attackSound != null) attackSound.Play();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0) Destroy(gameObject);
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