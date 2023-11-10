using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Powerup : NetworkBehaviour
{
    public static int NumPowerUps = 0;

    public NetworkVariable<Buff.BuffType> BuffType = new(Buff.BuffType.Speed);

    [SerializeField]
    private Renderer powerUpGlow;

    [SerializeField]
    private Renderer powerUpGlow2;

    [SerializeField]
    private TMP_Text label;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            OnStartClient();
        }

        if (IsServer)
        {
            OnStartServer();
        }

        UpdateVisuals(BuffType.Value);
        BuffType.OnValueChanged += OnBuffTypeChanged;
    }

    public override void OnNetworkDespawn()
    {
        BuffType.OnValueChanged -= OnBuffTypeChanged;
    }

    private void OnStartClient()
    {
        if (!IsServer)
        {
            NumPowerUps += 1;
        }
    }

    private void OnStartServer()
    {
        NumPowerUps += 1;
    }

    private void OnBuffTypeChanged(Buff.BuffType previousValue, Buff.BuffType newValue)
    {
        UpdateVisuals(newValue);
    }

    private void UpdateVisuals(Buff.BuffType buffType)
    {
        Color buffColor = Buff.buffColors[(int)buffType];
        GetComponent<Renderer>().material.color = buffColor;
        powerUpGlow.material.SetColor("_Color", buffColor);
        powerUpGlow.material.SetColor("_EmissiveColor", buffColor);
        powerUpGlow2.material.SetColor("_Color", buffColor);
        powerUpGlow2.material.SetColor("_EmissiveColor", buffColor);

        label.text = buffType.ToString().ToUpper();

        if (buffType == Buff.BuffType.QuadDamage)
        {
            label.text = "Quad Damage";
        }

        label.color = buffColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer)
        {
            return;
        }

        ShipControl otherShipControl = other.gameObject.GetComponent<ShipControl>();
        if (otherShipControl != null)
        {
            otherShipControl.AddBuff(BuffType.Value);
            DestroyPowerUp();
        }
    }

    private void DestroyPowerUp()
    {
        AudioSource.PlayClipAtPoint(GetComponent<AudioSource>().clip, transform.position);
        NumPowerUps -= 1;

        NetworkObject.Despawn(true);
    }
}
