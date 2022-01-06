using UnityEngine;

namespace Assets.Scripts.PlayerScript
{
    public class DeathRagdollManager : MonoBehaviour
    {
        [SerializeField] GameObject ragdoll;
        private void OnEnable()
        {
            GetComponent<PlayerHpController>().onPlayerDeathEvent += spawnRagdoll;
        }

        private void spawnRagdoll()
        {
            GameObject go = Instantiate(ragdoll, transform.position, transform.rotation);
            go.GetComponent<Ragdoll>().MainRB.velocity = GetComponent<CharacterController>().velocity;
            Destroy(go, 15f);

        }
    }
}