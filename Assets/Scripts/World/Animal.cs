using UnityEngine;

public class Animal : MonoBehaviour
{
    public IBigBrain Brain
    {
        get => _brain;
        set => _brain = value;
    }

    public float Health => _health;
    public int Score => _score;
    public bool IsAlive => _health > 0;

    public Vector2Int worldSize;
    private IBigBrain _brain;
    private float _health = 1.0f;
    private int _score = 0;
    private Vector3 velocity = Vector3.zero;
    private float lastUpdateTime;


    public void UpdateBehaviour()
    {
        var colliders = Physics.OverlapSphere(transform.position, Settings.Player.animalViewRange);
        int num = 0;
        foreach (var collider in colliders)
        {
            if (collider.gameObject.transform == transform ||
                collider.gameObject.transform.parent != transform.parent) continue;

            num++;
        }

        var inputs = new[] {transform.localPosition.x / worldSize.x, transform.localPosition.z / worldSize.y};
        Vector3 decision = _brain.GetDecision(inputs);

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
        if (!IsAlive) return;

        var newPosition = transform.localPosition + (velocity * Time.deltaTime);
        newPosition.x = Mathf.Clamp(newPosition.x, 0.5f, worldSize.x - 0.5f);
        newPosition.z = Mathf.Clamp(newPosition.z, 0.5f, worldSize.y - 0.5f);

        velocity = (newPosition - transform.localPosition) / Time.deltaTime;
        if (velocity.sqrMagnitude > 0) transform.forward = velocity.normalized;

        transform.localPosition = newPosition;

        _health -= Settings.Player.healthLosePerTick;
        if (IsAlive) _score++;
        else gameObject.SetActive(false);
    }
}