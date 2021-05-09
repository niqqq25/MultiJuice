using System;
using System.Collections.Generic;
using UnityEngine;

public struct Collider
{
    public Vector3 PrvsPosition;
    public Transform Object;
}

public class ServerPlayer : MonoBehaviour
{
    public int PlayerId { get; set; }

    CharacterController2D characterController;
    GameObject serverPlayerRay;
    GameObject serverHit;
    GameObject advancedServerRaycast;
    GameObject rayPlayer;

    float speed = 1.5f;

    bool showRay = false;
    int rayDuration = 6;
    float rayTime = 0;
    float maxRaycastDistance = 10;
    float raycastOffset = 0.1f;
    ContactFilter2D contactFilter;

    public Vector3 Velocity { get; set; } // used for fake players

    public void Initialize(bool fakePlayer = false)
    {
        characterController = GetComponent<CharacterController2D>();

        if(!fakePlayer)
        {
            contactFilter = new ContactFilter2D() { layerMask = LayerMask.GetMask("ServerPlayer"), useTriggers = true, useLayerMask = true };

            var serverPlayerRayPrefab = Resources.Load<GameObject>("ServerPlayerRay");
            serverPlayerRay = GameObject.Instantiate(serverPlayerRayPrefab);
            Vector3 scale = serverPlayerRay.transform.localScale;
            scale.y = maxRaycastDistance;
            serverPlayerRay.transform.localScale = scale;
            serverPlayerRay.SetActive(false);

            var serverHitPrefab = Resources.Load<GameObject>("ServerHit");
            serverHit = GameObject.Instantiate(serverHitPrefab);
            serverHit.SetActive(false);

            var advancedServerRaycastPrefab = Resources.Load<GameObject>("AdvancedServerRaycast");
            advancedServerRaycast = GameObject.Instantiate(advancedServerRaycastPrefab);
            scale = advancedServerRaycast.transform.localScale;
            scale.y = maxRaycastDistance;
            advancedServerRaycast.transform.localScale = scale;
            advancedServerRaycast.SetActive(false);

            var rayPlayerPrefab = Resources.Load<GameObject>("RayPlayer");
            rayPlayer = GameObject.Instantiate(rayPlayerPrefab);
            rayPlayer.SetActive(false);
        }
    }

    void Update()
    {
        if (showRay)
        {
            rayTime += Time.deltaTime;
            if (rayTime >= rayDuration)
            {
                RemoveGunRayAndHit();
            }
        }
    }

    void RemoveGunRayAndHit()
    {
        serverPlayerRay.SetActive(false);
        serverHit.SetActive(false);
        advancedServerRaycast.SetActive(false);
        rayPlayer.SetActive(false);
        showRay = false;
    }

    public PlayerState ToPlayerState()
    {
        return new PlayerState()
        {
            PlayerId = PlayerId,
            Position = transform.position,
            Rotation = transform.rotation
        };
    }

    public void ApplyMovement(Buttons buttons, float deltaTime)
    {
        if (buttons.Left)
        {
            characterController.move(Vector3.left * deltaTime * speed);
        }
        if (buttons.Up)
        {
            characterController.move(Vector3.up * deltaTime * speed);
        }
        if (buttons.Right)
        {
            characterController.move(Vector3.right * deltaTime * speed);
        }
        if (buttons.Down)
        {
            characterController.move(Vector3.down * deltaTime * speed);
        }
    }

    public void ApplyRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    public void CheckForHitSimple()
    {
        float halfHeight = maxRaycastDistance / 2;
        Vector3 direction = Utils.QuaternionToDirection(transform.rotation);
        Vector3 circleOffset = ((transform.lossyScale.x / 2) + raycastOffset) * direction;
        Vector3 origin = transform.position + halfHeight * direction + circleOffset;

        serverPlayerRay.transform.rotation = transform.rotation;
        serverPlayerRay.transform.position = origin;
        serverPlayerRay.SetActive(true);

        showRay = true;
        rayTime = 0;

        serverHit.SetActive(false);
        RaycastHit2D[] raycastHits = new RaycastHit2D[10];
        if (Physics2D.Raycast(transform.position + circleOffset, direction, contactFilter, raycastHits, maxRaycastDistance) != 0)
        {
            foreach (var raycastHit in raycastHits)
            {
                if (raycastHit.collider == null) continue;

                serverHit.transform.position = raycastHit.point;
                serverHit.SetActive(true);
                break;
            }
        }
    }

    public void CheckForHit(Snapshot snapshot, int tickDiff)
    {
        float halfHeight = maxRaycastDistance / 2;
        Vector3 direction = Utils.QuaternionToDirection(transform.rotation);
        Vector3 circleOffset = ((transform.lossyScale.x / 2) + raycastOffset) * direction;
        Vector3 origin = transform.position + halfHeight * direction + circleOffset;

        serverPlayerRay.transform.rotation = transform.rotation;
        serverPlayerRay.transform.position = origin;
        serverPlayerRay.SetActive(true);

        showRay = true;
        rayTime = 0;

        // Overwatch raycast

        float boxWidth = tickDiff / 2.5f;
        if(GameConfig.showServerRay)
        {
            advancedServerRaycast.SetActive(true);
            var scale = advancedServerRaycast.transform.localScale;
            scale.x = boxWidth;
            advancedServerRaycast.transform.localScale = scale;
            advancedServerRaycast.transform.rotation = transform.rotation;
            advancedServerRaycast.transform.position = origin;
        }

        var angle = transform.rotation.eulerAngles.z;
        RaycastHit2D[] raycasts = Physics2D.BoxCastAll(origin, new Vector2(boxWidth, 10), angle, direction, 0, LayerMask.GetMask("ServerPlayer"));

        HashSet<Collider> savedPositions = new HashSet<Collider>();
        foreach (var raycast in raycasts)
        {
            try
            {
                var playerState = Array.Find(snapshot.PlayerStates, playerState => playerState.PlayerId.ToString() == raycast.collider.name);
                savedPositions.Add(new Collider() {
                    PrvsPosition = raycast.transform.position,
                    Object = raycast.transform
                });

                raycast.transform.position = playerState.Position;

                if (GameConfig.showServerRay)
                {
                    rayPlayer.SetActive(true);
                    rayPlayer.transform.position = playerState.Position;
                }

                } catch
            { }
        }

        Physics2D.SyncTransforms();

        serverHit.SetActive(false);
        RaycastHit2D[] raycastHits = new RaycastHit2D[10];
        if (Physics2D.Raycast(transform.position + circleOffset, direction, contactFilter, raycastHits, maxRaycastDistance) != 0)
        {
            foreach (var raycastHit in raycastHits)
            { 
                if (raycastHit.collider == null) continue;

                serverHit.transform.position = raycastHit.point;
                serverHit.SetActive(true);
                break;
            }
        }

        // revert back positions
        foreach (var pos in savedPositions)
        {
            pos.Object.position = pos.PrvsPosition;
        }

    }

    // for fake players
    public bool InBounds(Bounds bounds)
    {
        return bounds.Contains(transform.position);
    }

    public void ApplyFakeRotation(float angle)
    {
        transform.Rotate(new Vector3(0, 0, angle));
    }

    public void ApplyFakeMovement(float deltaTime)
    {
        characterController.move(Velocity * deltaTime * speed);
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }
}