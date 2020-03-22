using UnityEngine;

public class SphereScript : MonoBehaviour
{
    private float speed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("hello");
        if (Input.GetKey(KeyCode.W))
        {
            gameObject.transform.Translate(transform.forward * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.S))
        {
            gameObject.transform.Translate(-transform.forward * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            gameObject.transform.Translate(transform.right * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.A))
        {
            gameObject.transform.Translate(-transform.right * (speed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            gameObject.transform.Translate(transform.up * (speed * Time.deltaTime));
        }
    }
}