using UnityEngine;
using System.Collections;

public abstract class WorldObject : MonoBehaviour
{
    protected Vector3 position;
    public Vector3 Position { get { return position; } }

    // Use this for initialization
    protected void Start()
    {
        if(Settings.Player.renderOptions == RenderOptions.None)
        {
            foreach (Transform eachChild in transform)
            {
                if (eachChild.name == "Model")
                {
                    eachChild.gameObject.SetActive(false);
                }
            }
        }
    }
}
