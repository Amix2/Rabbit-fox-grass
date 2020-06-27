using UnityEngine;
using World;

public class RestartButton : MonoBehaviour
{
    public WorldCreator worldCreator;

    public void OnClickReset()
    {
        worldCreator.ResetWorlds();
    }
}