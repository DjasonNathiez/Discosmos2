using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector2 rotationOffset;
    [SerializeField] private float cameraSpeed;
    
    [SerializeField] private AnimationCurve cameraZoomCurve;
    [SerializeField] private AnimationCurve speedLinesCurve;
    [SerializeField] private Transform speedLines;

    public bool activeZoom;
    public float zoomInActive;
    public Vector3 posInActive;
    public float activeTime;
    
    private PlayerController playerController;

    public bool cameraLock = true;
    private Vector3 nextPos;
    private Vector3 forward;
    private Vector3 right;

    private void Awake()
    {
        if(!player) player = FindObjectOfType<PlayerManager>().controller.transform;
    }

    private void Start()
    {
        //transform rotation but just the y
        transform.rotation = Quaternion.Euler(0, rotationOffset.y, 0);
        forward = transform.forward;
        right = transform.right;
        transform.rotation = Quaternion.Euler(rotationOffset.x, rotationOffset.y, 0);
        transform.position += offset;
        playerController = player.GetComponent<PlayerController>();
    }
    
    
    private void OnToggleCameraLock(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        cameraLock = !cameraLock;
        Debug.Log("Camera Lock Toggled");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            cameraLock = !cameraLock;
        }
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }
    
    public void SetupActiveZoom(float time,Vector3 position,float zoom)
    {
        /*activeTime = time;
        activeZoom = true;
        posInActive = position;
        zoomInActive = zoom;*/
    }


    private void UpdateCamera()
    {

        if (activeZoom)
        {
            transform.position = Vector3.Lerp(transform.position, posInActive, Time.deltaTime * 5);

            if (activeTime > 0)
            {
                activeTime -= Time.deltaTime;
            }
            else
            {
                activeZoom = false;
            }
        }
        else
        {
            if (cameraLock)
            {
                transform.position = Vector3.Lerp(transform.position, player.position, Time.deltaTime * 5);
                speedLines.localPosition = Vector3.Lerp(speedLines.localPosition, new Vector3(0,0,speedLinesCurve.Evaluate(playerController.manager.force)), Time.deltaTime * 5);
            }
            else
            {
                speedLines.localPosition = Vector3.Lerp(speedLines.localPosition, new Vector3(0,0,speedLinesCurve.Evaluate(0)), Time.deltaTime * 5);
                nextPos = transform.position;

                if (Input.mousePosition.x >= Screen.width - 1)
                {
                    nextPos += right * cameraSpeed;
                }

                if (Input.mousePosition.x <= 0)
                {
                    nextPos -= right * cameraSpeed;
                }

                if (Input.mousePosition.y >= Screen.height - 1)
                {
                    nextPos += forward * cameraSpeed;
                }

                if (Input.mousePosition.y <= 0)
                {
                    nextPos -= forward * cameraSpeed;
                }
                transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime * 5);
            }
        }
        
        // ZOOM
        if(playerController != null)
        {
            if (activeZoom)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * zoomInActive, Time.deltaTime * 5);   
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * cameraZoomCurve.Evaluate(playerController.manager.force), Time.deltaTime * 3);   
            }
        }


    }
}
