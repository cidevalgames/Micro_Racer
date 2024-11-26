using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Multiplayer.Fishnet.Player.Capsule
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField, Range(1, 5)] private float speed = 3.5f;
        [SerializeField, Range(5, 100)] private float mouseSensitivityX = 30f;
        [SerializeField, Range(5, 100)] private float mouseSensitivityY = 30f;
        [SerializeField, Range(1, 2)] private float cameraYOffset = 1.5f;

        private Vector2 _moveValue;
        private Vector2 _lookValue;
        private float _xRotation = 0f;

        private Camera _playerCamera;
        private InputActionAsset _actions;

        private CharacterController m_characterController;

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (base.IsOwner)
            {
                _playerCamera = Camera.main;
                _playerCamera.transform.position = transform.position + Vector3.up * cameraYOffset;
                _playerCamera.transform.SetParent(transform);
            }
            else
            {
                gameObject.GetComponent<PlayerController>().enabled = false;
            }
        }

        private void Awake()
        {
            m_characterController = GetComponent<CharacterController>();

            _actions = InputSystem.actions;

            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (!base.IsOwner)
                return;

            if (!IsClientStarted) 
                return;

            // Move

            _moveValue = _actions.FindAction("Move").ReadValue<Vector2>();

            Vector3 motion = (Vector3.right * _moveValue.x + Vector3.forward * _moveValue.y).normalized * Time.deltaTime * speed;

            m_characterController.Move(transform.TransformDirection(motion));

            // Look

            _lookValue = _actions.FindAction("Look").ReadValue<Vector2>();

            float _mouseX = _lookValue.x * mouseSensitivityX * Time.deltaTime;
            float _mouseY = _lookValue.y * mouseSensitivityY * Time.deltaTime;

            _xRotation -= _mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

            _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0, 0);

            transform.Rotate(Vector3.up * _mouseX);
        }
    }
}
