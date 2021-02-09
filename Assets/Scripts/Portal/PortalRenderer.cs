using UnityEngine;
using UnityEngine.Rendering;

public class PortalRenderer : MonoBehaviour {
   private Portal[] portals;

   private void Start() {
      portals = FindObjectsOfType<Portal>();
      RenderPipelineManager.beginFrameRendering += RenderPortals;
   }

   private void RenderPortals(ScriptableRenderContext context, Camera[] camera) {
      foreach (var portal in portals) {
         portal.Render(context);
      }

      foreach (var portal in portals) {
         portal.PostPortalRender();
      }
   }

   private void OnDestroy() {
      RenderPipelineManager.beginFrameRendering -= RenderPortals;
   }
}
