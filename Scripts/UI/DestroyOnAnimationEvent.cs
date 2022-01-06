using UnityEngine;

public class DestroyOnAnimationEvent : MonoBehaviour
{
    [SerializeField] Transform parentToDestroy;
    public void Destroy()
    {
        Destroy(parentToDestroy.gameObject);
    }
}
