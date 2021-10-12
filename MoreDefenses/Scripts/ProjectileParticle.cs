using System.Collections.Generic;
using UnityEngine;

public class ProjectileParticle : MonoBehaviour
{
    private ParticleSystem m_particleSystem;
    private List<ParticleCollisionEvent> m_particleCollisionEvents = new List<ParticleCollisionEvent>();

    private void Start()
    {
        m_particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnParticleCollision(GameObject other)
    {
        var events = m_particleSystem.GetCollisionEvents(other, m_particleCollisionEvents);

        for (var i = 0; i < events; i++)
        {

        }
    }
}
