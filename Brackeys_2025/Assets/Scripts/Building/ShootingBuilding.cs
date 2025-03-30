using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingBuilding : Building
{

    [SerializeField]
    GameObject bullet_prefab;

    GameObject bullet_obj = null;

    [SerializeField]
    Vector3 vert_offset;

    public override IEnumerator ProcessBuildingAttack(HexTile nearest_enemy)
    {
        if(nearest_enemy == null) {
            yield break;
        }

        float distance = Utilities.CubeDistance(nearest_enemy.GetCubeCoords(), current_tile.GetCubeCoords());

        if (distance <= special_value)
        {
            if (bullet_obj == null)
            {
                bullet_obj = Instantiate(bullet_prefab, transform.position, Quaternion.identity);
            }
            bullet_obj.SetActive(true);

            EventBus.Publish(new AudioEvent(AudioType.Shoot, current_tile.transform.position));

            bullet_obj.transform.position = transform.position + vert_offset;

            GameObject enemy_obj = nearest_enemy.GetObjectOnHex();

            yield return LerpUtilities.LerpToPosition(bullet_obj.transform, enemy_obj.transform.position, distance / 2);

            enemy_obj.GetComponent<HasHealth>().TakeDamage(strength);

            bullet_obj.SetActive(false);
        }

        yield return null;
    }
}

