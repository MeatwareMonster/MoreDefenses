using System.Collections.Generic;
using UnityEngine;

public class ProjectileParticle : MonoBehaviour
{
    public HitData HitData;

    private ParticleSystem m_particleSystem;
    private List<ParticleCollisionEvent> m_particleCollisionEvents = new List<ParticleCollisionEvent>();

    private void Start()
    {
        m_particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        var events = m_particleSystem.GetCollisionEvents(other, m_particleCollisionEvents);

        //for (var i = 0; i < events; i++)
        //{

        //}

        if (other.TryGetComponent(out Character character) && character.IsOwner() && character.m_faction != Character.Faction.Players && !character.IsTamed() && !character.IsDead())
        {
            character.Damage(HitData);
        }
    }
}
