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
        velocity = brain.GetDecision(inputs);
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireSphere(transform.position, Settings.Player.animalViewRange);
    }

    private void Update()
    { 
        if (transform.localPosition.x <= 0 || transform.localPosition.x >= worldSize.x-1) velocity.x = 0;
        if (transform.localPosition.z <= 0 || transform.localPosition.z >= worldSize.y-1) velocity.z = 0;
        if (velocity.sqrMagnitude > 0) transform.forward = velocity.normalized;
        transform.Translate(velocity * Time.deltaTime);
    }

}