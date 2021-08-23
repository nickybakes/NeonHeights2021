using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum CharacterState
{
    /// <summary>
    /// Grounded, standing still
    /// </summary>
    Idle,
    /// <summary>
    /// Grounded, but player is moving them across the ground
    /// </summary>
    Run,

    /// <summary>
    /// Grounded, but player is moving them up a slope
    /// </summary>
    AscendSlope,

    /// <summary>
    /// Grounded, but player is moving them down a slope
    /// </summary>
    DescendSlope,

    /// <summary>
    /// Freely going up in the air, not grounded
    /// </summary>
    Jump,
    /// <summary>
    /// Freely going down in the air, not grounded
    /// </summary>
    Fall,
    /// <summary>
    /// Player is attempting to catch the ball
    /// </summary>
    Catch,
    /// <summary>
    /// Player has thrown the ball
    /// </summary>
    Throw,
    /// <summary>
    /// Launches the character in a desired direction
    /// </summary>
    DashTackle,
}

public class NeonHeightsCharacterController : NeonHeightsPhysicsObject
{

    public CharacterState currentState;

    public Vector2 trueVelocity;
    public float trueSpeed;


    public float groundAngleLimit = 60;

    public float baseRunSpeed = 10;

    public float baseJumpVelocity = 20;

    public bool prevGrounded;

    public NeonHeightsStaticCollision prevGroundCollision;

    public bool ascendingSlope;
    public bool descendingSlope;

    public bool prevAscendingSlope;
    public bool prevDescendingSlope;

    public float previousSlopeAngle;
    public float slopeAngle;

    public Vector2 prevPosition;

    public Vector2 movementInput;

    public StaticCollisionSegment groundSegment;
    public StaticCollisionSegment rightSegment;
    public StaticCollisionSegment leftSegment;

    public bool slippingDownSlope;


    // Start is called before the first frame update
    void Start()
    {
        currentState = CharacterState.Idle;
    }

    public Vector2 GetPossibleCollisionPosHorizontal()
    {
        if (velocity.x > 0)
        {
            return new Vector2(Mathf.Min(transform.position.x + width + xOffset, Mathf.Min(transform.position.x + width + xOffset + velocity.x * Time.deltaTime, rightCollision.collisionPosition.x)) - width - xOffset, transform.position.y);
        }
        else
        {
            return new Vector2(Mathf.Max(transform.position.x + xOffset, Mathf.Max(transform.position.x + xOffset + velocity.x * Time.deltaTime, leftCollision.collisionPosition.x)) - xOffset, transform.position.y);
        }
    }

    public Vector2 GetPossibleCollisionPosVertical()
    {
        if (velocity.y > 0)
        {
            return new Vector2(transform.position.x, Mathf.Min(transform.position.y + height + yOffset, Mathf.Min(transform.position.y + height + yOffset + velocity.y * Time.deltaTime, topCollision.collisionPosition.y)) - height - yOffset);
        }
        else
        {
            return new Vector2(transform.position.x, Mathf.Max(transform.position.y + yOffset, Mathf.Max(transform.position.y + yOffset + (velocity.y * Time.deltaTime), bottomCollision.collisionPosition.y)) - yOffset);
        }
    }

    public void ApplyVerticalCollisions()
    {
        if (bottomCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Max(transform.position.y + yOffset + (velocity.y * Time.deltaTime), bottomCollision.collisionPosition.y) - yOffset);
            grounded = true;
        }
        else if (groundCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Max(transform.position.y + yOffset + (velocity.y * Time.deltaTime), groundCollision.collisionPosition.y) - yOffset);
        }
        else if (groundCollision == null && bottomCollision == null && velocity.y < 0)
        {
            ApplyVelocityY();
        }

        if (topCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Min(transform.position.y + height + yOffset + velocity.y * Time.deltaTime, topCollision.collisionPosition.y) - height - yOffset);
            velocity.y = 0;
        }
        else if (topCollision == null && velocity.y > 0)
        {
            ApplyVelocityY();
        }
    }

    public void ApplyHorizontalCollisions()
    {
        if (rightCollision != null)
        {
            transform.position = new Vector2(Mathf.Min(transform.position.x + width + xOffset + velocity.x * Time.deltaTime, rightCollision.collisionPosition.x) - width - xOffset, transform.position.y);
        }
        // else if (rightCollision == null && velocity.x > 0)
        // {
        //     ApplyVelocityX();
        // }

        if (leftCollision != null)
        {
            transform.position = new Vector2(Mathf.Max(transform.position.x + xOffset + velocity.x * Time.deltaTime, leftCollision.collisionPosition.x) - xOffset, transform.position.y);
        }
        // else if (leftCollision == null && velocity.x < 0)
        // {
        //     ApplyVelocityX();
        // }
    }

    public void UpdateAugust()
    {
        DrawBoundingRect();



        CheckCollisionsBottom();
        CheckCollisionsTop();

        prevGrounded = grounded;
        slippingDownSlope = false;
        ApplyVerticalCollisions();


        prevGroundCollision = groundCollision;
        GroundCheck();
        if (groundCollision == null && bottomCollision == null)
        {
            grounded = false;
        }

        if (groundCollision != null && groundCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
        {
            grounded = false;
        }
        if (bottomCollision != null && bottomCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
        {
            grounded = false;
        }

        if (prevGrounded && !grounded && prevGroundCollision != null)
        {
            velocity.y = Mathf.Max(velocity.y, prevGroundCollision.segment.downPointingTangent.y * Mathf.Abs(velocity.x));
        }

        if (groundCollision != null)
        {
            groundSegment = groundCollision.segment;
        }
        else
        {
            groundSegment = null;
        }


        if (leftCollision != null && groundCollision != null && leftCollision.segment == groundCollision.segment)
        {
            Debug.Log("SAME THINGGG ");
        }

        if (!grounded && bottomCollision != null && bottomCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && (Mathf.Sign(bottomCollision.segment.downPointingTangent.x) != Mathf.Sign(velocity.x) || Mathf.Abs(velocity.normalized.x) <= Mathf.Abs(bottomCollision.segment.downPointingTangent.x)) && bottomCollision.segment.length > stepHeight)
        {

            grounded = false;
            ApplyGravity();
            float originalFallVelocityY = velocity.y;
            velocity.y = bottomCollision.segment.downPointingTangent.y * Mathf.Abs(velocity.y);
            velocity.x = bottomCollision.segment.downPointingTangent.x * Mathf.Abs(velocity.y);
            CheckCollisionsLeft();
            CheckCollisionsRight();
            if (velocity.x > 0 && rightCollision == null)
            {
                slippingDownSlope = true;
                ApplyVelocityToTransform();
                velocity.y = originalFallVelocityY;
            }
            else if (velocity.x < 0 && leftCollision == null)
            {
                slippingDownSlope = true;
                ApplyVelocityToTransform();
                velocity.y = originalFallVelocityY;
            }
            else
            {
                grounded = true;
            }
        }
        else if (!grounded && groundCollision != null && groundCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && (Mathf.Sign(groundCollision.segment.downPointingTangent.x) != Mathf.Sign(velocity.x) || Mathf.Abs(velocity.normalized.x) <= Mathf.Abs(groundCollision.segment.downPointingTangent.x)) && groundCollision.segment.length > stepHeight)
        {

            grounded = false;
            ApplyGravity();
            float originalFallVelocityY = velocity.y;
            velocity.y = groundCollision.segment.downPointingTangent.y * Mathf.Abs(velocity.y);
            velocity.x = groundCollision.segment.downPointingTangent.x * Mathf.Abs(velocity.y);
            CheckCollisionsLeft();
            CheckCollisionsRight();
            if (velocity.x > 0 && rightCollision == null)
            {
                slippingDownSlope = true;
                ApplyVelocityToTransform();
                velocity.y = originalFallVelocityY;
            }
            else if (velocity.x < 0 && leftCollision == null)
            {
                slippingDownSlope = true;
                ApplyVelocityToTransform();
                velocity.y = originalFallVelocityY;
            }
            else
            {
                grounded = true;
            }
        }
        else
        {
            CheckCollisionsLeft();
            CheckCollisionsRight();

            if (rightCollision != null)
            {
                rightSegment = rightCollision.segment;
            }
            else
            {
                rightSegment = null;
            }

            if (leftCollision != null)
            {
                leftSegment = leftCollision.segment;
            }
            else
            {
                leftSegment = null;
            }

            //moving to the right
            if (velocity.x > 0)
            {
                if (rightCollision != null && rightCollision.segment.topVertex.y < transform.position.y + yOffset + stepHeight && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && transform.position.y < rightCollision.segment.topVertex.y && groundCollision != null)
                {
                    Debug.Log("STEP RIGHT");

                    SetPositionY(rightCollision.segment.topVertex.y + .1f - yOffset);
                    UpdateCollisionRect();
                    CheckCollisionsRight();
                    GroundCheck();
                }
                //we are on a walkable slope
                if (grounded && groundCollision != null && groundCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    //hit an unwalkable wall
                    if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && rightCollision.segment.topVertex.y >= transform.position.y + yOffset + stepHeight)
                    {
                        Debug.Log("a");
                        //ApplyHorizontalCollisions();
                    }
                    else
                    {
                        if (!onFlatGround)
                        {
                            Debug.Log("b");
                            velocity.y = groundCollision.segment.rightPointingTangent.y * baseRunSpeed;
                            velocity.x = groundCollision.segment.rightPointingTangent.x * baseRunSpeed;
                            CheckCollisionsTop();
                            ApplyVerticalCollisions();
                            if (topCollision == null)
                            {
                                ApplyVelocityX();
                            }
                        }
                        else
                        {
                            Debug.Log("c");
                            velocity.y = 0;
                            velocity.x = baseRunSpeed;
                            ApplyVelocityToTransform();
                        }
                    }
                }
                //we are on an UNwalkable slope
                else if (grounded && groundCollision != null && groundCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    // grounded = false;
                    Debug.Log("d");
                    // velocity.y = groundCollision.segment.downPointingTangent.y * 50;
                    // velocity.x = groundCollision.segment.downPointingTangent.x * 50;
                    // ApplyVelocityToTransform();
                }
                else if (!grounded && rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    ApplyHorizontalCollisions();
                    ApplyVerticalCollisions();
                    Debug.Log("e");
                }
                else if (!grounded && rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    // ApplyHorizontalCollisions();
                    grounded = false;
                    Debug.Log("f");
                }
                else if (!grounded && rightCollision == null)
                {
                    Debug.Log("g");
                    ApplyVelocityX();
                }
            }
            //moving to the left
            else if (velocity.x < 0)
            {
                if (leftCollision != null && leftCollision.segment.topVertex.y < transform.position.y + yOffset + stepHeight && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && transform.position.y < leftCollision.segment.topVertex.y && groundCollision != null)
                {
                    Debug.Log("STEP LEFT");
                    SetPositionY(leftCollision.segment.topVertex.y + .1f - yOffset);
                    UpdateCollisionRect();
                    CheckCollisionsLeft();
                    GroundCheck();
                }
                //we are on a walkable slope
                if (grounded && groundCollision != null && groundCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    //hit an unwalkable wall
                    if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && leftCollision.segment.topVertex.y >= transform.position.y + yOffset + stepHeight)
                    {
                        Debug.Log("A");
                        //ApplyHorizontalCollisions();
                    }
                    else
                    {
                        if (!onFlatGround)
                        {
                            Debug.Log("B");
                            velocity.y = groundCollision.segment.leftPointingTangent.y * baseRunSpeed;
                            velocity.x = groundCollision.segment.leftPointingTangent.x * baseRunSpeed;
                            CheckCollisionsTop();
                            ApplyVerticalCollisions();
                            if (topCollision == null)
                            {
                                ApplyVelocityX();
                            }
                        }
                        else
                        {
                            Debug.Log("C");
                            velocity.y = 0;
                            velocity.x = -baseRunSpeed;
                            ApplyVelocityToTransform();
                        }
                    }

                }
                //we are on an UNwalkable slope
                else if (grounded && groundCollision != null && groundCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    // grounded = false;
                    Debug.Log("D");
                    // velocity.y = groundCollision.segment.downPointingTangent.y * 50;
                    // velocity.x = groundCollision.segment.downPointingTangent.x * 50;
                    // ApplyVelocityToTransform();
                }
                else if (!grounded && leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    Debug.Log("E");
                    ApplyHorizontalCollisions();
                    ApplyVerticalCollisions();
                }
                else if (!grounded && leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    Debug.Log("F");
                    grounded = false;
                    //ApplyHorizontalCollisions();
                }
                else if (!grounded && leftCollision == null)
                {
                    Debug.Log("G");
                    ApplyVelocityX();
                }
            }
            else if (velocity.x == 0)
            {

            }
        }



        trueVelocity = (new Vector2(transform.position.x, transform.position.y) - prevPosition) / Time.deltaTime;
        trueSpeed = trueVelocity.magnitude;
        prevPosition = transform.position;

        if (!slippingDownSlope)
            ApplyGravity();

        if (grounded)
        {
            if (Input.GetKey(KeyCode.A))
            {
                //velocity.x = Mathf.Max(-baseRunSpeed, velocity.x -= (baseRunSpeed * 8) * Time.deltaTime);
                velocity.x = -1 * baseRunSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                velocity.x = baseRunSpeed;
                //velocity.x = Mathf.Min(baseRunSpeed, velocity.x += (baseRunSpeed * 8) * Time.deltaTime);
            }
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                velocity.x = 0;
                // if (velocity.x > 0)
                // {
                //     velocity.x = Mathf.Max(0, velocity.x -= (baseRunSpeed*30.65f) * Time.deltaTime);
                // }
                // else if (velocity.x < 0)
                // {
                //     velocity.x = Mathf.Min(0, velocity.x += (baseRunSpeed*30.65f) * Time.deltaTime);
                // }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                velocity.x = Mathf.Max(-baseRunSpeed, velocity.x -= (baseRunSpeed * 8) * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                velocity.x = Mathf.Min(baseRunSpeed, velocity.x += (baseRunSpeed * 8) * Time.deltaTime);
            }
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                if (velocity.x > 0)
                {
                    velocity.x = Mathf.Max(0, velocity.x -= (baseRunSpeed) * Time.deltaTime);
                }
                else if (velocity.x < 0)
                {
                    velocity.x = Mathf.Min(0, velocity.x += (baseRunSpeed) * Time.deltaTime);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded && topCollision == null)
        {
            velocity.y = baseJumpVelocity;
            grounded = false;
        }

        if (Input.GetKey(KeyCode.R))
        {
            transform.position = new Vector2(-14.65f, 5.54f);
            velocity = Vector2.zero;
        }

        UpdateCollisionRect();
    }



    public void NewNewNewUpdate()
    {
        DrawBoundingRect();
        prevPosition = transform.position;

        rightCollision = null;
        leftCollision = null;
        bottomCollision = null;
        topCollision = null;

        switch (currentState)
        {
            default:
            case (CharacterState.Idle):
                {
                    CheckCollisionsBottom();
                    ApplyVerticalCollisions();
                    if (!grounded)
                    {
                        currentState = CharacterState.Fall;
                        break;
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                        currentState = CharacterState.Run;
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                        currentState = CharacterState.Run;
                    }
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        velocity.y = baseJumpVelocity;
                        currentState = CharacterState.Jump;
                        grounded = false;
                        break;
                    }
                    break;
                }
            case (CharacterState.Run):
                {
                    CheckCollisionsBottom();
                    ApplyVerticalCollisions();
                    if (!grounded)
                    {
                        currentState = CharacterState.Fall;
                        break;
                    }
                    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    {
                        velocity.x = 0;
                        currentState = CharacterState.Idle;
                        break;
                    }
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        velocity.y = baseJumpVelocity;
                        currentState = CharacterState.Jump;
                        grounded = false;
                        break;
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                        CheckCollisionsLeft();
                        if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                        {
                            currentState = CharacterState.Idle;
                            ApplyHorizontalCollisions();
                        }
                        else
                        {
                            ApplyVelocityX();
                        }
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                        CheckCollisionsRight();
                        if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                        {
                            currentState = CharacterState.Idle;
                            ApplyHorizontalCollisions();
                        }
                        else
                        {
                            ApplyVelocityX();
                        }
                    }
                    break;
                }

            case (CharacterState.AscendSlope):
                {
                    break;
                }

            case (CharacterState.DescendSlope):
                {
                    break;
                }

            case (CharacterState.Jump):
                {
                    ApplyGravity();
                    CheckCollisionsTop();
                    if (topCollision != null)
                    {
                        ApplyVerticalCollisions();
                        velocity.y = 0;
                        currentState = CharacterState.Fall;
                        break;
                    }
                    else
                    {
                        ApplyVelocityY();
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                        CheckCollisionsLeft();
                        if (leftCollision != null)
                        {
                            ApplyHorizontalCollisions();
                        }
                        else
                        {
                            ApplyVelocityY();
                        }
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                        CheckCollisionsRight();
                        if (rightCollision != null)
                        {
                            ApplyHorizontalCollisions();
                        }
                        else
                        {
                            ApplyVelocityY();
                        }
                    }

                    if (velocity.y <= 0)
                    {
                        currentState = CharacterState.Fall;
                        break;
                    }
                    break;
                }
            case (CharacterState.Fall):
                {
                    ApplyGravity();
                    CheckCollisionsBottom();
                    if (bottomCollision != null)
                    {
                        ApplyVerticalCollisions();
                    }
                    else
                    {
                        ApplyVelocityY();
                    }

                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                        CheckCollisionsLeft();
                        if (leftCollision != null)
                        {
                            ApplyHorizontalCollisions();
                        }
                        else
                        {
                            ApplyVelocityY();
                        }
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                        CheckCollisionsRight();
                        if (rightCollision != null)
                        {
                            ApplyHorizontalCollisions();
                        }
                        else
                        {
                            ApplyVelocityY();
                        }
                    }

                    if (grounded)
                    {
                        currentState = CharacterState.Idle;
                    }
                    break;
                }
        }

        trueVelocity = (new Vector2(transform.position.x, transform.position.y) - prevPosition) / Time.deltaTime;
        trueSpeed = trueVelocity.magnitude;

        UpdateCollisionRect();
    }

    public void NewNewUpdate()
    {
        DrawBoundingRect();
        previousSlopeAngle = slopeAngle;
        prevAscendingSlope = ascendingSlope;
        prevDescendingSlope = descendingSlope;
        prevPosition = transform.position;

        rightCollision = null;
        leftCollision = null;
        bottomCollision = null;
        topCollision = null;

        if (Input.GetKey(KeyCode.R))
        {
            transform.position = new Vector2(-14.65f, 5.54f);
            velocity = Vector2.zero;
        }

        switch (currentState)
        {
            default:
            case (CharacterState.Idle):
                {
                    CheckCollisionsBottom();
                    CheckCollisionsTop();
                    ApplyVerticalCollisions();
                    ApplyHorizontalCollisions();

                    grounded = bottomCollision != null && transform.position.y + yOffset == bottomCollision.collisionPosition.y && bottomCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit;

                    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                    {
                        currentState = CharacterState.Run;
                        break;
                    }

                    if (Input.GetKeyDown(KeyCode.Space) && grounded)
                    {
                        velocity.y = baseJumpVelocity;
                        currentState = CharacterState.Jump;
                        grounded = false;
                        break;
                    }

                    if (velocity.y > 0)
                    {
                        currentState = CharacterState.Jump;
                        grounded = false;
                        break;
                    }

                    if (velocity.y < 0 || !grounded)
                    {
                        currentState = CharacterState.Fall;
                        grounded = false;
                        break;
                    }

                    break;
                }
            case (CharacterState.Run):
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                    }
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        velocity.y = baseJumpVelocity;
                        currentState = CharacterState.Jump;
                        grounded = false;
                        break;
                    }
                    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    {
                        velocity.x = 0;
                        velocity.y = 0;
                        currentState = CharacterState.Idle;
                        break;
                    }
                    if (!grounded)
                    {
                        currentState = CharacterState.Fall;
                        break;
                    }

                    CheckCollisionsBottom();
                    if (bottomCollision != null)
                    {
                        slopeAngle = bottomCollision.segment.angleFromHorizontalDegrees;
                    }

                    if (onFlatGround)
                    {
                        slopeAngle = 0;
                    }
                    CheckCollisionsRight();
                    CheckCollisionsLeft();
                    if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                    {
                        velocity.x = 0;
                        velocity.y = 0;
                        ApplyHorizontalCollisions();
                        ApplyVerticalCollisions();
                        break;
                    }
                    else if (rightCollision == null && velocity.x > 0)
                    {
                        velocity.y = 0;
                        CheckCollisionsBottom();
                        ApplyVerticalCollisions();
                        if (bottomCollision != null)
                        {
                            velocity.y = bottomCollision.segment.rightPointingTangent.y * baseRunSpeed;
                            velocity.x = bottomCollision.segment.rightPointingTangent.x * baseRunSpeed;
                            ApplyVelocityToTransform();
                            grounded = true;
                        }
                    }
                    else if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                    {
                        velocity.x = 0;
                        velocity.y = 0;
                        ApplyHorizontalCollisions();
                        ApplyVerticalCollisions();
                        break;
                    }
                    else if (leftCollision == null && velocity.x < 0)
                    {
                        velocity.y = 0;
                        CheckCollisionsBottom();
                        ApplyVerticalCollisions();
                        if (bottomCollision != null)
                        {
                            velocity.y = bottomCollision.segment.leftPointingTangent.y * baseRunSpeed;
                            velocity.x = bottomCollision.segment.leftPointingTangent.x * baseRunSpeed;
                            ApplyVelocityToTransform();
                            grounded = true;
                        }
                    }
                    else if (slopeAngle <= groundAngleLimit)
                    {
                        if (onFlatGround)
                        {
                            velocity.x = baseRunSpeed * Mathf.Sign(velocity.x);
                            velocity.y = 0;
                        }
                        else
                        {
                            velocity.x = bottomCollision.segment.rightPointingTangent.x * baseRunSpeed * Mathf.Sign(velocity.x);
                            velocity.y = bottomCollision.segment.rightPointingTangent.y * baseRunSpeed;
                        }
                        CheckCollisionsTop();
                        if (topCollision != null)
                        {
                            velocity = Vector2.zero;
                        }
                        else
                        {
                            ApplyVelocityX();
                            CheckCollisionsBottom();
                            ApplyVerticalCollisions();
                        }
                        grounded = true;
                    }

                    break;

                    //Moving to the right
                    if (velocity.x > 0)
                    {
                        CheckCollisionsRight();

                        if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                        {
                            velocity.x = 0;
                            ApplyHorizontalCollisions();
                            //the move up the slope
                            velocity.y = rightCollision.segment.rightPointingTangent.y * baseRunSpeed;
                            velocity.x = rightCollision.segment.rightPointingTangent.x * baseRunSpeed;

                            CheckCollisionsTop();
                            if (topCollision == null)
                            {
                                ApplyVelocityToTransform();
                            }
                            break;
                            // ascendingSlope = true;
                            // descendingSlope = false;
                            // grounded = true;
                        }
                        else if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                        {
                            ApplyHorizontalCollisions();
                            velocity.x = 0;
                            velocity.y = 0;
                            break;
                        }
                        else if (rightCollision == null)
                        {
                            velocity.y = 0;
                            CheckCollisionsBottom();
                            ApplyVerticalCollisions();
                            if (bottomCollision != null)
                            {
                                velocity.y = bottomCollision.segment.rightPointingTangent.y * baseRunSpeed;
                                velocity.x = bottomCollision.segment.rightPointingTangent.x * baseRunSpeed;
                                ApplyVelocityToTransform();
                            }
                        }
                    }
                    else if (velocity.x < 0)
                    {
                        CheckCollisionsLeft();

                        if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                        {
                            velocity.x = 0;
                            ApplyHorizontalCollisions();
                            //the move up the slope
                            velocity.y = leftCollision.segment.leftPointingTangent.y * baseRunSpeed;
                            velocity.x = leftCollision.segment.leftPointingTangent.x * baseRunSpeed;

                            CheckCollisionsTop();
                            if (topCollision == null)
                            {
                                ApplyVelocityToTransform();
                            }
                            break;
                        }
                        else if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                        {
                            velocity.x = 0;
                            velocity.y = 0;
                            ApplyHorizontalCollisions();
                            break;
                        }
                        else if (leftCollision == null)
                        {
                            velocity.y = 0;
                            CheckCollisionsBottom();
                            ApplyVerticalCollisions();
                            if (bottomCollision != null)
                            {
                                velocity.y = bottomCollision.segment.leftPointingTangent.y * baseRunSpeed;
                                velocity.x = bottomCollision.segment.leftPointingTangent.x * baseRunSpeed;
                                ApplyVelocityToTransform();
                            }
                        }
                    }

                    break;
                }

            case (CharacterState.Jump):
                {
                    ApplyGravity();
                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                    }
                    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    {
                        velocity.x = 0;
                    }

                    if (velocity.y < 0)
                    {
                        currentState = CharacterState.Fall;
                        break;
                    }


                    if (velocity.x > 0)
                    {
                        CheckCollisionsRight();
                        if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                        {
                            currentState = CharacterState.Run;
                            break;
                        }
                        else if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                        {
                            ApplyHorizontalCollisions();
                            float speed = velocity.y;
                            velocity.x = rightCollision.segment.rightPointingTangent.x * speed;
                            velocity.y = rightCollision.segment.rightPointingTangent.y * speed;
                            CheckCollisionsTop();
                            if (topCollision == null)
                            {
                                ApplyVelocityToTransform();
                            }
                            else
                            {
                                ApplyVerticalCollisions();
                                velocity.y = 0;
                                currentState = CharacterState.Fall;
                            }
                            break;
                        }
                        else if (rightCollision == null)
                        {
                            CheckCollisionsTop();
                            if (topCollision != null)
                            {
                                ApplyVerticalCollisions();
                                velocity.y = 0;
                            }
                            ApplyVelocityToTransform();
                        }
                    }
                    else if (velocity.x < 0)
                    {
                        CheckCollisionsLeft();
                        if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                        {
                            currentState = CharacterState.Run;
                            break;
                        }
                        else if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                        {
                            ApplyHorizontalCollisions();
                            float speed = velocity.y;
                            velocity.x = leftCollision.segment.leftPointingTangent.x * speed;
                            velocity.y = leftCollision.segment.leftPointingTangent.y * speed;
                            CheckCollisionsTop();
                            if (topCollision == null)
                            {
                                ApplyVelocityToTransform();
                            }
                            else
                            {
                                ApplyVerticalCollisions();
                                velocity.y = 0;
                                currentState = CharacterState.Fall;
                            }
                            break;
                        }
                        else if (leftCollision == null)
                        {
                            CheckCollisionsTop();
                            if (topCollision != null)
                            {
                                ApplyVerticalCollisions();
                                velocity.y = 0;
                            }
                            ApplyVelocityToTransform();
                        }
                    }
                    else
                    {
                        CheckCollisionsTop();
                        if (topCollision != null)
                        {
                            velocity.y = 0;
                        }
                        ApplyVelocityToTransform();
                    }

                    break;
                }

            case (CharacterState.Fall):
                {
                    ApplyGravity();

                    if (Input.GetKey(KeyCode.A))
                    {
                        velocity.x = -1 * baseRunSpeed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        velocity.x = baseRunSpeed;
                    }
                    if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    {
                        velocity.x = 0;
                    }

                    if (velocity.y > 0)
                    {
                        currentState = CharacterState.Jump;
                        break;
                    }

                    CheckCollisionsLeft();
                    CheckCollisionsRight();
                    if (rightCollision != null || leftCollision != null)
                        velocity.x = 0;

                    CheckCollisionsBottom();
                    if (bottomCollision != null && bottomCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                    {
                        ApplyVerticalCollisions();
                        currentState = CharacterState.Idle;
                        grounded = true;
                    }
                    else if (bottomCollision != null && bottomCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                    {

                        velocity.x = bottomCollision.segment.downPointingTangent.x * Mathf.Abs(velocity.y);
                        velocity.y = bottomCollision.segment.downPointingTangent.y * Mathf.Abs(velocity.y);
                        ApplyVelocityY();
                        CheckCollisionsLeft();
                        CheckCollisionsRight();

                        if (rightCollision != null || leftCollision != null)
                            break;
                        ApplyHorizontalCollisions();
                    }
                    else if (bottomCollision == null)
                    {
                        ApplyVelocityY();
                        ApplyVelocityX();
                    }
                    break;
                    if (bottomCollision != null && bottomCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                    {
                        ApplyVerticalCollisions();
                        if (grounded)
                        {
                            currentState = CharacterState.Run;
                        }
                        break;
                    }
                    else if (bottomCollision != null && bottomCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                    {
                        ApplyHorizontalCollisions();
                        float speed = Mathf.Abs(velocity.y);
                        velocity.x = bottomCollision.segment.downPointingTangent.x * speed;
                        velocity.y = bottomCollision.segment.downPointingTangent.y * speed;
                        if (velocity.x > 0)
                        {
                            CheckCollisionsRight();
                            if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                            {
                                grounded = true;
                                currentState = CharacterState.Run;
                                break;
                            }
                            else if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                            {
                                ApplyHorizontalCollisions();
                                velocity = Vector2.zero;
                                break;
                            }
                            else if (rightCollision == null)
                            {
                                ApplyVelocityToTransform();
                            }
                        }
                        else if (velocity.x < 0)
                        {
                            CheckCollisionsLeft();
                            if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                            {
                                grounded = true;
                                currentState = CharacterState.Run;
                                break;
                            }
                            else if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                            {
                                ApplyHorizontalCollisions();
                                velocity = Vector2.zero;
                                break;
                            }
                            else if (leftCollision == null)
                            {
                                ApplyVelocityToTransform();
                            }
                        }
                    }
                    else if (bottomCollision == null)
                    {
                        if (velocity.x > 0)
                        {
                            CheckCollisionsRight();
                            if (rightCollision != null)
                            {
                                ApplyHorizontalCollisions();
                                velocity.x = 0;
                            }
                            ApplyVelocityToTransform();
                        }
                        else if (velocity.x < 0)
                        {
                            CheckCollisionsLeft();
                            if (leftCollision != null)
                            {
                                ApplyHorizontalCollisions();
                                velocity.x = 0;
                            }
                            ApplyVelocityToTransform();
                        }
                        else
                        {
                            ApplyVelocityToTransform();

                        }
                    }

                    break;
                }
        }

        trueVelocity = (new Vector2(transform.position.x, transform.position.y) - prevPosition) / Time.deltaTime;
        UpdateCollisionRect();
    }

    public void NewUpdate()
    {
        DrawBoundingRect();
        previousSlopeAngle = slopeAngle;
        prevAscendingSlope = ascendingSlope;
        prevDescendingSlope = descendingSlope;
        prevPosition = transform.position;

        if (Input.GetKey(KeyCode.R))
        {
            transform.position = new Vector2(-14.65f, 5.54f);
            velocity = Vector2.zero;
        }

        switch (currentState)
        {
            default:
            case (CharacterState.Idle):
                CheckCollisionsBottom();
                CheckCollisionsTop();
                ApplyVelocityY();
                ApplyVerticalCollisions();
                ascendingSlope = false;
                descendingSlope = false;


                if (Input.GetKey(KeyCode.A))
                {
                    velocity.x = -1 * baseRunSpeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    velocity.x = baseRunSpeed;
                }
                if (Input.GetKeyDown(KeyCode.Space) && grounded)
                {
                    velocity.y = baseJumpVelocity;
                    ascendingSlope = false;
                    descendingSlope = false;
                    grounded = false;
                }


                if (velocity.x != 0)
                    currentState = CharacterState.Run;
                if (velocity.y > 0)
                    currentState = CharacterState.Jump;
                else if (velocity.y < 0)
                    currentState = CharacterState.Fall;
                else if (!grounded)
                    currentState = CharacterState.Fall;

                break;

            case (CharacterState.Run):
                CheckCollisionsBottom();
                CheckCollisionsTop();
                ApplyVelocityY();
                ApplyVerticalCollisions();

                if (Input.GetKey(KeyCode.A))
                {
                    velocity.x = -1 * baseRunSpeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    velocity.x = baseRunSpeed;
                }

                CheckCollisionsRight();
                CheckCollisionsLeft();

                //This is a really weird edge case where a player quickly switches from
                //ascending a slope to descending. It would originally sometimes make the player just fall
                //off the slope, not descending it.
                // if (rightCollision == null && leftCollision == null && bottomCollision == null)
                // {
                //     velocity.y = -10;
                //     CheckCollisionsBottom();
                //     ApplyVelocityY();
                //     ApplyVerticalCollisions();
                // }

                //no LR collision:
                if (rightCollision == null && leftCollision == null && bottomCollision != null)
                {
                    if (prevAscendingSlope && bottomCollision.segment.angleFromHorizontalDegrees != 0)
                    {
                        grounded = true;
                    }
                    //moving on ground, could be descending a slope
                    slopeAngle = bottomCollision.segment.angleFromHorizontalDegrees;
                    Vector2 tangent = bottomCollision.segment.rightPointingTangent;
                    if (velocity.x < 0)
                    {
                        tangent = bottomCollision.segment.leftPointingTangent;
                    }
                    float moveDistance = Mathf.Abs(baseRunSpeed);
                    float descendVelocityY = tangent.y * moveDistance;
                    velocity.y = descendVelocityY;
                    velocity.x = tangent.x * moveDistance;
                    ascendingSlope = false;
                    descendingSlope = true;
                    grounded = true;
                    // if (velocity.y != 0 && velocity.y > descendVelocityY)
                    // {
                    //     ascendingSlope = false;
                    //     descendingSlope = false;
                    //     grounded = false;
                    // }
                    // else
                    // {
                    //     velocity.y = descendVelocityY;
                    //     velocity.x = tangent.x * moveDistance;
                    //     ascendingSlope = false;
                    //     descendingSlope = true;
                    //     grounded = true;
                    // }

                    if (bottomCollision.segment.angleFromHorizontalDegrees == 0)
                    {
                        ascendingSlope = false;
                        descendingSlope = false;
                    }

                    ApplyVelocityX();
                }
                //Right collision with climbable slope
                else if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    //if we dont hit a cieling
                    if (topCollision == null)
                    {
                        if (!ascendingSlope)
                        {
                            Debug.Log("www");
                        }
                        ApplyHorizontalCollisions();
                        transform.position = new Vector2(transform.position.x, Mathf.Max(transform.position.y + yOffset, rightCollision.collisionPosition.y) - yOffset);

                        //the move up the slope
                        float ascendVelocityY = rightCollision.segment.rightPointingTangent.y * baseRunSpeed;
                        //if our Y velocity is really high (we are jumping), then jump and dont climb the slope
                        if (velocity.y <= ascendVelocityY)
                        {
                            velocity.y = ascendVelocityY;
                            velocity.x = rightCollision.segment.rightPointingTangent.x * baseRunSpeed;
                            ascendingSlope = true;
                            descendingSlope = false;
                            grounded = true;
                        }
                        else
                        {
                            ascendingSlope = false;
                            grounded = false;
                        }
                    }
                    ApplyVelocityX();
                }

                //Left collision with climbable slope
                else if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    //if we dont hit a cieling
                    if (topCollision == null)
                    {
                        if (!ascendingSlope)
                        {
                            Debug.Log("www");
                        }
                        ApplyHorizontalCollisions();
                        transform.position = new Vector2(transform.position.x, Mathf.Max(transform.position.y + yOffset, leftCollision.collisionPosition.y) - yOffset);

                        //the move up the slope
                        float ascendVelocityY = leftCollision.segment.leftPointingTangent.y * baseRunSpeed;
                        //if our Y velocity is really high (we are jumping), then jump and dont climb the slope
                        if (velocity.y <= ascendVelocityY)
                        {
                            velocity.y = ascendVelocityY;
                            velocity.x = leftCollision.segment.leftPointingTangent.x * baseRunSpeed;
                            ascendingSlope = true;
                            descendingSlope = false;
                            grounded = true;
                        }
                        else
                        {
                            ascendingSlope = false;
                            grounded = false;
                        }
                    }
                    ApplyVelocityX();
                }

                //Right collision with solid wall
                else if (rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    if (prevAscendingSlope || prevDescendingSlope)
                    {
                        velocity.y = 0;
                    }
                    ApplyVelocityX();
                    ApplyHorizontalCollisions();
                }
                //Left collision with solid wall
                else if (leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    if (prevAscendingSlope || prevDescendingSlope)
                        velocity.y = 0;
                    ApplyVelocityX();
                    ApplyHorizontalCollisions();
                }


                if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
                {
                    velocity.x = 0;
                    ascendingSlope = false;
                    descendingSlope = false;
                    if (ascendingSlope || prevAscendingSlope || descendingSlope || prevDescendingSlope)
                    {
                        velocity.y = 0;
                    }
                    currentState = CharacterState.Idle;
                }
                if (Input.GetKeyDown(KeyCode.Space) && grounded)
                {
                    velocity.y = baseJumpVelocity;
                    ascendingSlope = false;
                    descendingSlope = false;
                    grounded = false;
                }

                if (rightCollision == null && leftCollision == null && bottomCollision == null)
                {
                    Debug.Log("STOPPED ASC AND STARTED DEC");
                }

                if (velocity.y > 0 && !grounded)
                {
                    currentState = CharacterState.Jump;
                }
                else if (velocity.y < 0 && !grounded)
                {
                    currentState = CharacterState.Fall;
                }

                if (prevAscendingSlope && !ascendingSlope)
                {
                    Debug.Log("STOPPED ASCENDING");
                }

                if (prevDescendingSlope && !descendingSlope)
                {
                    Debug.Log("STOPPED DESCENDING");
                }

                if (descendingSlope && prevAscendingSlope)
                {
                    Debug.Log("STOPPED ASC AND STARTED DEC");
                }

                break;

            case (CharacterState.Jump):
                ApplyGravity();
                CheckCollisionsBottom();
                CheckCollisionsTop();
                ApplyVelocityY();
                ApplyVerticalCollisions();
                ascendingSlope = false;
                descendingSlope = false;

                if (Input.GetKey(KeyCode.A))
                {
                    velocity.x = -1 * baseRunSpeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    velocity.x = baseRunSpeed;
                }

                CheckCollisionsRight();
                CheckCollisionsLeft();
                ApplyVelocityX();
                ApplyHorizontalCollisions();


                if (velocity.y < 0)
                    currentState = CharacterState.Fall;
                if (grounded)
                    currentState = CharacterState.Idle;
                break;

            case (CharacterState.Fall):
                ApplyGravity();
                CheckCollisionsBottom();
                CheckCollisionsTop();
                ApplyVelocityY();
                ApplyVerticalCollisions();
                ascendingSlope = false;
                descendingSlope = false;

                if (Input.GetKey(KeyCode.A))
                {
                    velocity.x = -1 * baseRunSpeed;
                }
                if (Input.GetKey(KeyCode.D))
                {
                    velocity.x = baseRunSpeed;
                }

                CheckCollisionsRight();
                CheckCollisionsLeft();
                ApplyVelocityX();
                ApplyHorizontalCollisions();

                if (velocity.y > 0)
                    currentState = CharacterState.Jump;
                if (grounded)
                    currentState = CharacterState.Idle;
                break;
        }

        UpdateCollisionRect();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAugust();
        //NewNewNewUpdate();
        return;
        DrawBoundingRect();
        ApplyGravity();

        previousSlopeAngle = slopeAngle;
        prevAscendingSlope = ascendingSlope;
        prevDescendingSlope = descendingSlope;

        if (Input.GetKey(KeyCode.A))
        {
            velocity.x = -1 * baseRunSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity.x = baseRunSpeed;
        }

        if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        {
            velocity.x = 0;
            if (ascendingSlope || prevAscendingSlope)
            {
                velocity.y = 0;
            }
        }

        // if (descendingSlope && prevAscendingSlope)
        //     velocity.y = 0;

        if (prevAscendingSlope && descendingSlope && grounded)
        {
            Debug.Log("grounded");
        }
        else if (prevAscendingSlope && descendingSlope && !grounded)
        {
            Debug.Log("NOT grounded");
        }

        if (Input.GetKey(KeyCode.R))
        {
            transform.position = new Vector2(-14.65f, 5.54f);
            velocity = Vector2.zero;
        }

        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            velocity.y = baseJumpVelocity;
            ascendingSlope = false;
            descendingSlope = false;
            grounded = false;
        }

        CheckCollisionsBottom();
        CheckCollisionsTop();
        ApplyVelocityY();

        if (grounded)
        {
            velocity.y = Mathf.Max(velocity.y, 0);
        }
        else
        {
            descendingSlope = false;
            ascendingSlope = false;
        }

        if (bottomCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Max(transform.position.y + yOffset, bottomCollision.collisionPosition.y) - yOffset);
        }

        if (topCollision != null)
        {
            transform.position = new Vector2(transform.position.x, Mathf.Min(transform.position.y + height + yOffset, topCollision.collisionPosition.y) - height - yOffset);
        }

        CheckCollisionsLeft();
        CheckCollisionsRight();


        if (rightCollision != null)
        {
            slopeAngle = rightCollision.segment.angleFromHorizontalDegrees;
            if (slopeAngle <= groundAngleLimit && topCollision == null)
            {
                if (topCollision == null)
                {
                    float ascendVelocityY = rightCollision.segment.rightPointingTangent.y * baseRunSpeed;
                    if (velocity.y <= ascendVelocityY)
                    {
                        velocity.y = ascendVelocityY;
                        velocity.x = rightCollision.segment.rightPointingTangent.x * baseRunSpeed;
                        ascendingSlope = true;
                        descendingSlope = false;
                        grounded = true;
                    }
                    else
                    {
                        ascendingSlope = false;
                        grounded = false;
                    }
                }
                ApplyVelocityX();
            }
            else
            {
                Debug.DrawLine(transform.position, rightCollision.collisionPosition, Color.magenta);
                if (prevAscendingSlope)
                    velocity.y = 0;
                transform.position = new Vector2(Mathf.Min(transform.position.x + width + xOffset, rightCollision.collisionPosition.x) - width - xOffset, transform.position.y);
                ascendingSlope = false;
            }
        }

        else if (leftCollision != null)
        {
            slopeAngle = leftCollision.segment.angleFromHorizontalDegrees;
            if (slopeAngle <= groundAngleLimit && topCollision == null)
            {
                if (topCollision == null)
                {
                    float ascendVelocityY = leftCollision.segment.leftPointingTangent.y * baseRunSpeed;
                    if (velocity.y <= ascendVelocityY)
                    {
                        velocity.y = ascendVelocityY;
                        velocity.x = leftCollision.segment.leftPointingTangent.x * baseRunSpeed;
                        ascendingSlope = true;
                        descendingSlope = false;
                        grounded = true;
                    }
                    else
                    {
                        ascendingSlope = false;
                        grounded = false;
                    }
                }
                ApplyVelocityX();
            }
            else
            {
                if (prevAscendingSlope)
                    velocity.y = 0;
                transform.position = new Vector2(Mathf.Max(transform.position.x + xOffset, leftCollision.collisionPosition.x) - xOffset, transform.position.y);
                ascendingSlope = false;
            }
        }

        //moving with no walls to the side of us
        else if (rightCollision == null && leftCollision == null && velocity.x != 0 && bottomCollision != null)
        {
            if (prevAscendingSlope)
            {
                velocity.y = 0;
                grounded = true;
            }
            //moving on ground, could be descending a slope
            slopeAngle = bottomCollision.segment.angleFromHorizontalDegrees;
            Vector2 tangent = bottomCollision.segment.rightPointingTangent;
            if (velocity.x < 0)
            {
                tangent = bottomCollision.segment.leftPointingTangent;
            }
            float moveDistance = Mathf.Abs(baseRunSpeed);
            float descendVelocityY = tangent.y * moveDistance;
            if (velocity.y != 0 && velocity.y > descendVelocityY)
            {
                ascendingSlope = false;
                descendingSlope = false;
                grounded = false;
            }
            else
            {
                velocity.y = descendVelocityY;
                velocity.x = tangent.x * moveDistance;
                ascendingSlope = false;
                descendingSlope = true;
                grounded = true;
            }
            ApplyVelocityX();
        }
        else
        {
            ApplyVelocityX();
        }



        UpdateCollisionRect();
    }
}



// switch (currentState)
// {
//     default:
//     case CharacterState.Idle:
//         CheckForCollisionsVertical();
//         ApplyCollisionsVertical();
//         ApplyVelocityToTransform();
//         if (Input.GetKey(KeyCode.A))
//         {
//             currentState = CharacterState.Run;
//             //facing left = false;
//         }

//         if (Input.GetKey(KeyCode.D))
//         {
//             currentState = CharacterState.Run;
//             //facing left = true;
//         }

//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             SetVelocityRaw(new Vector2(velocity.x, 10));
//             grounded = false;
//         }

//         if (!grounded)
//         {
//             if (velocity.y > 0)
//                 currentState = CharacterState.Jump;
//             else if (velocity.y < 0)
//                 currentState = CharacterState.Fall;
//         }
//         break;
//     case CharacterState.Run:
//         //set velocity based on slope angle
//         if (Input.GetKey(KeyCode.A))
//         {
//             velocity.x = -1 * baseRunSpeed;

//             // if(leftCollision != null){
//             //     if(Mathf.Abs(leftCollision.collisionPosition.y -  transform.position.y) < stepHeight
//             //     && leftCollision.segment.angleFromHorizontalDegrees < groundAngleLimit){
//             //         SetVelocityRaw(baseRunSpeed * leftCollision.segment.leftPointingTangent);
//             //     }
//             // }
//             // else if (bottomCollision.segment.angleFromHorizontalDegrees < groundAngleLimit)
//             // {
//             //     SetVelocityRaw(baseRunSpeed * bottomCollision.segment.leftPointingTangent);
//             // }
//             //facing left = false;
//         }
//         if (Input.GetKey(KeyCode.D))
//         {
//             velocity.x = baseRunSpeed;
//             // if (rightCollision != null)
//             // {
//             //     if (Mathf.Abs(rightCollision.collisionPosition.y - transform.position.y) < stepHeight
//             //     && rightCollision.segment.angleFromHorizontalDegrees < groundAngleLimit)
//             //     {
//             //         SetVelocityRaw(baseRunSpeed * rightCollision.segment.leftPointingTangent);
//             //     }
//             // }
//             // //going to the right
//             // else if (bottomCollision.segment.angleFromHorizontalDegrees < groundAngleLimit)
//             //     SetVelocityRaw(baseRunSpeed * bottomCollision.segment.rightPointingTangent);
//             //facing left = true;
//         }
//         CheckForCollisionsHorizontal();
//         ApplyCollisions();
//         ApplyVelocityToTransform();

//         if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
//         {
//             velocity.x = 0;
//             currentState = CharacterState.Idle;
//         }

//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             SetVelocityRaw(new Vector2(velocity.x, 10));
//             grounded = false;
//         }

//         if (!grounded)
//         {
//             if (velocity.y > 0)
//                 currentState = CharacterState.Jump;
//             else if (velocity.y < 0)
//                 currentState = CharacterState.Fall;
//         }

//         break;

//     case CharacterState.Jump:
//         CheckForCollisionsVertical();
//         ApplyCollisionsVertical();
//         ApplyVelocityToTransform();
//         if (velocity.y < 0)
//             currentState = CharacterState.Fall;

//         if (grounded)
//             currentState = CharacterState.Idle;
//         break;
//     case CharacterState.Fall:
//         CheckForCollisionsVertical();
//         ApplyCollisionsVertical();
//         ApplyVelocityToTransform();
//         if (velocity.y > 0)
//             currentState = CharacterState.Jump;

//         if (grounded)
//             currentState = CharacterState.Idle;
//         break;
// }