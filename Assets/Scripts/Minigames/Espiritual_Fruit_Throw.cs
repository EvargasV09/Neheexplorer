using UnityEngine;

public class FruitThrower : MonoBehaviour
{
    [SerializeField] GameObject fruitPrefab;
    [SerializeField] Transform firePoint;
    [SerializeField] float throwForce = 12f;
    [SerializeField] float cooldown = 0.4f;

    float nextThrowTime;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextThrowTime)
        {
            nextThrowTime = Time.time + cooldown;
            Throw();
        }
    }

    void Throw()
    {
        // Posición del mouse en el mundo
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;

        Vector2 direction = (mouse - firePoint.position).normalized;

        GameObject fruit = Instantiate(fruitPrefab, firePoint.position, Quaternion.identity);
        fruit.GetComponent<Rigidbody2D>().linearVelocity = direction * throwForce;
    }
}