using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Netherlands3D.Masking
{
    public class MaskingDome : MonoBehaviour
    {
        [SerializeField] private float margin;

        [Header("Global shader names")]
        [SerializeField] private string sphericalMaskPositionName = "_SphericalMaskPosition";
        [SerializeField] private string sphericalMaskRadiusName = "_SphericalMaskRadius";

        private int positionPropertyID;
        private int radiusPropertyID;

        [SerializeField] private DomeInteraction domeInteraction;

        private void Awake() {
            GetPropertyIDs();
            ApplyGlobalShaderVariables();
        }

        void Update()
        {
            if (domeInteraction.transform.hasChanged)
            {
                ApplyGlobalShaderVariables();
                domeInteraction.transform.hasChanged = false;
            }

            domeInteraction.ScaleByDistance();
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
