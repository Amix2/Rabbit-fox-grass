using UnityEngine;
using System.Collections;

public abstract class WorldObject : MonoBehaviour
{
    protected Vector3 position;
    public Vector3 Position { get { return position; } }

    protected Vector3 forward;
    public Vector3 Forward { get { return forward; } }

    public Vector3 Right { get; protected set; }

    protected void Awake()
    {
        //if(Settings.Player.renderOptions == RenderOptions.None)
        //{
        //    this.DisableModel();
        //}
    }

    public void DisableModel()
    {
        foreach (Transform eachChild in transform)
        {
            if (eachChild.name == "Model")
            {
                eachChild.gameObject.SetActive(false);
            }
        }
    }

    public void EnableModel()
    {
        foreach (Transform eachChild in transform)
        {
            if (eachChild.name == "Model")
            {
                eachChild.gameObject.SetActive(true);
            }
        }
    }
}
