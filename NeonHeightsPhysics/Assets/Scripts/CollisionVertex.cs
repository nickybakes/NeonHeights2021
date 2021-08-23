using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionVertex : MonoBehaviour
{
    public void Init(Vector2 pos)
    {
        this.transform.position = pos;
    }

    public void ResetZCoord()
    {
        this.transform.position = new Vector2(this.transform.position.x, this.transform.position.y);
    }

}
