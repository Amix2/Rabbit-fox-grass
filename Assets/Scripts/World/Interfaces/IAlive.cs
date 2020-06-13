using UnityEngine;
using System.Collections;
namespace World
{
    public interface IAlive
    {
        float Health { get; set; }
        float HungerRate { get; }
        bool IsAlive { get; }
    }

}