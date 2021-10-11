//using System;
//using System.Collections;
//using UnityEngine;

//namespace MoreDefenses.Scripts
//{
//    public class CustomGuardstone : MonoBehaviour
//    {
//        [SerializeField] internal PrivateArea _area;
//        [SerializeField] internal SphereCollider testcollider;

//        internal Collider _collider;
//        internal Transform strikelocation;
//        private Humanoid hum;
//        private HitData hitdata;

//        private void OnEnable()
//        {
//            _area = gameObject.GetComponentInParent<PrivateArea>();
//            InvokeRepeating(nameof(CustomArea), 0f, 1);
//            if (CustomGuardStone.m_radius.Value > 0)
//            {
//                _area.m_radius = CustomGuardStone.m_radius.Value;
//            }
//        }

//        private void LateUpdate()
//        {
//            if (_area.m_radius != CustomGuardStone.m_radius.Value && CustomGuardStone.m_radius.Value > 0)
//            {
//                _area.m_radius = CustomGuardStone.m_radius.Value;
//            }
//        }

//        internal void CustomArea()
//        {
//            if (!_area.IsEnabled()) return;
//            Collider[] hitcolliders = Physics.OverlapSphere(transform.position, _area.m_radius);
//            foreach (var hitcollider in hitcolliders)
//            {
//                if (hitcollider.gameObject.GetComponent<MonsterAI>() != null)
//                {
//                    if (hitcollider.gameObject.GetComponent<Humanoid>().m_tamed == true) return;
//                    try
//                    {
//                        var tame = hitcollider.GetComponent<Tameable>();
//                        if (tame != null)
//                            if (tame.GetTameness() > 0) return;
//                    }
//                    catch (Exception e)
//                    {
//                        Jotunn.Logger.LogInfo(e);
//                    }

//                    var tmp = hitcollider.gameObject;
//                    _collider = hitcollider;
//                    hum = tmp.GetComponent<Humanoid>();
//                    hitdata = new HitData
//                    {
//                        m_attacker = Player.m_localPlayer.GetZDOID(),
//                        m_blockable = false,
//                        m_damage = new HitData.DamageTypes
//                        {
//                            m_blunt = CustomGuardStone.m_blunt.Value,
//                            m_chop = CustomGuardStone.m_chop.Value,
//                            m_lightning = CustomGuardStone.m_lightning.Value,
//                            m_damage = CustomGuardStone.m_damage.Value,
//                            m_fire = CustomGuardStone.m_fire.Value,
//                            m_frost = CustomGuardStone.m_frost.Value,
//                            m_pickaxe = CustomGuardStone.m_pickaxe.Value,
//                            m_pierce = CustomGuardStone.m_pierce.Value,
//                            m_poison = CustomGuardStone.m_poison.Value,
//                            m_slash = CustomGuardStone.m_slash.Value,
//                            m_spirit = CustomGuardStone.m_spirit.Value
//                        },
//                        m_dir = Vector3.zero,
//                        m_dodgeable = false,
//                        m_ranged = true,
//                        m_skill = Skills.SkillType.All,
//                        m_backstabBonus = CustomGuardStone.m_backstabBonus.Value,
//                        m_hitCollider = _collider,
//                        m_pushForce = CustomGuardStone.m_pushForce.Value,
//                        m_staggerMultiplier = CustomGuardStone.m_staggerMultiplier.Value,
//                        m_toolTier = 10,
//                        m_statusEffect = "",
//                        m_point = Vector3.zero
//                    };
//                    strikelocation = hum.transform;
//                    StartCoroutine(LightningStrike());
//                }
//            }
//        }

//        internal IEnumerator LightningStrike()
//        {
//            ZNetScene.Instantiate(CustomGuardStone.VFX2, strikelocation, false);
//            hum.ApplyDamage(hitdata, true, triggerEffects: false, HitData.DamageModifier.Weak);
//            yield return new WaitForSeconds(CustomGuardStone.YieldTime.Value);
//        }

//    }
//}