using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Portal : MonoBehaviour {
   [Header("Main Settings")] public Portal linkedPortal;
   public MeshRenderer screen;
   public int recursionLimit = 5;

   [Header("Advanced Settings")] public float nearClipOffset = 0.05f;
   public float nearClipLimit = 0.2f;

   // Private variables
   RenderTexture viewTexture;
   public Camera portalCam;
   public Camera playerCam;
   Material firstRecursionMat;
   MeshFilter screenMeshFilter;

   void Start() {
      portalCam.enabled = false;
      screenMeshFilter = screen.GetComponent<MeshFilter>();
      screen.material.SetInt("displayMask", 1);
   }

   // Manually render the camera attached to this portal
   // Called after PrePortalRender, and before PostPortalRender
   public void Render(ScriptableRenderContext context) {
      // Skip rendering the view from this portal if player is not looking at the linked portal
      if (!CameraUtility.VisibleFromCamera(linkedPortal.screen, playerCam)) {
         return;
      }

      CreateViewTexture();

      var localToWorldMatrix = playerCam.transform.localToWorldMatrix;
      var renderPositions = new Vector3[recursionLimit];
      var renderRotations = new Quaternion[recursionLimit];

      int startIndex = 0;
      portalCam.projectionMatrix = playerCam.projectionMatrix;
      for (int i = 0; i < recursionLimit; i++) {
         if (i > 0) {
            // No need for recursive rendering if linked portal is not visible through this portal
            if (!CameraUtility.BoundsOverlap(screenMeshFilter, linkedPortal.screenMeshFilter, portalCam)) {
               break;
            }
         }

         localToWorldMatrix = transform.localToWorldMatrix *
                              linkedPortal.transform.worldToLocalMatrix *
                              localToWorldMatrix;
         int renderOrderIndex = recursionLimit - i - 1;
         renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn(3);
         renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

         portalCam.transform.SetPositionAndRotation(renderPositions[renderOrderIndex],
            renderRotations[renderOrderIndex]
         );
         startIndex = renderOrderIndex;
      }

      // Hide screen so that camera can see through portal
      screen.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
      linkedPortal.screen.material.SetInt("displayMask", 0);

      for (int i = startIndex; i < recursionLimit; i++) {
         portalCam.transform.SetPositionAndRotation(renderPositions[i], renderRotations[i]);
         SetNearClipPlane();
         UniversalRenderPipeline.RenderSingleCamera(context, portalCam);
         // portalCam.Render();

         if (i == startIndex) {
            linkedPortal.screen.material.SetInt("displayMask", 1);
         }
      }

      // Unhide objects hidden at start of render
      screen.shadowCastingMode = ShadowCastingMode.On;
   }

   // Called once all portals have been rendered, but before the player camera renders
   public void PostPortalRender() {
      ProtectScreenFromClipping(playerCam.transform.position);
   }

   void CreateViewTexture() {
      if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height) {
         if (viewTexture != null) {
            viewTexture.Release();
         }

         viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
         // Render the view from the portal camera to the view texture
         portalCam.targetTexture = viewTexture;
         // Display the view texture on the screen of the linked portal
         linkedPortal.screen.material.SetTexture("_MainTex", viewTexture);
      }
   }

   // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
   void ProtectScreenFromClipping(Vector3 viewPoint) {
      float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
      float halfWidth = halfHeight * playerCam.aspect;
      float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
      float screenThickness = dstToNearClipPlaneCorner;

      Transform screenT = screen.transform;
      bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
      screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
      screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
   }

   // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
   // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
   void SetNearClipPlane() {
      // Learning resource:
      // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
      Transform clipPlane = transform;
      int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));

      Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
      Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
      float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

      // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
      if (Mathf.Abs(camSpaceDst) > nearClipLimit) {
         Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

         // Update projection based on new clip plane
         // Calculate matrix with player cam so that player camera settings (fov, etc) are used
         portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
      }
      else {
         portalCam.projectionMatrix = playerCam.projectionMatrix;
      }
   }

   void OnValidate() {
      if (linkedPortal != null) {
         linkedPortal.linkedPortal = this;
      }
   }
}
