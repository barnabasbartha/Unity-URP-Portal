using UnityEngine;

public class PlayerMovement : MonoBehaviour {
   [SerializeField] float movementSpeed = 5f;
   [SerializeField] float gravity = -9.81f * 2;
   [SerializeField] float jumpHeight = 3f;

   public Transform groundCheck;
   [SerializeField] float groundDistance = 0.4f;
   private Vector3 velocity;
   private CharacterController controller;

   void Start() {
      controller = GetComponent<CharacterController>();
   }

   void Update() {
      bool grounded = Physics.CheckSphere(groundCheck.position, groundDistance);
      if (grounded) {
         if (velocity.y < 0) {
            velocity.y = -2f;
         }

         if (Input.GetButtonDown("Jump")) {
            velocity.y += Mathf.Sqrt(jumpHeight * -2f * gravity);
         }
      }

      // Movement
      float x = Input.GetAxis("Horizontal");
      float z = Input.GetAxis("Vertical");

      var transform1 = transform;
      Vector3 move = transform1.right * x + transform1.forward * z;
      controller.Move(move * (movementSpeed * Time.deltaTime));

      // Gravity
      velocity.y += gravity * Time.deltaTime;
      controller.Move(velocity * Time.deltaTime);
   }
}
