using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PortalRenderer : MonoBehaviour {
   Portal[] portals;

   void Awake() {
      portals = FindObjectsOfType<Portal>();
      RenderPipelineManager.beginFrameRendering += RenderPortals;
   }

   private void RenderPortals(ScriptableRenderContext context, Camera[] camera) {
      for (int i = 0; i < portals.Length; i++) {
         portals[i].Render(context);
      }

      for (int i = 0; i < portals.Length; i++) {
         portals[i].PostPortalRender();
      }
   }

   private void OnDestroy() {
      RenderPipelineManager.beginFrameRendering -= RenderPortals;
   }
}
