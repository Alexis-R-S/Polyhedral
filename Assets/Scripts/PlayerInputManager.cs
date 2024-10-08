using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
    // InputAction scrollAction;
    // InputAction touch0pos;
    // InputAction touch1pos;
    // InputAction touch0contact;
    // InputAction touch1contact;
    PlayerInput playerInput;

    [SerializeField] Planet planet;

    [SerializeField] float minFOV = 20;
    [SerializeField] float maxFOV = 60;
    [SerializeField] float zoomSpeed = 0.01f;
    [SerializeField] float camSpeed = 0.1f;
    [SerializeField] float pieceRotationSpeed = 1.0f;
    [SerializeField] float minTouchMotionToRotate = 50;        // TODO : relative to screen size
    [SerializeField] float minRotationToConfirm = 0.5f;
    
    Camera cam;
    Vector3 curScreenPos;
    float prevMagnitude = 0;
    int touchCount = 0;
    bool isDragging = false;

    void ActivateInputActions() {
        // Mouse scroll
        playerInput.actions.FindAction("Scroll").performed += ctx => CameraZoom(ctx.ReadValue<Vector2>().y * zoomSpeed);

        // Pinch gesture
        InputAction touch0pos = playerInput.actions.FindAction("Touch0_pos");
        InputAction touch1pos = playerInput.actions.FindAction("Touch1_pos");

        InputAction touch0contact = playerInput.actions.FindAction("Touch0_contact");
        InputAction touch1contact = playerInput.actions.FindAction("Touch1_contact");

        touch0contact.started += _ => {
            touchCount++;
            if (touchCount == 1) {
                StartCoroutine( Drag() );
            }
            else {
                if (!planet.isGroupRotating)
                    isDragging = false;
            }
        };
        touch1contact.started += _ => {
            touchCount++;
            if (!planet.isGroupRotating)
                isDragging = false;
        };
        touch0contact.canceled += _ => {
            touchCount--;
            prevMagnitude = 0;
            if (touchCount == 0)
                isDragging = false;
        };
        touch1contact.canceled += _ => {
            touchCount--;
            prevMagnitude = 0;
            if (touchCount == 0)
                isDragging = false;
        };

        touch0pos.performed += ctx => {
            curScreenPos = ctx.ReadValue<Vector2>();
        };

        touch1pos.performed += _ => {
            if (touchCount < 2 || planet.isGroupRotating)
                return;

            float magnitude = (touch0pos.ReadValue<Vector2>() - touch1pos.ReadValue<Vector2>()).magnitude;
            if (prevMagnitude == 0) {
                prevMagnitude = magnitude;
            }
            float difference = magnitude - prevMagnitude;
            prevMagnitude = magnitude;
            CameraZoom(difference * zoomSpeed);
        };
    }

    void Awake() {
        playerInput = GetComponent<PlayerInput>();
    }

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        ActivateInputActions();
    }

    private void CameraZoom(float decrement) {
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - decrement, minFOV, maxFOV);
    }

    private IEnumerator Drag() {
        if (isDragging) {
            yield break;
        }
        isDragging = true;

        Vector3 frameScreenPos = curScreenPos;
        Vector3 startScreenPos = frameScreenPos;
        Vector3 lastScreenPos = frameScreenPos;
        Ray ray = cam.ScreenPointToRay(frameScreenPos);

        RaycastHit hit;
        int layerMask = 1 << 6;     // Only cast ray against layer 6
        bool hitExists = Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask);
        if (hitExists && !hit.collider.CompareTag("NoRotation")) {
            TouchDetector detector = hit.collider.GetComponent<TouchDetector>();
            // Outside the loop because planet should be still during drag
            Vector3 screenDetectorDirection = Vector3.zero;
            
            float angle_prop = 0;
            int axis_index = -1;
            Vector3 screenTouchMotion;

            while(isDragging) {
                frameScreenPos = curScreenPos;
                if (frameScreenPos != lastScreenPos) {
                    screenTouchMotion = frameScreenPos - startScreenPos;

                    // If direction has not been chosen yet
                    if (axis_index == -1) {

                        // If touch has dragged far enough
                        if (screenTouchMotion.magnitude > minTouchMotionToRotate) {
                            if (detector.Directions.Length < 1) {
                                Debug.LogError("Error: TouchDetector has no directions.");
                            }
                            Vector3 detectorWorldPos = detector.transform.position;
                            Vector3 detectorScreenPos = cam.WorldToScreenPoint(detectorWorldPos);

                            // Find the detector direction closest to screenTouchMotion                        
                            float best_score = -1;
                            float current_score;
                            Vector3 current_direction;
                            for (int i=0; i<detector.Directions.Length; i++) {
                                current_direction = cam.WorldToScreenPoint(detectorWorldPos + detector.transform.rotation * detector.Directions[i]) - detectorScreenPos;
                                current_score = Mathf.Abs( Vector3.Dot(current_direction.normalized, screenTouchMotion) );
                                if (current_score > best_score) {
                                    best_score = current_score;
                                    axis_index = i;
                                    screenDetectorDirection = current_direction;
                                }
                            }
                        }
                    }
                    else {
                        // If direction has already been chosen
                        // Change rotation (visuals only)
                        angle_prop = Vector3.Dot(screenTouchMotion, screenDetectorDirection)/screenDetectorDirection.sqrMagnitude;
                        angle_prop = Mathf.Clamp(angle_prop*pieceRotationSpeed, -1, 1);
                        detector.SetRotation(axis_index, angle_prop);

                        lastScreenPos = frameScreenPos;
                    }
                }
                yield return null;
            }

            // Drop
            // If direction has been chosen, hence pieces have rotated
            if (axis_index != -1) {
                if (Mathf.Abs(angle_prop) > minRotationToConfirm) {
                    detector.ConfirmRotation(axis_index, angle_prop > 0);
                }
                else {
                    detector.CancelRotation(axis_index);
                }
            }
        }
        else {
            // Rotate camera around planet

            Vector3 rotationAxis;
            while (isDragging) { 
                // Dragging
                frameScreenPos = curScreenPos;
                if (frameScreenPos != lastScreenPos) {
                    rotationAxis = cam.transform.up*(frameScreenPos-lastScreenPos).x - cam.transform.right*(frameScreenPos-lastScreenPos).y;
                    cam.transform.RotateAround(planet.transform.position, rotationAxis, rotationAxis.magnitude*camSpeed);
                    lastScreenPos = frameScreenPos;
                }
                yield return null;
            }
        }
    }

    // TODO : solve bug: doesn't work in build ?
}
