using System;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    CharacterController2D characterController;
    bool isFired = false;
    Dictionary<int, Vector3> positions = new Dictionary<int, Vector3>();
    ContactFilter2D contactFilter;

    GameObject localPlayerRay;
    GameObject localHit;
    bool showRay = false;
    int rayDuration = 3;
    float rayTime = 0;
    float maxRaycastDistance = 10;
    float raycastOffset = 0.1f;

    float movementSpeed = 1.5f;
    float rotationSpeed = 60f;

    void Awake()
    {
        characterController = GetComponent<CharacterController2D>();
        contactFilter = new ContactFilter2D() { layerMask = LayerMask.GetMask("ClientPlayer"), useTriggers = true, useLayerMask = true };

        var localPlayerRayPrefab = Resources.Load<GameObject>("LocalPlayerRay");
        localPlayerRay = GameObject.Instantiate(localPlayerRayPrefab);
        localPlayerRay.SetActive(false);
        Vector3 scale = localPlayerRay.transform.localScale;
        scale.y = maxRaycastDistance;
        localPlayerRay.transform.localScale = scale;

        var localHitPrefab = Resources.Load<GameObject>("LocalHit");
        localHit = GameObject.Instantiate(localHitPrefab);
        localHit.SetActive(false);
    }

    void CheckForHit()
    {
        float halfHeight = maxRaycastDistance / 2;
        Vector3 direction = Utils.QuaternionToDirection(transform.rotation);
        Vector3 circleOffset = ((transform.lossyScale.x / 2) + raycastOffset) * direction;
        Vector3 origin = transform.position + halfHeight * direction + circleOffset;

        localPlayerRay.transform.rotation = transform.rotation;
        localPlayerRay.transform.position = origin;
        localPlayerRay.SetActive(true);

        showRay = true;
        rayTime = 0;

        localHit.SetActive(false);
        RaycastHit2D[] raycastHits = new RaycastHit2D[1];
        if (Physics2D.Raycast(transform.position + circleOffset, direction, contactFilter, raycastHits, maxRaycastDistance) != 0)
        {
            localHit.transform.position = raycastHits[0].point;
            localHit.SetActive(true);
        }
    }

    void RemoveGunRayAndHit()
    {
        localPlayerRay.SetActive(false);
        localHit.SetActive(false);
        showRay = false;
    }

    private void Update()
    {
        transform.Rotate(-Vector3.forward * Convert.ToInt32(Input.GetKey(KeyCode.E)) * Time.deltaTime * rotationSpeed);
        transform.Rotate(Vector3.forward * Convert.ToInt32(Input.GetKey(KeyCode.Q)) * Time.deltaTime * rotationSpeed);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            isFired = true;
            CheckForHit();
        }

        if(showRay)
        {
            rayTime += Time.deltaTime;
            if(rayTime >= rayDuration)
            {
                RemoveGunRayAndHit();
            }
        }
    }

    public void ApplyMovement(Buttons buttons, float deltaTime) // TODO change name
    {
        if (buttons.Left)
        {
            characterController.move(Vector3.left * deltaTime * movementSpeed);
        }
        if (buttons.Up)
        {
            characterController.move(Vector3.up * deltaTime * movementSpeed);
        }
        if (buttons.Right)
        {
            characterController.move(Vector3.right * deltaTime * movementSpeed);
        }
        if (buttons.Down)
        {
            characterController.move(Vector3.down * deltaTime * movementSpeed);
        }

        positions[Client.instance.LocalTick] = transform.position;
    }

    public void ValidatePosition(Vector3 position, int tick)
    {
        if(positions.TryGetValue(tick, out Vector3 localPosition))
        {
            if (!position.IsEqual(localPosition, 0.10f))
            {
                Debug.Log("Something went wrong");
            }

            positions.RemoveLowerThanAnd(tick);
        }
    }

    public UserCommand GetUserCommand()
    {
        var userCommand = new UserCommand()
        {
            Buttons = new Buttons()
            {
                Up = Input.GetKey(KeyCode.W),
                Down = Input.GetKey(KeyCode.S),
                Left = Input.GetKey(KeyCode.A),
                Right = Input.GetKey(KeyCode.D),
                Fire = isFired
            },
            Rotation = transform.rotation,
        };


        isFired = false;

        return userCommand;
    }

    public void ApplyPosition(Vector3 position)
    {
        transform.position = position;
    }
}
