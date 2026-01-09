using UnityEngine;

public class SpikeTrapAutomatick : MonoBehaviour
{
    [SerializeField] int damage = 10;


    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Player":
                CharacterControl control = other.GetComponent<CharacterControl>();
                control.TakeDamage(damage);
                break;

            case "Enemy":
                EnemyAI enemy = other.GetComponent<EnemyAI>();
                enemy.TakeEnemyDamage(damage);
                break;
        }
    }
}
