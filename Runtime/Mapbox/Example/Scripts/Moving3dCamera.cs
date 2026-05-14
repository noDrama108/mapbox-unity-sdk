using System;
using Mapbox.BaseModule.Map;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Mapbox.Example.Scripts.MapInput
{
    [Serializable]
    public class Moving3dCamera : MapInput
    {
        [Range(15, 90)]
        public float Pitch;
        [Range(-180, 180)]
        public float Bearing;

        [NonSerialized] public float ZoomValue;
        public float Speed = -10;
        public float CameraDistance;
        
        public Action Updated = () => { };

        private Vector3 _previousScreenPosition;
        private Vector3 _dragOrigin;
        [SerializeField] private Vector3 _targetPosition;
        private float deltaAngleH;
        private float deltaAngleV;
        public float RotationSpeed = 50.0f;
        public AnimationCurveContainer CameraCurve;
        public float CamDistanceMultiplier = 2;
        public float ZoomSpeed = 0.25f;
        
        
        public void Initialize(Camera camera, IMapInformation start)
        {
            _camera = camera ? camera : Camera.main;
            Pitch = start.Pitch;
            Bearing = start.Bearing;
            ZoomValue = start.Zoom;
            SetCamPositionByMapInfo(start);
            start.LatitudeLongitudeChanged += information =>
            {
                _targetPosition = Vector3.zero;
                SetCamera(information);
            };
            start.ViewChanged += SetCamera;
        }
        
        public override bool UpdateCamera(IMapInformation mapInformation)
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return false;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return false;

            Vector3 mousePos = new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, 0f);
            var hasChanged = false;

            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            {
                _previousScreenPosition = mousePos;
                _dragOrigin = GetPlaneIntersection(mousePos);
            }

            if (mouse.leftButton.isPressed)
            {
                var newPoint = GetPlaneIntersection(mousePos);
                Vector3 pos = newPoint - _dragOrigin;
                Vector3 move = new Vector3(pos.x * Speed, 0, pos.z * Speed);
                _targetPosition += move;
                hasChanged = true;
            }
            else if (mouse.rightButton.isPressed)
            {
                var deltaMousePos = (Vector3)(mouse.position.ReadValue() - (Vector2)_previousScreenPosition);
                deltaAngleH = deltaMousePos.x;
                deltaAngleV = deltaMousePos.y;
                if (deltaAngleH != 0 || deltaAngleV != 0)
                {
                    Pitch -= deltaAngleV * Time.deltaTime * RotationSpeed;
                    Pitch = Mathf.Min(90, Mathf.Max(15, Pitch));
                    Bearing += deltaAngleH * Time.deltaTime * RotationSpeed;
                }
                hasChanged = true;
            }
            else
            {
                var scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.0001f)
                {
                    Zoom(mapInformation, GetPlaneIntersection(mousePos), scroll);
                    hasChanged = true;
                }
            }

            _dragOrigin = GetPlaneIntersection(mousePos);
            _previousScreenPosition = mousePos;

            return hasChanged;
        }

        public void Zoom(IMapInformation mapInformation, Vector3 position, float zoomAction)
        {
            //calculate next zoom value by simply adding scroll delta * speed
            //var mouseWorld = position; 
            //var mouseMeter = mouseWorld * mapInformation.Scale;
            var postZoom = ZoomValue + zoomAction * ZoomSpeed;
				
            //to be able to achieve zoom on mouse cursor, we have to move camera on mouse world pos - camera pos line (b-c)
            //but our camera distance uses camera target (mid screen) to camera pos distance (a-c)
            //and there'll be a difference between (ac) and (bc) distance
            //so we calculate new distance and then use pre/post distance ratio to calculate the value on mouse-camera line
            //we then use this new (bc) distance for final pos calculation
            /// a----b
            /// |   / 
            /// |  / 
            /// | /
            /// c
            var newScaleWillBe = mapInformation.GetScaleFor(postZoom);
            var latlng = mapInformation.ConvertPositionToLatLng(position);
            var postZoomPos = mapInformation.ConvertLatLngToPositionForScale(latlng, newScaleWillBe);
            
            var targetPoslatlng = mapInformation.ConvertPositionToLatLng(_targetPosition);
            var postZoomTarget = mapInformation.ConvertLatLngToPositionForScale(targetPoslatlng, newScaleWillBe);
            
            
            var preDistance = CalculateCameraDistance(mapInformation, ZoomValue);
            var camDistanceToMouse = Vector3.Distance(_camera.transform.position, position);
            ZoomValue = postZoom;
            var postDistance = CalculateCameraDistance(mapInformation, postZoom);
            var newCamDistanceToMouse = camDistanceToMouse * (postDistance / preDistance);
            CameraDistance = CalculateCameraDistance(mapInformation, ZoomValue);
            _targetPosition = Vector3.LerpUnclamped(postZoomTarget, postZoomPos, (camDistanceToMouse - newCamDistanceToMouse) / camDistanceToMouse);
        }

        private void SetCamPositionByMapInfo(IMapInformation start)
        {
            CameraDistance = CalculateCameraDistance(start, start.Zoom);
            _camera.transform.position = _targetPosition;
            _camera.transform.rotation = Quaternion.Euler(Pitch, Bearing, 0);
            _camera.transform.position += _camera.transform.forward * (-1f * CameraDistance);
            var mouse = Mouse.current;
            if (mouse != null)
                _dragOrigin = GetPlaneIntersection(new Vector3(mouse.position.ReadValue().x, mouse.position.ReadValue().y, 0f));
        }

        public void SetCamera(IMapInformation mapInfo)
        {
            Pitch = mapInfo.Pitch;
            Bearing = mapInfo.Bearing;
            SetCamPositionByMapInfo(mapInfo);
        }

        private Vector3 GetPlaneIntersection(Vector3 screenPosition)
        {
            var ray = _camera.ScreenPointToRay(screenPosition);
            var dirNorm = ray.direction / ray.direction.y;
            var intersectionPos = ray.origin - dirNorm * ray.origin.y;
            return intersectionPos;
        }
    
        private float CalculateCameraDistance(IMapInformation mapInformation, float postZoom)
        {
            var distance = CameraCurve.Evaluate(postZoom);
            return CamDistanceMultiplier * distance / mapInformation.Scale;
        }

        public Vector3 GetViewCenterPosition()
        {
            return GetPlaneIntersection(_camera.ViewportToScreenPoint(new Vector3(.5f, .5f, 0f)));
        }

        public Plane[] GetFrustrumPlanes()
        {
            return GeometryUtility.CalculateFrustumPlanes(_camera);
        }

        public Transform GetTransform()
        {
            return _camera.transform;
        }
    }
}