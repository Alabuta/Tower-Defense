using UnityEngine;

[CreateAssetMenu(fileName = "NewTowerParams", menuName = "New Type Of Tower", order = 2)]
public sealed class TowerParams : ScriptableObject {

    [Header("Attack Settings"), Range(1, 100)]
    public int hitDamage;

    [Range(.1f, 10f)]
    public float attackRadius;

    [Range(.01f, 10f)]
    public float fireRate;

    [Range(.1f, 1.0f)]
    public float projectileContactRadius;

    [Header("Other Settings"), Range(1, 500)]
    public int price;
}