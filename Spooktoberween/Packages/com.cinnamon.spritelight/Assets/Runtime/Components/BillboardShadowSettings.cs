using UnityEngine;
using UnityEngine.Rendering;

namespace SpriteLightRendering
{
    [ExecuteInEditMode]
    public class BillboardShadowSettings : MonoBehaviour
    {
        public ShadowCastingMode shadowCastingMode = ShadowCastingMode.On;
        public bool ReceivesShadows = false;

        new Renderer renderer;

        void Start()
        {
            if(isActiveAndEnabled)
            {
                UpdateShadowSettings();
            }
        }

        public void UpdateShadowSettings()
        {
            if(!renderer)
            {
                renderer = GetComponent<Renderer>();
            }

            if (renderer)
            {
                renderer.shadowCastingMode = shadowCastingMode;
                renderer.receiveShadows = ReceivesShadows;
            }
        }

    #if UNITY_EDITOR
        private void Update()
        {
            if(!Application.isPlaying)
            {
                UpdateShadowSettings();
            }
        }

        private void OnValidate()
        {
            UpdateShadowSettings();
        }
    #endif
    }
}
