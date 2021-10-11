using System;
using System.Collections;
using System.Collections.Generic;
using MoreDefenses;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public float Range = 20f;
    public float FireInterval = 0.5f;
    public float Damage = 25f;

    private readonly float m_targetUpdateInterval = 0.5f;

    private readonly int m_viewBlockMask = LayerMask.GetMask("Default", "static_solid", "Default_small", "piece", "terrain", "viewblock", "vehicle");
    private Character m_target;
    private float m_updateTargetTimer;
    private float m_shootTimer;

    private AudioSource m_audioSource;
    private ParticleSystem m_particleSystem;
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

        m_particleSystem = GetComponentInChildren<ParticleSystem>();
        m_bounds = GetComponent<BoxCollider>().bounds;

        m_nview = GetComponent<ZNetView>();
        m_nview.Register("Fire", RPC_Fire);
    }

    private void SetVolume(object sender, EventArgs e)
    {
        SetVolume();
    }

    private void SetVolume()
    {
        //Jotunn.Logger.LogDebug(Mod.TurretVolume.Value);
        m_audioSource.volume = Mod.TurretVolume.Value * 0.0025f;
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
            if (m_updateTargetTimer < 0)
            {
                StartCoroutine(FindTarget());
                m_updateTargetTimer = m_targetUpdateInterval;
            }
        }

        if (m_target != null)
        {
            if (m_target.IsDead())
            {
                m_target = null;
            }
            else
            {
                // Debug targeting
                //m_lineRenderer.SetPosition(0, Bounds.center);
                //m_lineRenderer.SetPosition(1, Target.GetCenterPoint());

                transform.LookAt(m_target.transform);

                if (m_shootTimer < 0)
                {
                    if (!IsCharacterInRange(m_target) || !CanSeeCharacter(m_target))
                    {
                        m_target = null;
                        //Jotunn.Logger.LogDebug("Target lost");
                    }
                    else
                    {
                        //Jotunn.Logger.LogDebug("Fire");
                        m_nview.InvokeRPC(ZNetView.Everybody, "Fire");
                        m_target.Damage(new HitData
                        {
                            m_damage = new HitData.DamageTypes
                            {
                                m_damage = Damage
                            }
                        });
                    }

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
            if (character.m_faction != Character.Faction.Players && !character.IsTamed() && !character.IsDead() && IsCharacterInRange(character) && CanSeeCharacter(character))
            {
                //Jotunn.Logger.LogDebug($"Target changed to {character.m_name}");
                m_target = character;
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

    private void RPC_Fire(long sender)
    {
        m_audioSource.Play();
        m_particleSystem.Play();
    }

    public void SetVolume(float volume)
    {
        m_audioSource.volume = volume / 100 * 0.25f;
    }
}
