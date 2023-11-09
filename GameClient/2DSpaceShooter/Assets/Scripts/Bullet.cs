using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private bool m_Bounce;
    private int m_Damage = 5;
    private ShipControl m_Owner;

    public GameObject explosionParticle;

    public void Config(ShipControl owner, int damage, bool bounce, float lifetime)
    {
        m_Owner = owner;
        m_Damage = damage;
        m_Bounce = bounce;
        if (IsServer)
        {
            // This is bad code don't use invoke.
            Invoke(nameof(DestroyBullet), lifetime);
        }
    }

    public override void OnNetworkDespawn()
    {
        // This is inefficient, the explosion object could be pooled.
        _ = Instantiate(
            explosionParticle,
            transform.position + new Vector3(0, 0, -2),
            Quaternion.identity
        );
    }

    private void DestroyBullet()
    {
        if (!NetworkObject.IsSpawned)
        {
            return;
        }

        NetworkObject.Despawn(true);
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (IsServer)
        {
            Rigidbody2D bulletRb = GetComponent<Rigidbody2D>();
            bulletRb.velocity = velocity;
            SetVelocityClientRpc(velocity);
        }
    }

    [ClientRpc]
    private void SetVelocityClientRpc(Vector2 velocity)
    {
        if (!IsHost)
        {
            Rigidbody2D bulletRb = GetComponent<Rigidbody2D>();
            bulletRb.velocity = velocity;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject otherObject = other.gameObject;

        if (!NetworkManager.Singleton.IsServer || !NetworkObject.IsSpawned)
        {
            return;
        }

        if (otherObject.TryGetComponent<Asteroid>(out Asteroid asteroid))
        {
            asteroid.Explode();
            DestroyBullet();
            return;
        }

        if (
            m_Bounce == false
            && (otherObject.CompareTag("Wall") || otherObject.CompareTag("Obstacle"))
        )
        {
            DestroyBullet();
        }

        if (otherObject.TryGetComponent<ShipControl>(out ShipControl shipControl))
        {
            if (shipControl != m_Owner)
            {
                shipControl.TakeDamage(m_Damage);
                DestroyBullet();
            }
        }
    }
}
