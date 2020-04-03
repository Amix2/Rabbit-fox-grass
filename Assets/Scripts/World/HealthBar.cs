using UnityEngine;
namespace World
{

    public class HealthBar : MonoBehaviour
    {
        private Material material;
        private IAlive owner;
        private float fullness = 1f;

        public float Fullness
        {
            get { return fullness; }
            set
            {
                fullness = Mathf.Clamp(value, 0f, 1f);
                transform.localScale = new Vector3(0.1f * fullness, 1, 0.01f);
                material.color = Color.Lerp(Color.red, Color.green, fullness);
            }
        }

        // Start is called before the first frame update
        private void Start()
        {
            if (Settings.Player.renderOptions != RenderOptions.Full)
            {
                Destroy(this.gameObject);
            }
            material = GetComponent<MeshRenderer>().material;
            owner = GetComponentInParent<IAlive>();
        }

        // Update is called once per frame
        private void Update()
        {
            transform.LookAt(Camera.main.transform.position);
            transform.Rotate(Vector3.right, 90f);
            if (owner != null) Fullness = owner.Health;
        }
    }
}