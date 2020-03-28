using UnityEngine;

public class Animal : MonoBehaviour
{
    private Vector3 velocity = Vector3.zero;
    public Vector2Int worldSize;

    private float lastUpdateTime;

    public void UpdateBehaviour(IBigBrain brain)
    {
        float[] inputs = null;
        var colliders = Physics.OverlapSphere(transform.position, Settings.Player.animalViewRange);
        int num = 0;
        foreach (var collider in colliders)
        {
            if (collider.gameObject.transform == transform || collider.gameObject.transform.parent != transform.parent) continue;

            num++;
        }
        Vector3 decision = brain.GetDecision(inputs);
        if (Settings.Player.fastTrainingMode)
        {
            velocity = decision * Time.fixedDeltaTime / (Time.time - lastUpdateTime);
            lastUpdateTime = Time.time;
        }
        else
        {
            velocity = decision;
        }
    }

    private void Start()
    {
        lastUpdateTime = -Time.fixedDeltaTime;
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

        velocity = (newPosition - transform.localPosition) / Time.deltaTime;
        if (velocity.sqrMagnitude > 0) transform.forward = velocity.normalized;

        transform.localPosition = newPosition;
    }
}