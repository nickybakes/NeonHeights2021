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

    /// <summary>
    /// Amount of time the player is in the air
    /// </summary>
    public float timeInAir;

    /// <summary>
    /// Amount of time the player can still jump while in the air
    /// </summary>
    public float roadRunnerTimeMax = .13f;

    public bool holdingJump = false;

    public float jumpHoldTimer;

    public float jumpHoldTimerMax = .5f;

    /// <summary>
    /// Amount of times we have jumped in the air. Reset to 0 when grounded
    /// </summary>
    public int jumpsInAir;

    /// <summary>
    /// Amount of times the player is allowed to jump in the air
    /// 2 means 1 jump from the ground, then 2 in the air, so 3 in total
    /// </summary>
    public int jumpsInAirMax = 2;


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
            //Debug.Log("SAME THINGGG ");
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
                    //Debug.Log("STEP RIGHT");

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
                        //Debug.Log("a");
                        //ApplyHorizontalCollisions();
                    }
                    else
                    {
                        if (!onFlatGround)
                        {
                            //Debug.Log("b");
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
                            //Debug.Log("c");
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
                    //Debug.Log("d");
                    // velocity.y = groundCollision.segment.downPointingTangent.y * 50;
                    // velocity.x = groundCollision.segment.downPointingTangent.x * 50;
                    // ApplyVelocityToTransform();
                }
                else if (!grounded && rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    ApplyHorizontalCollisions();
                    ApplyVerticalCollisions();
                    //Debug.Log("e");
                }
                else if (!grounded && rightCollision != null && rightCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    // ApplyHorizontalCollisions();
                    grounded = false;
                    //Debug.Log("f");
                }
                else if (!grounded && rightCollision == null)
                {
                    //Debug.Log("g");
                    ApplyVelocityX();
                }
            }
            //moving to the left
            else if (velocity.x < 0)
            {
                if (leftCollision != null && leftCollision.segment.topVertex.y < transform.position.y + yOffset + stepHeight && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit && transform.position.y < leftCollision.segment.topVertex.y && groundCollision != null)
                {
                    //Debug.Log("STEP LEFT");
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
                        //Debug.Log("A");
                        //ApplyHorizontalCollisions();
                    }
                    else
                    {
                        if (!onFlatGround)
                        {
                            //Debug.Log("B");
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
                            //Debug.Log("C");
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
                    //Debug.Log("D");
                    // velocity.y = groundCollision.segment.downPointingTangent.y * 50;
                    // velocity.x = groundCollision.segment.downPointingTangent.x * 50;
                    // ApplyVelocityToTransform();
                }
                else if (!grounded && leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees <= groundAngleLimit)
                {
                    //Debug.Log("E");
                    ApplyHorizontalCollisions();
                    ApplyVerticalCollisions();
                }
                else if (!grounded && leftCollision != null && leftCollision.segment.angleFromHorizontalDegrees > groundAngleLimit)
                {
                    //Debug.Log("F");
                    grounded = false;
                    //ApplyHorizontalCollisions();
                }
                else if (!grounded && leftCollision == null)
                {
                    //Debug.Log("G");
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

        if (!slippingDownSlope && !holdingJump)
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


        //keeping track of how much time is spent in the air for "road runner time"
        if (!grounded)
        {
            timeInAir += Time.deltaTime;
        }
        else
        {
            //when grounded, reset these values
            timeInAir = 0;
            jumpsInAir = 0;
        }


        //jumping from the ground
        if (Input.GetKeyDown(KeyCode.Space) && (grounded || (!grounded && timeInAir < roadRunnerTimeMax)) && topCollision == null)
        {
            jumpHoldTimer = 0;
            grounded = false;
            holdingJump = true;
            timeInAir = roadRunnerTimeMax;
        }
        //"double" jumping in the air
        else if (Input.GetKeyDown(KeyCode.Space) && !grounded && topCollision == null && jumpsInAir < jumpsInAirMax)
        {
            jumpHoldTimer = 0;
            grounded = false;
            holdingJump = true;
            jumpsInAir++;
        }

        //hold the jump button to jump higher
        if (holdingJump && jumpHoldTimer < jumpHoldTimerMax)
        {
            velocity.y = baseJumpVelocity;
            jumpHoldTimer += Time.deltaTime;
        }

        //limit how long the button can be held for extra hight
        if (jumpHoldTimer >= jumpHoldTimerMax || Input.GetKeyUp(KeyCode.Space) || grounded)
        {
            holdingJump = false;
        }

        //reset the position of the player in the test map
        if (Input.GetKey(KeyCode.R))
        {
            transform.position = new Vector2(-14.65f, 5.54f);
            velocity = Vector2.zero;
        }

        UpdateCollisionRect();
    }



    

    // Update is called once per frame
    void Update()
    {
        UpdateAugust();
        //NewNewNewUpdate();
        return;
    }
}



