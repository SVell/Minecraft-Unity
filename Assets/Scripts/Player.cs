using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Transform cam;
    private World world;
    
    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public bool isGrounded;
    public bool isSprinting;
    
    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float mouseSensitivity = 3;

    public float playerWidth = 0.3f;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8;

    public Toolbar toolbar;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            world.inUI = !world.inUI;
        }
        
        if (!world.inUI)
        {
            GetPlayerInput();
            PlaceCursorBlock();
        }
    }

    private void FixedUpdate()
    {
        if (!world.inUI)
        {
            CalculateVelocity();
        
            if (jumpRequest)
            {
                Jump();
            }
        
            cam.Rotate(Vector3.right * (-mouseVertical * mouseSensitivity));
            transform.Rotate(Vector3.up * (mouseHorizontal * mouseSensitivity));
            transform.Translate(velocity, Space.World);
        }
    }

    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private void CalculateVelocity()
    {
        // Affect vertical momentum with gravity
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.fixedDeltaTime * gravity;
        }
        
        // If we're sprinting, use the sprint multiplier
        if (isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;
        }
        
        // Apply vertical momentum
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
        }

        if (velocity.y < 0)
        {
            velocity.y = CheckDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = CheckUpSpeed(velocity.y);
        }
    }
    
    private void GetPlayerInput()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }

        if (highlightBlock.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position,0);
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (toolbar.slots[toolbar.slotIndex].HasItem)
                {
                    world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position,toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
                    toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
                }
                    
            }
        }
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x),Mathf.FloorToInt(pos.y),Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                //placeBlock.gameObject.SetActive(true);

                return;
            }
            
            lastPos = new Vector3(Mathf.FloorToInt(pos.x),Mathf.FloorToInt(pos.y),Mathf.FloorToInt(pos.z));

            step += checkIncrement;
        }
        
        highlightBlock.gameObject.SetActive(false);
        //placeBlock.gameObject.SetActive(false);
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) || 
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) || 
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) || 
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
        )
        {
            isGrounded = true;
            return 0;
        }

        isGrounded = false;
        return downSpeed;
    }
    
    private float CheckUpSpeed(float upSpeed)
    {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) || 
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) || 
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) || 
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
        )
        {
            return 0;
        }
        
        return upSpeed;
    }

    public bool front
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x,transform.position.y,transform.position.z + playerWidth )) || 
                world.CheckForVoxel(new Vector3(transform.position.x,transform.position.y + 1f,transform.position.z + playerWidth ))
                )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public bool back
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x,transform.position.y,transform.position.z - playerWidth )) || 
                world.CheckForVoxel(new Vector3(transform.position.x,transform.position.y + 1f,transform.position.z - playerWidth ))
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public bool right
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth ,transform.position.y,transform.position.z)) || 
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth ,transform.position.y + 1f,transform.position.z))
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public bool left
    {
        get
        {
            if(
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth ,transform.position.y,transform.position.z)) || 
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth ,transform.position.y + 1f,transform.position.z))
            )
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
