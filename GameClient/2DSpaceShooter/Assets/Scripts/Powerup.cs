using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class Powerup : NetworkBehaviour
{
    public static int NumPowerUps = 0;

    public NetworkVariable<Buff.BuffType> BuffType = new();

    [SerializeField]
    private Renderer powerUpGlow;

    [SerializeField]
    private Renderer powerUpGlow2;

    [SerializeField]
    private UIDocument m_PowerUpUIDocument;
    private VisualElement m_PowerUpRootVisualElement;
    private VisualElement m_PowerUpUIWrapper;
    private TextElement m_PowerUpLabel;
    private Camera m_MainCamera;

    private IPanel m_Panel;

    private void Awake()
    {
        m_MainCamera = Camera.main;
    }

    private void OnEnable()
    {
        m_PowerUpRootVisualElement = m_PowerUpUIDocument.rootVisualElement;
        m_PowerUpUIWrapper = m_PowerUpRootVisualElement.Q<VisualElement>("PowerUpUIBox");
        m_PowerUpLabel = m_PowerUpRootVisualElement.Q<TextElement>("PowerUpLabel");
        m_Panel = m_PowerUpUIWrapper.panel;
    }

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
        float dir = -70.0f;
        transform.rotation = Quaternion.Euler(0, 180, dir);
        GetComponent<Rigidbody2D>().angularVelocity = dir;

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

        m_PowerUpLabel.text = buffType.ToString().ToUpper();

        if (buffType == Buff.BuffType.QuadDamage)
        {
            m_PowerUpLabel.text = "Quad Damage";
        }

        m_PowerUpLabel.style.color = buffColor;
    }

    private void LateUpdate()
    {
        SetLabelPosition();
    }

    private void SetLabelPosition()
    {
        if (m_Panel != null)
        {
            Vector2 screenPosition = RuntimePanelUtils.CameraTransformWorldToPanel(
                m_Panel,
                transform.position,
                m_MainCamera
            );
            m_PowerUpUIWrapper.transform.position = screenPosition;
        }
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
