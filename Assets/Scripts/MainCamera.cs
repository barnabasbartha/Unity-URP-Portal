using UnityEngine;

public class MainCamera : MonoBehaviour {
   Portal[] portals;

   void Awake() {
      portals = FindObjectsOfType<Portal>();
   }

   void Update() {
      for (int i = 0; i < portals.Length; i++) {
         portals[i].Render();
      }

      for (int i = 0; i < portals.Length; i++) {
         portals[i].PostPortalRender();
      }
   }
}
