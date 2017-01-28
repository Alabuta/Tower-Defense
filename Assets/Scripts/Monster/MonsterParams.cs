using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterParams", menuName = "New Type Of Monster", order = 1)]
public sealed class MonsterParams : ScriptableObject {

    [Header("Health Settings"), Range(1, 100)]
    public int health;

    [Header("Size Settings"), Range(.1f, 1f)]
    public float radius;

    [Header("Speed Settings"), Range(1, 10)]
    public float speed;

    [Range(1, 100)]
    public float acceleration;

    [Range(1, 1000)]
    public float angularSpeed;

    [Header("Attack Settings"), Range(1, 100)]
    public int damage;

    [Range(1, 2)]
    public float attackRadius;

    [Header("Other Settings"), Range(0, 1)]
    public float chanceToSpawn;

    [Range(1, 100)]
    public int rewardForKilling;

    [Range(1, 99)]
    public int avoidancePriority;
}