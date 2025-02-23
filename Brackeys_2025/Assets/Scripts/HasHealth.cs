using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HasHealth : MonoBehaviour
{
    [SerializeField]
    int health = 3;

    public void SetHealth(int _health) {
        health = _health;
    }

    public int GetHealth() {
        return health;
    }

    public void TakeDamage(int amount) {
        health -= amount;

        if(health <= 0) {
            StartCoroutine(PlayDeathAffect());
            Debug.Log("Now has no health");
            return;
        }

        PublishBurst(10);
    }

    IEnumerator PlayDeathAffect() {
        PublishBurst(30);

        Faction my_faction = GetComponent<HasFaction>().GetFaction();
        EventBus.Publish(new DestroyedFactionObjectEvent(my_faction, gameObject));

        yield return null;

        Destroy(gameObject);
    }

    void PublishBurst(int emit_amt) {

        Faction my_faction = GetComponent<HasFaction>().GetFaction();

        BurstType burst_type;
        if(my_faction == Faction.Player) {
            burst_type = BurstType.PlayerDamage;
        }
        else { 
            burst_type = BurstType.EnemyDamage;
        }

        EventBus.Publish(new ParticleBurstEvent(transform.position, burst_type, emit_amt));
    }

    public void Heal(int amount) {
        health += amount;
    }
}
