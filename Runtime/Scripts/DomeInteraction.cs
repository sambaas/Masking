using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Netherlands3D.Masking
{
    public class DomeInteraction : MonoBehaviour, 
    IPointerDownHandler, IPointerClickHandler,
    IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler,
    IBeginDragHandler, IDragHandler
    {
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color defaultColor = Color.white;
        private Material domeMaterial;

        private Camera mainCamera;
        
        private bool hovering = false;
        private bool isDragging = false;
        private bool hoveringEdge = false;
        private float scale = 1.0f;

        private Vector3 startDragPointerPosition;

        [SerializeField] private UnityEvent selected;
        [SerializeField] private UnityEvent deselected;

        private void Start()
        {
            mainCamera = Camera.main;
            if(!mainCamera.TryGetComponent<PhysicsRaycaster>(out PhysicsRaycaster raycaster))
            {
                Debug.LogWarning("A PhysicsRaycaster is required in order for the dome to be selectable", this.gameObject);
            }

            domeMaterial = this.GetComponent<MeshRenderer>().material;

            //Start listening to generic clicks using input system to detect world clicks

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
                transform.position = GetMouseWorldPosition(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Reset the dragging flag
            isDragging = false;
        }

        private Vector3 GetMouseWorldPosition(PointerEventData eventData)
        {
            // Calculate the mouse position in world space
            Ray ray = mainCamera.ScreenPointToRay(eventData.position);
            Plane plane = new Plane(Vector3.up, transform.parent.position);
            float distance;
            plane.Raycast(ray, out distance);
            Vector3 mouseWorldPosition = ray.GetPoint(distance);

            return mouseWorldPosition;
        }

        public void ScaleByDistance()
        {
            var distanceScale = Mathf.Max(1.0f, scale * Vector3.Distance(mainCamera.transform.position, transform.position));
            this.transform.localScale = Vector3.one * distanceScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Output to console the clicked GameObject's name and the following message. You can replace this with your own actions for when clicking the GameObject.
            Debug.Log(name + " Game Object Clicked!");

            domeMaterial.color = highlightColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Debug.Log("Mouse Down: " + eventData.pointerCurrentRaycast.gameObject.name);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            domeMaterial.color = highlightColor;
            hovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Mouse Exit");
            domeMaterial.color = defaultColor;
            hovering = false;
        }
    }
}
