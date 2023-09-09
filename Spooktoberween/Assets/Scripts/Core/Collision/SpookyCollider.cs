using UnityEngine;

[ExecuteInEditMode]
public class SpookyCollider : MonoBehaviour
{
    public static readonly float CollisionYValue = 0f;

    public bool bShouldTick = false;
    public float CollisionYValueOffset = 0f;

    private void Awake()
    {
        transform.position = new Vector3(transform.position.x, CollisionYValue + CollisionYValueOffset, transform.position.z);
        transform.hasChanged = false;

        bool bShouldDisable = !bShouldTick;

        // don't turn off the component in edit mode
        #if UNITY_EDITOR
        bShouldDisable &= !Application.isEditor;
        #endif

        if(bShouldDisable)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if(transform.hasChanged)
        {
            transform.position = new Vector3(transform.position.x, CollisionYValue + CollisionYValueOffset, transform.position.z);
            transform.hasChanged = false;
        }
    }

    public void UpdateTransform()
    {
        transform.position = new Vector3(transform.position.x, CollisionYValue + CollisionYValueOffset, transform.position.z);
    }

    private void OnEnable()
    {
        bool bShouldDisable = !bShouldTick;

        Update();

        // don't turn off the component in edit mode
#if UNITY_EDITOR
        bShouldDisable &= !Application.isEditor;
#endif
        if (bShouldDisable)
        {
            enabled = false;
        }
    }
}
