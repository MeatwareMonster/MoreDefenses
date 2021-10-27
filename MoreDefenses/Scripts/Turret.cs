using System;
using System.Collections;
using System.Collections.Generic;
using MoreDefenses;
using MoreDefenses.Models;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float Range = 20f;
    public float FireInterval = 0.5f;
    public float Damage = 0f;
    public float PierceDamage = 0f;
    public float FireDamage = 0f;
    public float FrostDamage = 0f;
    public float LightningDamage = 0f;
    public float PoisonDamage = 0f;
    public float SpiritDamage = 0f;
    public float DamageRadius = 0f;
    public bool CanShootFlying = true;
    public bool IsContinuous = false;
    public TurretType Type = TurretType.Gun;
    public enum TurretType
    {
        Gun,
        Cannon,
        Projectile,
        Flamethrower
    }

    private HitData m_hitData;

    private readonly float m_targetUpdateInterval = 0.5f;

    private readonly int m_viewBlockMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain", "viewblock", "vehicle");
    private readonly int m_rayMaskSolids = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "piece_nonsolid", "terrain", "character", "character_net", "character_ghost", "hitbox", "character_noenv", "vehicle");
    private Character m_target;
    private float m_updateTargetTimer;
    private float m_shootTimer;
    private bool m_isFiring;

    private AudioSource m_audioSource;
    private ParticleSystem m_outputParticleSystem;
    private ParticleSystem m_impactParticleSystem;
    private ParticleSystem m_projectileParticleSystem;
    private Bounds m_bounds;

    private ZNetView m_nview;

    // Debug targeting
    //private LineRenderer m_lineRenderer;

    private void Awake()
    {
        // Debug targeting
        //m_lineRenderer = gameObject.AddComponent<LineRenderer>();
        //m_lineRenderer.startWidth = 0.1f;
        //m_lineRenderer.endWidth = 0.1f;

        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.outputAudioMixerGroup = AudioMan.instance.m_ambientMixer;
        SetVolume();
        Mod.TurretVolume.SettingChanged += SetVolume;


        m_bounds = GetComponent<BoxCollider>().bounds;

        m_nview = GetComponent<ZNetView>();
        m_nview.Register<Vector3>("Fire", RPC_Fire);
        m_nview.Register("StopFire", RPC_StopFire);

        m_hitData = new HitData
        {
            m_damage = new HitData.DamageTypes
            {
                m_damage = Damage,
                m_pierce = PierceDamage,
                m_fire = FireDamage,
                m_frost = FrostDamage,
                m_lightning = LightningDamage,
                m_poison = PoisonDamage,
                m_spirit = SpiritDamage
            }
        };

        m_outputParticleSystem = transform.Find("OutputParticleSystem")?.GetComponent<ParticleSystem>();
        if (m_outputParticleSystem == null) m_outputParticleSystem = transform.Find("Particle System")?.GetComponent<ParticleSystem>();

        m_impactParticleSystem = transform.Find("ImpactParticleSystem")?.GetComponent<ParticleSystem>();

        m_projectileParticleSystem = transform.Find("ProjectileParticleSystem")?.GetComponent<ParticleSystem>();
        if (m_projectileParticleSystem != null)
        {
            var projectileParticle = m_projectileParticleSystem.gameObject.AddComponent<ProjectileParticle>();
            projectileParticle.HitData = m_hitData;
            projectileParticle.SourceTurret = this;
        }
    }

    public void Initialize(TurretConfig turretConfig)
    {
        Type = (TurretType)Enum.Parse(typeof(TurretType), turretConfig.type, true);
        Range = turretConfig.range;
        Damage = turretConfig.damage;
        PierceDamage = turretConfig.pierceDamage;
        FireDamage = turretConfig.fireDamage;
        FrostDamage = turretConfig.frostDamage;
        LightningDamage = turretConfig.lightningDamage;
        PoisonDamage = turretConfig.poisonDamage;
        SpiritDamage = turretConfig.spiritDamage;
        FireInterval = turretConfig.fireInterval;
        DamageRadius = turretConfig.damageRadius;
        CanShootFlying = turretConfig.canShootFlying;
        IsContinuous = turretConfig.isContinuous;
    }

    public bool IsOwner()
    {
        if (m_nview == null || !m_nview.IsOwner())
        {
            return false;
        }

        return true;
    }

    private void SetVolume(object sender, EventArgs e)
    {
        SetVolume();
    }

    private void SetVolume()
    {
        //Jotunn.Logger.LogDebug(Mod.TurretVolume.Value);
        m_audioSource.volume = Mod.TurretVolume.Value * 0.005f;
    }

    private void OnDestroy()
    {
        Mod.TurretVolume.SettingChanged -= SetVolume;
    }

    private void Update()
    {
        if (m_nview == null || !m_nview.IsOwner())
        {
            return;
        }

        if (m_target == null)
        {
            if (IsContinuous && m_isFiring) m_nview.InvokeRPC(ZNetView.Everybody, "StopFire");
            if (m_updateTargetTimer < 0)
            {
                StartCoroutine(FindTarget());
                m_updateTargetTimer = m_targetUpdateInterval;
            }
        }

        if (m_target != null)
        {
            if (m_target.IsDead() || !IsCharacterInRange(m_target) || !CanSeeCharacter(m_target))
            {
                m_target = null;
                if (IsContinuous) m_nview.InvokeRPC(ZNetView.Everybody, "StopFire");
                //Jotunn.Logger.LogDebug("Target lost");
            }
            else
            {
                transform.LookAt(m_target.transform);

                if (m_shootTimer < 0)
                {
                    if (!IsContinuous) m_nview.InvokeRPC(ZNetView.Everybody, "Fire", m_target.transform.position);
                    switch (Type)
                    {
                        case TurretType.Gun:
                            m_target.Damage(m_hitData);
                            break;
                        case TurretType.Cannon:
                            DamageAreaTargets(m_target.transform.position);
                            break;
                        case TurretType.Flamethrower:
                            DamageFlamethrowerTargets();
                            break;
                        default:
                            break;
                    }

                    // Debug targeting
                    //m_lineRenderer.SetPosition(0, Bounds.center);
                    //m_lineRenderer.SetPosition(1, Target.GetCenterPoint());

                    //Jotunn.Logger.LogDebug("Fire");

                    m_shootTimer = FireInterval;
                }
            }
        }

        m_updateTargetTimer -= Time.deltaTime;
        m_shootTimer -= Time.deltaTime;
    }

    private IEnumerator FindTarget()
    {
        List<Character> allCharacters = Character.GetAllCharacters();
        foreach (Character character in allCharacters)
        {
            if (character.m_faction != Character.Faction.Players
                && (CanShootFlying || !character.IsFlying())
                && !character.IsTamed()
                && !character.IsDead()
                && IsCharacterInRange(character)
                && CanSeeCharacter(character))
            {
                //Jotunn.Logger.LogDebug($"Target changed to {character.m_name}");
                m_target = character;
                if (IsContinuous) m_nview.InvokeRPC(ZNetView.Everybody, "Fire", m_target.transform.position);
                yield break;
            }
        }
    }

    private bool IsCharacterInRange(Character character)
    {
        return Vector3.Distance(character.transform.position, transform.position) <= Range;
    }

    private bool CanSeeCharacter(Character character)
    {
        var vector = character.GetCenterPoint() - m_bounds.center;
        return !Physics.Raycast(m_bounds.center, vector.normalized, vector.magnitude, m_viewBlockMask);
    }

    private void RPC_Fire(long sender, Vector3 impactPosition)
    {
        m_audioSource.Play();
        if (m_outputParticleSystem != null) m_outputParticleSystem.Play();
        if (m_projectileParticleSystem != null) m_projectileParticleSystem.Play();
        if (m_impactParticleSystem != null)
        {
            m_impactParticleSystem.transform.position = impactPosition;
            m_impactParticleSystem.Play();
        }

        if (IsContinuous)
        {
            m_isFiring = true;
        }
    }

    private void RPC_StopFire(long sender)
    {
        m_audioSource.Stop();
        if (m_outputParticleSystem != null && !m_outputParticleSystem.isStopped) m_outputParticleSystem.Stop();
        if (m_projectileParticleSystem != null && !m_projectileParticleSystem.isStopped) m_projectileParticleSystem.Stop();

        m_isFiring = false;
    }

    public void SetVolume(float volume)
    {
        m_audioSource.volume = volume / 100;
    }

    private void DamageAreaTargets(Vector3 position)
    {
        var hits = Physics.OverlapSphere(position, DamageRadius, m_rayMaskSolids);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Character character) && character.m_faction != Character.Faction.Players && !character.IsTamed() && !character.IsDead())
            {
                character.Damage(m_hitData);
            }
        }
    }

    private void DamageFlamethrowerTargets()
    {
        var hits = Physics.OverlapCapsule(transform.position, transform.position + transform.forward * Range, 1, m_rayMaskSolids);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out Character character) && character.m_faction != Character.Faction.Players && !character.IsTamed() && !character.IsDead())
            {
                character.Damage(m_hitData);
            }
        }
    }
}
