using UnityEngine;

public class PlayerMouseLook : MonoBehaviour {
   [SerializeField] float mouseSensitivity = 100f;
   public Transform player;
   private float xRotation;
   private PlayerMovement playerMovement;

   private void Start() {
      Cursor.lockState = CursorLockMode.Locked;
      playerMovement = GetComponentInParent<PlayerMovement>();
      playerMovement.OnTeleportRotation += RotateHorizontally;
   }

   private void RotateHorizontally(float rotY) {
      player.Rotate(Vector3.up * rotY);
   }

   private void Update() {
      UpdateRotation();
   }

   private void UpdateRotation() {
      float mouseDeltaX = GetMouseAxis("Mouse X");
      float mouseDeltaY = GetMouseAxis("Mouse Y");

      // Vertical
      xRotation = Mathf.Clamp(xRotation - mouseDeltaY, -90f, 90f);
      transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

      // Horizontal
      RotateHorizontally(mouseDeltaX);
   }

   private float GetMouseAxis(string axis) {
      return Input.GetAxis(axis) * mouseSensitivity * Time.deltaTime;
   }

   private void OnDestroy() {
      playerMovement.OnTeleportRotation -= RotateHorizontally;
   }
}
