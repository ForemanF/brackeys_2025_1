using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BurstType { 
    PlayerDamage,
    EnemyDamage,
    BuildHeal,
    RevealFog
}

[System.Serializable] public class BurstPs { public BurstType burst_type; public ParticleSystem ps; }

public class ParticleManager : MonoBehaviour
{

    Subscription<ParticleBurstEvent> burst_sub;

    [SerializeField]
    List<BurstPs> burst_ps_list;

    Dictionary<BurstType, ParticleSystem> burst_ps_dict;

    // Start is called before the first frame update
    void Start()
    {
        EventBus.Subscribe<ParticleBurstEvent>(_OnBurstEvent);

        burst_ps_dict = new Dictionary<BurstType, ParticleSystem>();
        foreach(BurstPs burst_ps in burst_ps_list) {
            burst_ps_dict[burst_ps.burst_type] = burst_ps.ps;
        }
    }

    void _OnBurstEvent(ParticleBurstEvent e) {
        ParticleSystem ps = burst_ps_dict[e.burst_type];
        ps.transform.position = e.position;
        ps.Emit(e.emit_amt);
    }
}
