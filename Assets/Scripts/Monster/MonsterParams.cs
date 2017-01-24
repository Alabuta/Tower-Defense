using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterParams", menuName = "New Type Of Monster", order = 1)]
public class MonsterParams : ScriptableObject {

    [Header("Health Settings"), SerializeField, Range(1, 100)]
    public int health;

    [Header("Size Settings"), SerializeField, Range(0.1f, 1.0f)]
    public float radius;

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

    [SerializeField, Range(1, 99)]
    public int avoidancePriority;
}