using UnityEngine;

public class PlayerMovement : MonoBehaviour {
   private static readonly float GRAVITY_MULTIPLIER = 8.0f;
   public event OnTeleportRotationDelegate OnTeleportRotation;

   public delegate void OnTeleportRotationDelegate(float deltaRotY);

   [SerializeField] public float movementSpeed = 5f;
   [SerializeField] public float gravity = -9.81f;
   [SerializeField] public float jumpHeight = 3f;

   [SerializeField] public Transform groundCheck;
   [SerializeField] public float groundDistance = 0.4f;
   private Vector3 velocity;
   private CharacterController controller;

   private void Start() {
      controller = GetComponent<CharacterController>();
      IgnorePortalColliders();
   }

   private void IgnorePortalColliders() {
      foreach (var portal in FindObjectsOfType<Portal>()) {
         var portalCollider = portal.GetComponentInChildren<Collider>();
         Physics.IgnoreCollision(controller, portalCollider);
      }
   }

   private void Update() {
      bool grounded = Physics.CheckSphere(groundCheck.position, groundDistance);
      if (grounded) {
         if (velocity.y < 0) {
            velocity.y = -2f;
         }

         // if (Input.GetButtonDown("Jump")) {
         //    velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);
         // }
      }

      Vector3 move = GetMoveVector();

      // TODO: Extract teleport related code to new script
      RaycastHit raycastHit;
      if (Physics.Raycast(controller.transform.position, move, out raycastHit, move.magnitude)) {
         var colliderObject = raycastHit.transform.gameObject;
         var portal = colliderObject.GetComponentInParent<Portal>();
         if (portal) {
            Teleport(move, raycastHit.point, raycastHit.distance, portal.GetComponent<Transform>(), portal);
            return;
         }
      }

      controller.Move(move);

      // Gravity
      velocity.y += gravity * Time.deltaTime * GRAVITY_MULTIPLIER;
      controller.Move(velocity * Time.deltaTime);
   }

   private Vector3 GetMoveVector() {
      float x = Input.GetAxis("Horizontal");
      float z = Input.GetAxis("Vertical");
      var controllerTransform = controller.transform;
      return (controllerTransform.right * x + controllerTransform.forward * z) * (movementSpeed * Time.deltaTime);
   }

   private void Teleport(Vector3 originalMove, Vector3 raycastHitPoint, float raycastHitDistance,
      Transform portalTransform, Portal portal) {
      Portal targetPortal = portal.linkedPortal;
      Transform targetPortalTransform = targetPortal.GetComponent<Transform>();

      Vector3 relativeDeltaPosition = raycastHitPoint - portalTransform.position;

      var portalRotY = portalTransform.rotation.eulerAngles.y;
      var targetPortalRotY = targetPortalTransform.rotation.eulerAngles.y;

      var deltaRotY = targetPortalRotY - portalRotY + 0.1f;
      var deltaRot = Quaternion.Euler(0, deltaRotY, 0);

      var rotatedRelativeDeltaPosition = deltaRot * relativeDeltaPosition;
      Vector3 moveLeft = originalMove * (originalMove.magnitude - raycastHitDistance);
      Vector3 rotatedMoveLeft = deltaRot * moveLeft;

      controller.enabled = false;
      controller.transform.position = targetPortalTransform.position +
                                      rotatedRelativeDeltaPosition +
                                      rotatedMoveLeft;
      controller.enabled = true;
      velocity = deltaRot * velocity;

      OnTeleportRotation?.Invoke(deltaRotY);
   }
}
