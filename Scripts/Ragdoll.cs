using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] Rigidbody mainRB;

    public Rigidbody MainRB { get => mainRB; set => mainRB = value; }
}
