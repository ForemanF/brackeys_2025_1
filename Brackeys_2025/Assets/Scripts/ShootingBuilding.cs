using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingBuilding : Building
{
    [SerializeField]
    int range = 2;

    [SerializeField]
    int strength = 1;

    [SerializeField]
    GameObject bullet_prefab;

    GameObject bullet_obj = null;

    [SerializeField]
    Vector3 vert_offset;

    public void SetStrength(int _strength) {
        strength = _strength;
    }

    public void SetRange(int _range) {
        range = _range;
    }

    public override IEnumerator ProcessBuildingAttack(HexTile nearest_enemy) {
        float distance = Utilities.CubeDistance(nearest_enemy.GetCubeCoords(), current_tile.GetCubeCoords());

        if(distance <= range) { 
            Debug.Log("Fire");
            if(bullet_obj == null) { 
                bullet_obj = Instantiate(bullet_prefab, transform.position, Quaternion.identity);
            }
            bullet_obj.SetActive(true);

            bullet_obj.transform.position = transform.position + vert_offset;

            GameObject enemy_obj = nearest_enemy.GetObjectOnHex();

            yield return LerpUtilities.LerpToPosition(bullet_obj.transform, enemy_obj.transform.position, distance / 2);

            enemy_obj.GetComponent<HasHealth>().TakeDamage(strength);

            bullet_obj.SetActive(false);
            Debug.Log("Done");
        }

        yield return null;
    }
}
