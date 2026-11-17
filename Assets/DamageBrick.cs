using UnityEngine;

public class DamageBrick : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float damageCooldown = 1f;

    private float nextDamageTime;

    private void OnTriggerEnter(Collider other)
    {
        TryDamagePlayer(other.GetComponentInParent<PlayerHealth>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryDamagePlayer(
            collision.collider.GetComponentInParent<PlayerHealth>());
    }

    public void TryDamagePlayer(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
        {
            Debug.LogWarning(
                "Kill brick touched something without PlayerHealth.");
            return;
        }

        if (Time.time < nextDamageTime)
        {
            return;
        }

        nextDamageTime = Time.time + damageCooldown;

        Debug.Log($"Kill brick damaging player for {damage}.");
        playerHealth.TakeDamage(damage);
    }
}