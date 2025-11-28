using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerRaycaster : MonoBehaviour
    {
        [SerializeField] private float _maxDistance = 4f;
        [SerializeField] private Camera _camera;

        private Transform _cameraTransform;
        
        private void Awake()
        {
            _cameraTransform = _camera.transform;
        }
        
        public bool GetRaycast(LayerMask mask, out Collider coll)
        {
            Vector3 origin = _cameraTransform.position;
            Vector3 direction = _cameraTransform.forward;
            
            Debug.DrawRay(origin, direction * _maxDistance, Color.red);
            coll = null;

            if (Physics.Raycast(origin, direction, out var hit, _maxDistance, mask))
            {
                coll = hit.collider;
                return true;
            }
            
            return false;
        }
    }
}