using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIElementExtender<T> : MonoBehaviour where T : Component
    {
        protected T elementOfExtension;
        public virtual void Awake()
        {

            elementOfExtension = transform.root.GetComponentInChildren(typeof(T)) as T;

        }
    }
}