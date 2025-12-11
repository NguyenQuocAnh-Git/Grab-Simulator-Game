using UnityEngine;

public class TagAutoRegister : MonoBehaviour
{
    [SerializeField] private string customTag;

    void OnEnable() => TagRegistry.Register(customTag, gameObject);
    void OnDisable() => TagRegistry.Unregister(customTag, gameObject);
}

