using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeonHeightsPhysicsObject : MonoBehaviour
{
    private Rect rect;
    private Rect groundRect;
    private Rect bottomRect;
    private Rect rightRect;
    private Rect leftRect;
    private Rect topRect;
    public float width = 10;
    public float height = 10;
    public float xOffset = 0;
    public float yOffset = 0;

    public float hitBoxInset = 0;

    public float collisionCheckExtension = .04f;
    public float bottomCollisionCheckExtension = .5f;

    public StaticCollisionMap staticCollisionMap;

    public Vector2 velocity;
    public Vector2 gravity = new Vector2(0, -9.8f);
    public bool enableGravity = true;

    public bool grounded = false;

    public bool onFlatGround = false;

    public float stepHeight = 1;
    public NeonHeightsStaticCollision bottomCollision;
    public NeonHeightsStaticCollision groundCollision;
    public NeonHeightsStaticCollision topCollision;
    public NeonHeightsStaticCollision rightCollision;
    public NeonHeightsStaticCollision leftCollision;


    // Start is called before the first frame update
    void Start()
    {
        // Vector3 p = new Vector2(1,2);
        // Vector3 w0 = new Vector2(1, 2);
        // Vector3 w1 = new Vector2(1, 2);
        // Vector2 projection = Vector3.Project((p - w0), (w1 - w0)) + w0;
    }

    void Awake()
    {
        rect = new Rect(transform.position + new Vector3(xOffset, yOffset, 0), new Vector2(width, height));
        bottomRect = new Rect(transform.position + new Vector3(xOffset, yOffset, 0), new Vector2(width, height));
        rightRect = new Rect(transform.position + new Vector3(xOffset, yOffset, 0), new Vector2(width, height));
        leftRect = new Rect(transform.position + new Vector3(xOffset, yOffset, 0), new Vector2(width, height));
        topRect = new Rect(transform.position + new Vector3(xOffset, yOffset, 0), new Vector2(width, height));
    }

    /// <summary>
    /// Gives this a vector2 of raw values, and the method will multiply them by "Time.deltaTime" and
    /// set velocity to that
    /// </summary>
    /// <param name="newVelocity">Raw velocity values (not affected by frame time)</param>
    public void SetVelocityRaw(Vector2 newVelocity)
    {
        velocity = newVelocity;
    }

    public void SetPositionY(float y)
    {
        transform.position = new Vector3(transform.position.x, y);
    }

    public void SetPositionX(float x)
    {
        transform.position = new Vector3(x, transform.position.y);
    }

    public void CheckForCollisionsVertical()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> verticalCollisions = new List<NeonHeightsStaticCollision>();
        if (!grounded)
            bottomCollision = null;
        bottomCollision = null;
        topCollision = null;

        //moving up
        if (velocity.y > 0)
        {
            topRect.x = transform.position.x + xOffset;
            topRect.y = transform.position.y + yOffset - hitBoxInset;
            topRect.width = width;
            topRect.height = Time.deltaTime * velocity.y + hitBoxInset + height + collisionCheckExtension;
            //bottom left corner of box
            // topRect.x = transform.position.x + xOffset;
            // topRect.y = transform.position.y + height + yOffset - hitBoxInset;
            // topRect.width = width;
            // topRect.height = velocity.y + hitBoxInset;

            Debug.DrawLine(topRect.max, topRect.max + new Vector2(.5f, .5f), Color.red);
            Debug.DrawLine(topRect.position, topRect.position + new Vector2(.5f, .5f), Color.white);


            foreach (StaticCollisionSegment segment in staticCollisionMap.southSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, topRect);
                if (topRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (topRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the lowest down
            topCollision = null;
            foreach (NeonHeightsStaticCollision collision in verticalCollisions)
            {
                if (topCollision == null || topCollision.collisionPosition.y > collision.collisionPosition.y)
                {
                    topCollision = collision;
                }
            }
        }
        //moving down
        if (velocity.y < 0 || grounded)
        {
            bottomRect.x = transform.position.x + xOffset;
            bottomRect.y = transform.position.y + yOffset + hitBoxInset + height;
            bottomRect.width = width;
            bottomRect.height = Time.deltaTime * velocity.y - hitBoxInset - collisionCheckExtension - height;
            //top left corner of hitbox
            // bottomRect.x = transform.position.x + xOffset;
            // bottomRect.y = transform.position.y + yOffset + hitBoxInset;
            // bottomRect.width = width;
            // bottomRect.height = velocity.y + hitBoxInset;

            //Debug.DrawLine(bottomRect.position, bottomRect.max, Color.green);


            foreach (StaticCollisionSegment segment in staticCollisionMap.northSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, bottomRect);
                if (bottomRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (bottomRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the highest up
            bottomCollision = null;
            foreach (NeonHeightsStaticCollision collision in verticalCollisions)
            {
                if (bottomCollision == null || bottomCollision.collisionPosition.y > collision.collisionPosition.y)
                {
                    bottomCollision = collision;
                }
            }
        }

        //this object is grounded if a bottom collision is found
        grounded = bottomCollision != null;
        // grounde;d = bottomCollision != null && Mathf.Abs(bottomCollision.collisionPosition.y - transform.position.y) <= collisionCheckExtension;
    }


    public void GroundCheck()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> verticalCollisions = new List<NeonHeightsStaticCollision>();
        groundCollision = null;



        groundRect.x = transform.position.x + xOffset;
        groundRect.y = transform.position.y + yOffset + hitBoxInset + 2f;
        groundRect.width = width;
        groundRect.height = -hitBoxInset - 4;

        Debug.DrawLine(groundRect.position, groundRect.max, Color.green);


        foreach (StaticCollisionSegment segment in staticCollisionMap.northSegments)
        {
            List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, groundRect);
            if (groundRect.Contains(segment.a, true))
            {
                intersections.Add(segment.a);
            }
            if (groundRect.Contains(segment.b, true))
            {
                intersections.Add(segment.b);
            }
            foreach (Vector2 v in intersections)
            {
                verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
            }
        }

        //find the collision that is the highest up
        groundCollision = null;
        foreach (NeonHeightsStaticCollision collision in verticalCollisions)
        {
            if (groundCollision != null && groundCollision.collisionPosition == collision.collisionPosition && Mathf.Sign(groundCollision.segment.normalNormalized.x) != Mathf.Sign(collision.segment.normalNormalized.x))
            {
                onFlatGround = true;
                groundCollision = collision;
            }
            else if (groundCollision == null || groundCollision.collisionPosition.y <= collision.collisionPosition.y)
            {
                if (groundCollision == null || groundCollision.collisionPosition.y != collision.collisionPosition.y)
                {
                    onFlatGround = false;
                    groundCollision = collision;
                }
                else if (groundCollision == null || groundCollision.collisionPosition.y == collision.collisionPosition.y && groundCollision.segment.angleFromHorizontalDegrees > collision.segment.angleFromHorizontalDegrees)
                {
                    onFlatGround = false;
                    groundCollision = collision;
                }
            }
        }
    }



    public void CheckCollisionsBottom()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> verticalCollisions = new List<NeonHeightsStaticCollision>();
        bottomCollision = null;

        if (velocity.y < 0)
        {


            bottomRect.x = transform.position.x + xOffset;
            bottomRect.y = transform.position.y + yOffset + hitBoxInset;
            bottomRect.width = width;
            bottomRect.height = Mathf.Min(Time.deltaTime * velocity.y, 0) - hitBoxInset;

            Debug.DrawLine(bottomRect.position, bottomRect.max, Color.red);


            foreach (StaticCollisionSegment segment in staticCollisionMap.northSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, bottomRect);
                if (bottomRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (bottomRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the highest up
            bottomCollision = null;
            foreach (NeonHeightsStaticCollision collision in verticalCollisions)
            {
                if (bottomCollision != null && bottomCollision.collisionPosition == collision.collisionPosition && Mathf.Sign(bottomCollision.segment.normalNormalized.x) != Mathf.Sign(collision.segment.normalNormalized.x))
                {
                    onFlatGround = true;
                    bottomCollision = collision;
                }
                else if (bottomCollision == null || bottomCollision.collisionPosition.y <= collision.collisionPosition.y)
                {
                    onFlatGround = false;
                    bottomCollision = collision;
                }
            }

            //grounded = bottomCollision != null;
            // grounde;d = bottomCollision != null && Mathf.Abs(bottomCollision.collisionPosition.y - transform.position.y) <= collisionCheckExtension;
        }
    }

    public void CheckCollisionsTop()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> verticalCollisions = new List<NeonHeightsStaticCollision>();
        topCollision = null;

        //moving up
        if (velocity.y > 0)
        {
            topRect.x = transform.position.x + xOffset;
            topRect.y = transform.position.y + yOffset + height - hitBoxInset;
            topRect.width = width;
            topRect.height = Mathf.Max(Time.deltaTime * velocity.y, 0) + hitBoxInset;
            //bottom left corner of box
            // topRect.x = transform.position.x + xOffset;
            // topRect.y = transform.position.y + height + yOffset - hitBoxInset;
            // topRect.width = width;
            // topRect.height = velocity.y + hitBoxInset;

            Debug.DrawLine(topRect.max, topRect.max + new Vector2(.5f, .5f), Color.red);
            Debug.DrawLine(topRect.position, topRect.position + new Vector2(.5f, .5f), Color.white);


            foreach (StaticCollisionSegment segment in staticCollisionMap.southSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, topRect);
                if (topRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (topRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the lowest down
            topCollision = null;
            foreach (NeonHeightsStaticCollision collision in verticalCollisions)
            {
                if (topCollision == null || topCollision.collisionPosition.y > collision.collisionPosition.y)
                {
                    topCollision = collision;
                }
            }
        }
    }

    public void CheckCollisionsLeft()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> horizontalCollisions = new List<NeonHeightsStaticCollision>();
        leftCollision = null;

        if (velocity.x < 0)
        {
            //bottom left corner of hitbox
            leftRect.x = transform.position.x + xOffset + hitBoxInset;
            leftRect.y = transform.position.y + yOffset;
            leftRect.width = Mathf.Min(Time.deltaTime * velocity.x, 0) - hitBoxInset;
            leftRect.height = height;

            Debug.DrawLine(leftRect.position, leftRect.max, Color.yellow);

            foreach (StaticCollisionSegment segment in staticCollisionMap.eastSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, leftRect);
                if (leftRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (leftRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    horizontalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the farthest right
            leftCollision = null;
            foreach (NeonHeightsStaticCollision collision in horizontalCollisions)
            {
                // if (leftCollision != null && leftCollision.collisionPosition == collision.collisionPosition && Mathf.Sign(leftCollision.segment.normalNormalized.x) != Mathf.Sign(collision.segment.normalNormalized.x))
                // {
                //     onFlatGround = true;
                // }
                // else if (leftCollision == null || (leftCollision.collisionPosition.x <= collision.collisionPosition.x && leftCollision.segment.angleFromHorizontalDegrees < collision.segment.angleFromHorizontalDegrees))
                // {
                //     onFlatGround = false;
                //     leftCollision = collision;
                // }
                if (leftCollision == null || (leftCollision.collisionPosition.x <= collision.collisionPosition.x && leftCollision.segment.angleFromHorizontalDegrees < collision.segment.angleFromHorizontalDegrees))
                {
                    leftCollision = collision;
                }
            }
        }
    }

    public void CheckCollisionsRight()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> horizontalCollisions = new List<NeonHeightsStaticCollision>();
        rightCollision = null;

        //moving to the right
        if (velocity.x > 0)
        {
            //bottom right corner of hitbox
            rightRect.x = transform.position.x + xOffset - hitBoxInset + width;
            rightRect.y = transform.position.y + yOffset;
            rightRect.width = Mathf.Max(Time.deltaTime * velocity.x, 0) + hitBoxInset;
            rightRect.height = height;

            Debug.DrawLine(rightRect.position, rightRect.max);

            foreach (StaticCollisionSegment segment in staticCollisionMap.westSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, rightRect);
                if (rightRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (rightRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    horizontalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the farthest left
            rightCollision = null;
            foreach (NeonHeightsStaticCollision collision in horizontalCollisions)
            {
                // if (rightCollision != null && rightCollision.collisionPosition == collision.collisionPosition && Mathf.Sign(rightCollision.segment.normalNormalized.x) != Mathf.Sign(collision.segment.normalNormalized.x))
                // {
                //     onFlatGround = true;
                // }
                // else if (rightCollision == null || (rightCollision.collisionPosition.x >= collision.collisionPosition.x && rightCollision.segment.angleFromHorizontalDegrees < collision.segment.angleFromHorizontalDegrees))
                // {
                //     onFlatGround = false;
                //     rightCollision = collision;
                // }

                if (rightCollision == null || (rightCollision.collisionPosition.x >= collision.collisionPosition.x && rightCollision.segment.angleFromHorizontalDegrees < collision.segment.angleFromHorizontalDegrees))
                {
                    rightCollision = collision;
                }
            }
        }
    }

    public void CheckForCollisionsHorizontal()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> horizontalCollisions = new List<NeonHeightsStaticCollision>();
        leftCollision = null;
        rightCollision = null;

        //moving to the right
        if (velocity.x > 0)
        {
            //bottom right corner of hitbox
            rightRect.x = transform.position.x + xOffset - hitBoxInset;
            rightRect.y = transform.position.y + yOffset;
            rightRect.width = Time.deltaTime * velocity.x + hitBoxInset + collisionCheckExtension + width;
            rightRect.height = height;

            Debug.DrawLine(rightRect.position, rightRect.max);

            foreach (StaticCollisionSegment segment in staticCollisionMap.westSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, rightRect);
                if (rightRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (rightRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    horizontalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the farthest left
            rightCollision = null;
            foreach (NeonHeightsStaticCollision collision in horizontalCollisions)
            {
                if (rightCollision == null || rightCollision.collisionPosition.x > collision.collisionPosition.x)
                {
                    rightCollision = collision;
                }
            }
        }
        //moving to the left
        else if (velocity.x < 0)
        {
            //bottom left corner of hitbox
            leftRect.x = transform.position.x + xOffset + hitBoxInset + width;
            leftRect.y = transform.position.y + yOffset;
            leftRect.width = Time.deltaTime * velocity.x - hitBoxInset - collisionCheckExtension - width;
            leftRect.height = height;

            Debug.DrawLine(leftRect.position, leftRect.max);

            foreach (StaticCollisionSegment segment in staticCollisionMap.eastSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, leftRect);
                if (leftRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (leftRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    horizontalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the farthest right
            leftCollision = null;
            foreach (NeonHeightsStaticCollision collision in horizontalCollisions)
            {
                if (leftCollision == null || leftCollision.collisionPosition.x < collision.collisionPosition.x)
                {
                    leftCollision = collision;
                }
            }
        }
    }

    // public void ApplyCollisionsVertical()
    // {
    //     if (bottomCollision != null)
    //     {
    //         velocity.y = 0;
    //         SetPositionY(bottomCollision.collisionPosition.y);
    //     }

    //     if (topCollision != null)
    //     {
    //         velocity.y = Mathf.Abs(topCollision.collisionPosition.y - (transform.position.y + height + yOffset));
    //     }
    // }

    // public void ApplyCollisionsHorizontal()
    // {
    //     if (rightCollision != null)
    //     {
    //         velocity.x = 0;
    //         transform.position = new Vector2(Mathf.Min(transform.position.x + width + xOffset, rightCollision.collisionPosition.x) - width - xOffset, transform.position.y);
    //     }

    //     if (leftCollision != null)
    //     {
    //         Debug.Log("www");
    //         velocity.x = 0;
    //         transform.position = new Vector2(Mathf.Max(transform.position.x + xOffset, leftCollision.collisionPosition.x), transform.position.y);
    //     }
    // }

    /// <summary>
    /// Checks if the object's bounding box will collide with walls in the direction of its velocity.
    /// Stores those collisions, and also stores whether the object is "grounded" based on if it is
    /// colliding with something on the bottom side of the object.
    /// </summary>
    public void CheckForCollisions()
    {
        if (staticCollisionMap == null)
        {
            staticCollisionMap = FindObjectOfType<StaticCollisionMap>();
        }

        List<NeonHeightsStaticCollision> verticalCollisions = new List<NeonHeightsStaticCollision>();
        List<NeonHeightsStaticCollision> horizontalCollisions = new List<NeonHeightsStaticCollision>();
        if (!grounded)
            bottomCollision = null;
        topCollision = null;
        leftCollision = null;
        rightCollision = null;

        //moving up
        if (velocity.y > 0)
        {
            topRect.x = transform.position.x + xOffset;
            topRect.y = transform.position.y + yOffset - hitBoxInset;
            topRect.width = width;
            topRect.height = velocity.y + hitBoxInset + height + collisionCheckExtension;
            //bottom left corner of box
            // topRect.x = transform.position.x + xOffset;
            // topRect.y = transform.position.y + height + yOffset - hitBoxInset;
            // topRect.width = width;
            // topRect.height = velocity.y + hitBoxInset;

            Debug.DrawLine(topRect.max, topRect.max + new Vector2(.5f, .5f), Color.red);
            Debug.DrawLine(topRect.position, topRect.position + new Vector2(.5f, .5f), Color.white);


            foreach (StaticCollisionSegment segment in staticCollisionMap.southSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, topRect);
                if (topRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (topRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the lowest down
            topCollision = null;
            foreach (NeonHeightsStaticCollision collision in verticalCollisions)
            {
                if (topCollision == null || topCollision.collisionPosition.y > collision.collisionPosition.y)
                {
                    topCollision = collision;
                }
            }
        }
        //moving down
        if (velocity.y < 0 || grounded)
        {
            bottomRect.x = transform.position.x + xOffset;
            bottomRect.y = transform.position.y + yOffset + hitBoxInset;
            bottomRect.width = width;
            bottomRect.height = velocity.y - hitBoxInset - collisionCheckExtension;
            //top left corner of hitbox
            // bottomRect.x = transform.position.x + xOffset;
            // bottomRect.y = transform.position.y + yOffset + hitBoxInset;
            // bottomRect.width = width;
            // bottomRect.height = velocity.y + hitBoxInset;

            Debug.DrawLine(bottomRect.position, bottomRect.max, Color.green);


            foreach (StaticCollisionSegment segment in staticCollisionMap.northSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, bottomRect);
                if (bottomRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (bottomRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    verticalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the highest up
            bottomCollision = null;
            foreach (NeonHeightsStaticCollision collision in verticalCollisions)
            {
                if (bottomCollision == null || bottomCollision.collisionPosition.y > collision.collisionPosition.y)
                {
                    bottomCollision = collision;
                }
            }
        }


        //moving to the right
        if (velocity.x > 0)
        {
            //bottom right corner of hitbox
            rightRect.x = transform.position.x + xOffset + width - hitBoxInset;
            rightRect.y = transform.position.y + yOffset;
            rightRect.width = velocity.x + hitBoxInset + collisionCheckExtension;
            rightRect.height = height;

            Debug.DrawLine(rightRect.position, rightRect.max);

            foreach (StaticCollisionSegment segment in staticCollisionMap.westSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, rightRect);
                if (rightRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (rightRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    horizontalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the farthest left
            rightCollision = null;
            foreach (NeonHeightsStaticCollision collision in horizontalCollisions)
            {
                if (rightCollision == null || rightCollision.collisionPosition.x > collision.collisionPosition.x)
                {
                    rightCollision = collision;
                }
            }
        }
        //moving to the left
        else if (velocity.x < 0)
        {
            //bottom left corner of hitbox
            leftRect.x = transform.position.x + xOffset + hitBoxInset;
            leftRect.y = transform.position.y + yOffset;
            leftRect.width = velocity.x - hitBoxInset - collisionCheckExtension;
            leftRect.height = height;

            Debug.DrawLine(leftRect.position, leftRect.max);

            foreach (StaticCollisionSegment segment in staticCollisionMap.eastSegments)
            {
                List<Vector2> intersections = NeonHeightsHitBox.StaticLineSegmentAndBoxCollision(segment, leftRect);
                if (leftRect.Contains(segment.a, true))
                {
                    intersections.Add(segment.a);
                }
                if (leftRect.Contains(segment.b, true))
                {
                    intersections.Add(segment.b);
                }
                foreach (Vector2 v in intersections)
                {
                    horizontalCollisions.Add(new NeonHeightsStaticCollision(v, segment));
                }
            }

            //find the collision that is the farthest left
            leftCollision = null;
            foreach (NeonHeightsStaticCollision collision in horizontalCollisions)
            {
                if (leftCollision == null || leftCollision.collisionPosition.x < collision.collisionPosition.x)
                {
                    leftCollision = collision;
                }
            }
        }

        //this object is grounded if a bottom collision is found
        grounded = bottomCollision != null && Mathf.Abs(bottomCollision.collisionPosition.y - transform.position.y) <= collisionCheckExtension;
    }

    public void DrawBoundingRect()
    {
        Debug.DrawLine(rect.position, new Vector3(rect.xMin, rect.yMax));
        Debug.DrawLine(new Vector3(rect.xMin, rect.yMax), new Vector3(rect.xMax, rect.yMax));
        Debug.DrawLine(new Vector3(rect.xMax, rect.yMax), new Vector3(rect.xMax, rect.yMin));
        Debug.DrawLine(new Vector3(rect.xMax, rect.yMin), rect.position);
    }

    public void ApplyVelocityX()
    {
        transform.position = new Vector2(transform.position.x + (velocity.x * Time.deltaTime), transform.position.y);
    }

    public void ApplyVelocityY()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y + velocity.y * Time.deltaTime);
    }

    /// <summary>
    /// If gravity is enabled and the object is not grounded, gravity vector is added to the velocity vector
    /// </summary>
    public void ApplyGravity()
    {
        if (enableGravity && !grounded)
        {
            velocity += gravity * Time.deltaTime;
        }
        if (grounded)
        {
            velocity.y = -100;
            //velocity.y = Mathf.Max(groundCollision.segment.downPointingTangent.y * Mathf.Abs(velocity.x), groundCollision.segment.downPointingTangent.y);
        }
    }

    /// <summary>
    /// Tranlates the object to its new position based on its velocity vector
    /// </summary>
    public void ApplyVelocityToTransform()
    {
        transform.position = new Vector2(transform.position.x + velocity.x * Time.deltaTime, transform.position.y + velocity.y * Time.deltaTime);
    }

    /// <summary>
    /// Prevents the object from going further beyond a wall it is colliding with
    /// </summary>
    public void ApplyCollisions()
    {
        if (grounded)
        {
            velocity.y = Mathf.Max(velocity.y, 0);
        }

        if (bottomCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Max(transform.position.y, bottomCollision.collisionPosition.y));
        }

        if (topCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Min(transform.position.y + height + yOffset, topCollision.collisionPosition.y) - height - yOffset);
        }

        if (rightCollision != null)
        {
            transform.position = new Vector2(Mathf.Min(transform.position.x + width + xOffset, rightCollision.collisionPosition.x) - width - xOffset, transform.position.y);
        }

        if (leftCollision != null)
        {
            transform.position = new Vector2(Mathf.Max(transform.position.x, leftCollision.collisionPosition.x), transform.position.y);
        }
    }

    /// <summary>
    /// Updates the objects bounding box to the objects new position and size
    /// </summary>
    public void UpdateCollisionRect()
    {
        rect.width = width;
        rect.height = height;
        rect.x = transform.position.x + xOffset;
        rect.y = transform.position.y + yOffset;
    }


    // Update is called once per frame
    void Update()
    {
        //Use these methods hear to do a very basic test of collision detection
        // CheckForCollisions();
        // ApplyGravity();
        // ApplyVelocityToTransform();
        // ApplyCollisions();
        // UpdateCollisionRect();
    }
}
