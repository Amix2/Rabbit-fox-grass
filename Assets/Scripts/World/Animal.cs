using UnityEngine;
using UnityEditor;

public class Animal : MonoBehaviour
{
    Vector3 velocity = Vector3.zero;
    public Vector2Int worldSize;

    public void UpdateBehaviour(IBigBrain brain)
    {
        float[] inputs = null;
        var colliders = Physics.OverlapSphere(transform.position, Settings.Player.animalViewRange);
        int num = 0;
        foreach(var collider in colliders)
        {
            if (collider.gameObject.transform == transform || collider.gameObject.transform.parent != transform.parent) continue;

            num++;
        }
        print(num);
        velocity = brain.GetDecision(inputs);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, Settings.Player.animalViewRange);
    }

    private void Update()
    {
        var newPosition = transform.localPosition + (velocity * Time.deltaTime);
        newPosition.x = Mathf.Clamp(newPosition.x, 0.5f, worldSize.x - 0.5f);
        newPosition.z = Mathf.Clamp(newPosition.z, 0.5f, worldSize.y - 0.5f);
        if ((newPosition - transform.localPosition).sqrMagnitude > 0) transform.forward = (newPosition - transform.localPosition).normalized;
        transform.localPosition = newPosition;
        Debug.DrawRay(transform.position, velocity);
    }

}