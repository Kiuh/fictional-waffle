﻿using NetScripts;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

public class Buff
{
    public enum BuffType
    {
        Speed,
        Rotate,
        Triple,
        Double,
        Health,
        Energy,
        QuadDamage,
        Bounce,
        Last
    };

    public static Color[] buffColors =
    {
        Color.red,
        new(0.5f, 0.3f, 1),
        Color.cyan,
        Color.yellow,
        Color.green,
        Color.magenta,
        new(1, 0.5f, 0),
        new(0, 1, 0.5f)
    };

    public static Color GetColor(BuffType bt)
    {
        return buffColors[(int)bt];
    }
};

public class ShipControl : NetworkBehaviour
{
    private static string s_ObjectPoolTag = "ObjectPool";
    private NetworkObjectPool m_ObjectPool;

    public GameObject BulletPrefab;

    public AudioSource fireSound;
    private float m_RotateSpeed = 200f;
    private float m_Acceleration = 12f;
    private float m_BulletLifetime = 2;
    private float m_TopSpeed = 7.0f;

    public NetworkVariable<int> Health = new(100);

    public NetworkVariable<int> Energy = new(100);

    public NetworkVariable<float> SpeedBuffTimer = new(0f);

    public NetworkVariable<float> RotateBuffTimer = new(0f);

    public NetworkVariable<float> TripleShotTimer = new(0f);

    public NetworkVariable<float> DoubleShotTimer = new(0f);

    public NetworkVariable<float> QuadDamageTimer = new(0f);

    public NetworkVariable<float> BounceTimer = new(0f);

    public NetworkVariable<Color> LatestShipColor = new();
    private float m_EnergyTimer = 0;
    private bool m_IsBuffed;

    public NetworkVariable<FixedString32Bytes> PlayerName = new(new FixedString32Bytes(""));

    [SerializeField]
    private ParticleSystem m_Friction;

    [SerializeField]
    private ParticleSystem m_Thrust;

    [SerializeField]
    private SpriteRenderer m_ShipGlow;

    [SerializeField]
    private Color m_ShipGlowDefaultColor;

    [SerializeField]
    private UIDocument m_UIDocument;
    private VisualElement m_RootVisualElement;
    private ProgressBar m_HealthBar;
    private ProgressBar m_EnergyBar;
    private VisualElement m_PlayerUIWrapper;
    private TextElement m_PlayerName;
    private Camera m_MainCamera;
    private ParticleSystem.MainModule m_ThrustMain;

    private NetworkVariable<float> m_FrictionEffectStartTimer = new(-10);

    // for client movement command throttling
    private float m_OldMoveForce = 0;
    private float m_OldSpin = 0;

    // server movement
    private NetworkVariable<float> m_Thrusting = new();
    private float m_Spin;
    private Rigidbody2D m_Rigidbody2D;

    private NetworkVariable<string> uniqueId = new("");
    public string UniqueId => uniqueId.Value;

    private void Awake()
    {
        if (IsServer)
        {
            uniqueId.Value = Guid.NewGuid().ToString();
        }
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(
            m_ObjectPool,
            $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?"
        );

        m_ThrustMain = m_Thrust.main;
        m_ShipGlow.color = m_ShipGlowDefaultColor;
        m_IsBuffed = false;

        m_RootVisualElement = m_UIDocument.rootVisualElement;
        m_PlayerUIWrapper = m_RootVisualElement.Q<VisualElement>("PlayerUIWrapper");
        m_HealthBar = m_RootVisualElement.Q<ProgressBar>(name: "HealthBar");
        m_EnergyBar = m_RootVisualElement.Q<ProgressBar>(name: "EnergyBar");
        m_PlayerName = m_RootVisualElement.Q<TextElement>("PlayerName");
        m_MainCamera = Camera.main;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetPlayerUIVisibility(true);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            LatestShipColor.Value = m_ShipGlowDefaultColor;

            PlayerName.Value = $"Player {OwnerClientId}";

            if (!IsHost)
            {
                SetPlayerUIVisibility(false);
            }
        }
        Energy.OnValueChanged += OnEnergyChanged;
        Health.OnValueChanged += OnHealthChanged;
        OnEnergyChanged(0, Health.Value);
        OnHealthChanged(0, Energy.Value);

        SetPlayerName(PlayerName.Value.ToString().ToUpper());
    }

    public override void OnNetworkDespawn()
    {
        Energy.OnValueChanged -= OnEnergyChanged;
        Health.OnValueChanged -= OnHealthChanged;
    }

    private void OnEnergyChanged(int previousValue, int newValue)
    {
        SetEnergyBarValue(newValue);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        SetHealthBarValue(newValue);
    }

    public void TakeDamage(int amount, string source)
    {
        Health.Value -= amount;
        m_FrictionEffectStartTimer.Value = NetworkManager.LocalTime.TimeAsFloat;

        if (Health.Value <= 0)
        {
            SendDeathStatisticClientRpc(source);
            Health.Value = 0;

            //todo: reset all buffs

            Health.Value = 100;
            transform.position = NetworkManager
                .GetComponent<RandomPositionPlayerSpawner>()
                .GetNextSpawnPosition();
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
        }
    }

    [ClientRpc]
    private void SendDeathStatisticClientRpc(string uniqueId)
    {
        if (IsLocalPlayer)
        {
            StatisticCollector.Instance.PlayerStatisticDto.Deaths++;
        }

        if (
            FindObjectsByType<ShipControl>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .First(x => x.UniqueId == uniqueId)
                .IsLocalPlayer
        )
        {
            StatisticCollector.Instance.PlayerStatisticDto.Kills++;
        }
    }

    private void Fire(Vector3 direction)
    {
        fireSound.Play();

        int damage = 5;
        if (QuadDamageTimer.Value > NetworkManager.ServerTime.TimeAsFloat)
        {
            damage = 20;
        }

        bool bounce = BounceTimer.Value > NetworkManager.ServerTime.TimeAsFloat;

        GameObject bulletGo = m_ObjectPool.GetNetworkObject(BulletPrefab).gameObject;
        bulletGo.transform.position = transform.position + direction;

        Vector2 velocity = m_Rigidbody2D.velocity;
        velocity += (Vector2)direction * 10;
        bulletGo.GetComponent<NetworkObject>().Spawn(true);
        Bullet bullet = bulletGo.GetComponent<Bullet>();
        bullet.Config(this, damage, bounce, m_BulletLifetime);
        bullet.SetVelocity(velocity);
    }

    private void Update()
    {
        if (IsServer)
        {
            UpdateServer();
        }

        if (IsClient)
        {
            UpdateClient();
        }
    }

    private void LateUpdate()
    {
        if (IsLocalPlayer)
        {
            // center camera.. only if this is MY player!
            Vector3 pos = transform.position;
            pos.z = -50;
            m_MainCamera.transform.position = pos;
        }
        SetWrapperPosition();
    }

    private void UpdateServer()
    {
        // energy regen
        if (m_EnergyTimer < NetworkManager.ServerTime.TimeAsFloat)
        {
            if (Energy.Value < 100)
            {
                if (Energy.Value + 20 > 100)
                {
                    Energy.Value = 100;
                }
                else
                {
                    Energy.Value += 20;
                }
            }

            m_EnergyTimer = NetworkManager.ServerTime.TimeAsFloat + 1;
        }

        // update rotation
        float rotate = m_Spin * m_RotateSpeed;
        if (RotateBuffTimer.Value > NetworkManager.ServerTime.TimeAsFloat)
        {
            rotate *= 2;
        }

        m_Rigidbody2D.angularVelocity = rotate;

        // update thrust
        if (m_Thrusting.Value != 0)
        {
            float accel = m_Acceleration;
            if (SpeedBuffTimer.Value > NetworkManager.ServerTime.TimeAsFloat)
            {
                accel *= 2;
            }

            Vector3 thrustVec = transform.right * (m_Thrusting.Value * accel);
            m_Rigidbody2D.AddForce(thrustVec);

            // restrict max speed
            float top = m_TopSpeed;
            if (SpeedBuffTimer.Value > NetworkManager.ServerTime.TimeAsFloat)
            {
                top *= 1.5f;
            }

            if (m_Rigidbody2D.velocity.magnitude > top)
            {
                m_Rigidbody2D.velocity = m_Rigidbody2D.velocity.normalized * top;
            }
        }
    }

    private void HandleFrictionGraphics()
    {
        double time = NetworkManager.ServerTime.Time;
        float start = m_FrictionEffectStartTimer.Value;
        float duration = m_Friction.main.duration;

        bool frictionShouldBeActive = time >= start && time < start + duration; // 1f is the duration of the effect

        if (frictionShouldBeActive)
        {
            if (m_Friction.isPlaying == false)
            {
                m_Friction.Play();
            }
        }
        else
        {
            if (m_Friction.isPlaying)
            {
                m_Friction.Stop();
            }
        }
    }

    // changes color of the ship glow sprite and the trail effects based on the latest buff color
    private void HandleBuffColors()
    {
        m_ThrustMain.startColor = m_IsBuffed ? LatestShipColor.Value : m_ShipGlowDefaultColor;
        m_ShipGlow.material.color = m_IsBuffed ? LatestShipColor.Value : m_ShipGlowDefaultColor;
    }

    private void UpdateClient()
    {
        HandleFrictionGraphics();
        HandleIfBuffed();

        if (!IsLocalPlayer)
        {
            return;
        }

        // movement
        int spin = 0;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            spin += 1;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            spin -= 1;
        }

        int moveForce = 0;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            moveForce += 1;
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            moveForce -= 1;
        }

        if (m_OldMoveForce != moveForce || m_OldSpin != spin)
        {
            ThrustServerRpc(moveForce, spin);
            m_OldMoveForce = moveForce;
            m_OldSpin = spin;
        }

        // control thrust particles
        if (moveForce == 0.0f)
        {
            m_ThrustMain.startLifetime = 0.1f;
            m_ThrustMain.startSize = 1f;
            GetComponent<AudioSource>().Pause();
        }
        else
        {
            m_ThrustMain.startLifetime = 0.4f;
            m_ThrustMain.startSize = 1.2f;
            GetComponent<AudioSource>().Play();
        }

        // fire
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FireServerRpc();
        }
    }

    // a check to see if there's currently a buff applied, returns ship to default color if not
    private void HandleIfBuffed()
    {
        if (SpeedBuffTimer.Value > NetworkManager.ServerTime.Time)
        {
            m_IsBuffed = true;
        }
        else if (RotateBuffTimer.Value > NetworkManager.ServerTime.Time)
        {
            m_IsBuffed = true;
        }
        else if (TripleShotTimer.Value > NetworkManager.ServerTime.Time)
        {
            m_IsBuffed = true;
        }
        else if (DoubleShotTimer.Value > NetworkManager.ServerTime.Time)
        {
            m_IsBuffed = true;
        }
        else if (QuadDamageTimer.Value > NetworkManager.ServerTime.Time)
        {
            m_IsBuffed = true;
        }
        else if (BounceTimer.Value > NetworkManager.ServerTime.Time)
        {
            m_IsBuffed = true;
        }
        else
        {
            m_IsBuffed = false;
        }
        HandleBuffColors();
    }

    public void AddBuff(Buff.BuffType buff)
    {
        SendPowerUpPickupClientRpc();

        if (buff == Buff.BuffType.Speed)
        {
            SpeedBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 10;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Speed);
        }

        if (buff == Buff.BuffType.Rotate)
        {
            RotateBuffTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 10;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Rotate);
        }

        if (buff == Buff.BuffType.Triple)
        {
            TripleShotTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 10;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Triple);
        }

        if (buff == Buff.BuffType.Double)
        {
            DoubleShotTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 10;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Double);
        }

        if (buff == Buff.BuffType.Health)
        {
            Health.Value += 20;
            if (Health.Value >= 100)
            {
                Health.Value = 100;
            }
        }

        if (buff == Buff.BuffType.QuadDamage)
        {
            QuadDamageTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 10;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.QuadDamage);
        }

        if (buff == Buff.BuffType.Bounce)
        {
            QuadDamageTimer.Value = NetworkManager.ServerTime.TimeAsFloat + 10;
            LatestShipColor.Value = Buff.GetColor(Buff.BuffType.Bounce);
        }

        if (buff == Buff.BuffType.Energy)
        {
            Energy.Value += 50;
            if (Energy.Value >= 100)
            {
                Energy.Value = 100;
            }
        }
    }

    [ClientRpc]
    private void SendPowerUpPickupClientRpc()
    {
        if (IsLocalPlayer)
        {
            StatisticCollector.Instance.PlayerStatisticDto.Pickups++;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            return;
        }

        Asteroid asteroid = other.gameObject.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            TakeDamage(5, null);
        }
    }

    // --- ServerRPCs ---

    [ServerRpc]
    public void ThrustServerRpc(float thrusting, int spin)
    {
        m_Thrusting.Value = thrusting;
        m_Spin = spin;
    }

    [ServerRpc]
    public void FireServerRpc()
    {
        if (Energy.Value >= 10)
        {
            Vector3 right = transform.right;
            if (TripleShotTimer.Value > NetworkManager.ServerTime.TimeAsFloat)
            {
                Fire(Quaternion.Euler(0, 0, 20) * right);
                Fire(Quaternion.Euler(0, 0, -20) * right);
                Fire(right);
            }
            else if (DoubleShotTimer.Value > NetworkManager.ServerTime.TimeAsFloat)
            {
                Fire(Quaternion.Euler(0, 0, -10) * right);
                Fire(Quaternion.Euler(0, 0, 10) * right);
            }
            else
            {
                Fire(right);
            }

            Energy.Value -= 10;
            if (Energy.Value <= 0)
            {
                Energy.Value = 0;
            }
        }
    }

    [ServerRpc]
    public void SetNameServerRpc(string name)
    {
        PlayerName.Value = name;
    }

    private void SetWrapperPosition()
    {
        Vector2 screenPosition = RuntimePanelUtils.CameraTransformWorldToPanel(
            m_PlayerUIWrapper.panel,
            transform.position,
            m_MainCamera
        );
        m_PlayerUIWrapper.transform.position = screenPosition;
    }

    private void SetHealthBarValue(int healthBarValue)
    {
        m_HealthBar.value = healthBarValue;
    }

    private void SetEnergyBarValue(int resourceBarValue)
    {
        m_EnergyBar.value = resourceBarValue;
    }

    private void SetPlayerName(string playerName)
    {
        m_PlayerName.text = playerName;
    }

    private void SetPlayerUIVisibility(bool visible)
    {
        m_RootVisualElement.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
