using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//represents a point (vector2) where a collision occurs
//as well as what static segment it was colliding on
public class NeonHeightsStaticCollision
{
    public Vector2 collisionPosition;
    public StaticCollisionSegment segment;

    public NeonHeightsStaticCollision(Vector2 collisionPosition, StaticCollisionSegment segment)
    {
        this.collisionPosition = collisionPosition;
        this.segment = segment;
    }
}
