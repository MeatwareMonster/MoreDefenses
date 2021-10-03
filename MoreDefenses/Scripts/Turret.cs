using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    private float Range = 100f;
    private float TargetUpdateInterval = 5f;
    private float ShootInterval = 5f;
    private HitData.DamageTypes Damage = new HitData.DamageTypes
    {
        m_damage = 5f
    };

    private readonly int ViewBlockMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain", "viewblock", "vehicle");
    private Character Target;
    private float UpdateTargetTimer;
    private float ShootTimer;

    private AudioSource AudioSource;
    private ParticleSystem ParticleSystem;
    private Bounds Bounds;

    private ZNetView m_nview;

    private LineRenderer LineRenderer;

    private void Awake()
    {
        LineRenderer = gameObject.AddComponent<LineRenderer>();
        LineRenderer.startWidth = 0.1f;
        LineRenderer.endWidth = 0.1f;

        AudioSource = GetComponent<AudioSource>();
        ParticleSystem = GetComponentInChildren<ParticleSystem>();
        Bounds = GetComponent<BoxCollider>().bounds;

        m_nview = GetComponent<ZNetView>();
        m_nview.Register("Fire", RPC_Fire);
    }

    private void Update()
    {
        if (!m_nview.IsOwner())
        {
            return;
        }

        if (Target == null)
        {
            if (UpdateTargetTimer < 0)
            {
                FindTarget();
                UpdateTargetTimer = TargetUpdateInterval;
            }
        }

        if (Target != null)
        {
            if (Target.IsDead())
            {
                Target = null;
            }
            else
            {
                LineRenderer.SetPosition(0, Bounds.center);
                LineRenderer.SetPosition(1, Target.GetCenterPoint());
                transform.LookAt(Target.transform);

                if (ShootTimer < 0)
                {
                    if (!CanSeeCharacter(Target))
                    {
                        Target = null;
                        Jotunn.Logger.LogDebug("Target lost");
                    }
                    else
                    {
                        Jotunn.Logger.LogDebug("Shoot");
                        AudioSource.Play();
                        ParticleSystem.Play();
                        Target.Damage(new HitData
                        {
                            m_damage = Damage
                        });
                    }

                    ShootTimer = ShootInterval;
                }
            }
        }

        UpdateTargetTimer -= Time.deltaTime;
        ShootTimer -= Time.deltaTime;
    }

    private void FindTarget()
    {
        List<Character> allCharacters = Character.GetAllCharacters();
        foreach (Character character in allCharacters)
        {
            if (character.m_faction == Character.Faction.Players && !character.IsDead() && IsCharacterInRange(character) && CanSeeCharacter(character))
            {
                Jotunn.Logger.LogDebug($"Target changed to {character.m_name}");
                Target = character;
                return;
            }
        }
    }

    private bool IsCharacterInRange(Character character)
    {
        return Vector3.Distance(character.transform.position, transform.position) <= Range;
    }

    private bool CanSeeCharacter(Character character)
    {
        var vector = character.GetCenterPoint() - Bounds.center;
        return !Physics.Raycast(Bounds.center, vector.normalized, vector.magnitude, ViewBlockMask);
    }

    private void RPC_Fire(long sender)
    {

    }
}
