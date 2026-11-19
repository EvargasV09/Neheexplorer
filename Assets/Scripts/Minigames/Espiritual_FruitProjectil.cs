using UnityEngine;

public class Espiritual_FruitProjectil : MonoBehaviour
{
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, 4f); // Se destruye sola si no choca
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        BossHealth boss = col.collider.GetComponent<BossHealth>();
        if (boss != null)
            boss.TakeDamage(damage);

        Destroy(gameObject);
    }
}
