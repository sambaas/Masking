using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Netherlands3D.Masking
{
    public class DomeInteraction : MonoBehaviour, 
    IPointerClickHandler,
    IPointerUpHandler, IPointerEnterHandler,
    IBeginDragHandler, IDragHandler
    {   
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color defaultColor = Color.white;
        private Material domeMaterial;

        private Camera mainCamera;
        
        private bool hovering = false;
        private bool isDragging = false;
        private bool hoveringEdge = false;
        [SerializeField] private float scale = 1.0f;

        private Vector3 startDragPointerPosition;

        [SerializeField] private UnityEvent selected;
        [SerializeField] private UnityEvent deselected;

        private Coroutine animationCoroutine;

        [Header("Scale in animation")]
        [SerializeField] private AnimationCurve appearAnimationCurve;
        [SerializeField] private float appearTime = 0.5f;
        SphereCollider sphereCollider;

        public bool AllowInteraction
        {
            get => sphereCollider.enabled;
            set
            {
                sphereCollider.enabled = value;
            }
        }

        private void Awake() {
            sphereCollider = GetComponent<SphereCollider>();
            sphereCollider.enabled = false;
            mainCamera = Camera.main;
        }

        private void Start()
        {
            if(!mainCamera.TryGetComponent<PhysicsRaycaster>(out PhysicsRaycaster raycaster))
            {
                Debug.LogWarning("A PhysicsRaycaster is required  on main Camera in order for the dome to be selectable", this.gameObject);
            }

            domeMaterial = this.GetComponent<MeshRenderer>().material;
        }
        


        public void MoveToScreenPoint(Vector2 screenPoint)
        {
            transform.position = PointerWorldPosition(screenPoint);
            ScaleByCameraDistance();
        }
        
        public void AnimateIn()
        {
            if(animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AppearAnimation());
        }

        
        private IEnumerator AppearAnimation()
        {
            var targetScale = ScaleByCameraDistance();
            var animationTime = 0.0f;
            while(animationTime < appearTime){
                animationTime += Time.deltaTime;
                var curveTime = animationTime / appearTime;
                var curveValue = appearAnimationCurve.Evaluate(curveTime);

                this.transform.localScale = targetScale * curveValue;
                yield return null;
            }

            this.transform.localScale = targetScale;
            animationCoroutine = null;
        }

        private void Update()
        {
            //Check if we are hovering edge
            if(hovering && Pointer.current != null)
            {
                Vector2 pointerPosition = Pointer.current.position.ReadValue();
                var objectPosition = mainCamera.WorldToScreenPoint(this.transform.position);
            }        
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Set the object as being dragged
            isDragging = true;
            startDragPointerPosition = eventData.position;

            //Highlight 
            domeMaterial.color = highlightColor;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                // If we are dragging the edge of the dome; scale instead of drag.
                if(hoveringEdge)
                {
                    //Scale
                    return;
                }

                // Update the object's position based on the mouse position
                transform.position = PointerWorldPosition(eventData.position);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Reset the dragging flag
            isDragging = false;
        }

        private Vector3 PointerWorldPosition(Vector2 position)
        {
            // Calculate the mouse position in world space
            Ray ray = mainCamera.ScreenPointToRay(position);
            Plane plane = new Plane(Vector3.up, transform.parent.position);
            plane.Raycast(ray, out float distance);
            Vector3 pointerWorldPosition = ray.GetPoint(distance);

            return pointerWorldPosition;
        }

        public Vector3 ScaleByCameraDistance()
        {
            var distanceScale = Mathf.Max(1.0f, scale * Vector3.Distance(mainCamera.transform.position, transform.position));
            return Vector3.one * distanceScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
            Debug.Log(name + " Game Object Clicked!");

            domeMaterial.color = highlightColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            domeMaterial.color = highlightColor;
            hovering = true;
        }
    }
}
