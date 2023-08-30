using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

namespace Netherlands3D.Masking
{
    public class MaskingDome : MonoBehaviour
    {
        [Header("Action bindings")]
        [SerializeField] private InputActionReference clickPlacementAction;
        private InputSystemUIInputModule inputSystemUIInputModule;
        [SerializeField] private float maxCameraTravelToPlacement = 20.0f; 

        [SerializeField] private float margin;

        [Header("Global shader names")]
        [SerializeField] private string sphericalMaskPositionName = "_SphericalMaskPosition";
        [SerializeField] private string sphericalMaskRadiusName = "_SphericalMaskRadius";

        private int positionPropertyID;
        private int radiusPropertyID;

        [SerializeField] private DomeInteraction domeInteraction;

        private Camera mainCamera;
        private Vector3 cameraLookatPosition = Vector3.zero;
        private Quaternion cameraRotation = Quaternion.identity;

        private bool waitForInitialPlacement = false;

        private void Start() {
            mainCamera = Camera.main;
            
            GetPropertyIDs();
            ApplyGlobalShaderVariables();
        }

        private void OnEnable() {
            clickPlacementAction.action.Enable();
            clickPlacementAction.action.started += StartTap;
            clickPlacementAction.action.performed += EndTap;

            StickToPointer();
        }

        private void OnDisable()
        {
            // Unsubscribe and disable the click action when the script is disabled
            clickPlacementAction.action.performed -= StartTap;
            clickPlacementAction.action.Disable();
        }

        /// <summary>
        /// Initial start will make dome follow pointer untill first click
        /// </summary>
        private void StickToPointer()
        {
            domeInteraction.AnimateIn();
            waitForInitialPlacement = true;
        }

        private void StartTap(InputAction.CallbackContext context)
        {
            Debug.Log("Start");
            cameraLookatPosition = LookPosition();
        }
        private void EndTap(InputAction.CallbackContext context)
        {
            var currentCameraLookatPosition = LookPosition();
            var distanceTraveled = Vector3.Distance(cameraLookatPosition, currentCameraLookatPosition);
            Debug.Log($"End distanceTraveled {distanceTraveled}"); 
            if(distanceTraveled < maxCameraTravelToPlacement)
            {
                PlaceDome();
            }
        }

        private Vector3 LookPosition()
        {
            // Calculate the mouse position in world space
            Ray ray = mainCamera.ScreenPointToRay(Vector3.one*0.5f);
            Plane plane = new Plane(Vector3.up, transform.position);
            plane.Raycast(ray, out float distance);
            Vector3 pointerWorldPosition = ray.GetPoint(distance);

            return pointerWorldPosition;
        }

        private void PlaceDome()
        {
            waitForInitialPlacement = false;
            domeInteraction.AllowInteraction = true;

            if(!EventSystem.current.IsPointerOverGameObject()){
                Vector2 pointerPosition = Pointer.current.position.ReadValue();
                domeInteraction.MoveToScreenPoint(pointerPosition);
                domeInteraction.AnimateIn();
            }
        }      

        void Update()
        {
            if(waitForInitialPlacement)
            {
                domeInteraction.MoveToScreenPoint(Pointer.current.position.ReadValue());
            }

            if (domeInteraction.transform.hasChanged)
            {
                ApplyGlobalShaderVariables();
                domeInteraction.transform.hasChanged = false;
            }
        }

        private void GetPropertyIDs(){
            positionPropertyID = Shader.PropertyToID(sphericalMaskPositionName);
            radiusPropertyID = Shader.PropertyToID(sphericalMaskRadiusName);
        }

        private void ApplyGlobalShaderVariables()
        {
            Shader.SetGlobalVector(positionPropertyID,domeInteraction.transform.position);
            Shader.SetGlobalFloat(radiusPropertyID,(domeInteraction.transform.localScale.x/2.0f) + margin);
        }
    }
}
