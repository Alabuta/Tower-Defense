using UnityEngine;

public abstract class MonsterData : ScriptableObject {

    [Header("Health Settings"), SerializeField, Range(1, 100)]
    public int health;

    [Header("Speed Settings"), SerializeField, Range(1, 10)]
    public float speed;

    [SerializeField, Range(1, 10)]
    public float acceleration;

    [SerializeField, Range(1, 100)]
    public float angularSpeed;

    [Header("Attack Settings"), SerializeField, Range(1, 10)]
    public int damage;

    [SerializeField, Range(1, 2)]
    public float attackDistance;

    [Header("Other Settings"), SerializeField, Range(0, 1)]
    public float chanceToSpawn;

    [SerializeField, Range(1, 100)]
    public int rewardForKilling;
}