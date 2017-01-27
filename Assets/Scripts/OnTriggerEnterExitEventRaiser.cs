using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class OnTriggerEnterExitEventRaiser : MonoBehaviour {

    public OnTriggerEnterExitEvent onEnter = new OnTriggerEnterExitEvent();
    public OnTriggerEnterExitEvent onExit = new OnTriggerEnterExitEvent();

    public class OnTriggerEnterExitEvent : UnityEvent<GameObject, Collider> {
    }

    void Start()
    {
        if (GetComponent<Collider>() == null) {
            Debug.LogError("GameObject has no collider.", transform);
            Application.Quit();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        onEnter.Invoke(gameObject, other);
    }

    void OnTriggerExit(Collider other)
    {
        onExit.Invoke(gameObject, other);
    }
}