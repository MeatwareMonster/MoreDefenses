using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float Range = 20f;
    public float FireInterval = 0.5f;
    public float Damage = 25f;

    private readonly float TargetUpdateInterval = 0.5f;

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
                StartCoroutine(FindTarget());
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
                    if (!IsCharacterInRange(Target) || !CanSeeCharacter(Target))
                    {
                        Target = null;
                        Jotunn.Logger.LogDebug("Target lost");
                    }
                    else
                    {
                        Jotunn.Logger.LogDebug("Fire");
                        m_nview.InvokeRPC(ZNetView.Everybody, "Fire");
                        Target.Damage(new HitData
                        {
                            m_damage = new HitData.DamageTypes
                            {
                                m_damage = Damage
                            }
                        });
                    }

                    ShootTimer = FireInterval;
                }
            }
        }

        UpdateTargetTimer -= Time.deltaTime;
        ShootTimer -= Time.deltaTime;
    }

    private IEnumerator FindTarget()
    {
        List<Character> allCharacters = Character.GetAllCharacters();
        foreach (Character character in allCharacters)
        {
            if (character.m_faction != Character.Faction.Players && !character.IsDead() && IsCharacterInRange(character) && CanSeeCharacter(character))
            {
                Jotunn.Logger.LogDebug($"Target changed to {character.m_name}");
                Target = character;
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
        var vector = character.GetCenterPoint() - Bounds.center;
        return !Physics.Raycast(Bounds.center, vector.normalized, vector.magnitude, ViewBlockMask);
    }

    private void RPC_Fire(long sender)
    {
        AudioSource.Play();
        ParticleSystem.Play();
    }
}
