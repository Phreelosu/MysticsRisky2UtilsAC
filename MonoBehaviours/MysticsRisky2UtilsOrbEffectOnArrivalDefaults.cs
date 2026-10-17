using RoR2.Orbs;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace MysticsRisky2Utils.MonoBehaviours
{
	public class MysticsRisky2UtilsOrbEffectOnArrivalDefaults : MonoBehaviour
	{
		public OrbEffect orbEffect;
		public Transform[] transformsToUnparentChildren;
		public MonoBehaviour[] componentsToEnable;

		public void Awake()
		{
			if (transformsToUnparentChildren == null) transformsToUnparentChildren = new Transform[] { };
			if (componentsToEnable == null) componentsToEnable = new MonoBehaviour[] { };
			if (orbEffect)
			{
				orbEffect.onArrival.AddListener(() => OnArrival());
			}
		}

		public IEnumerator OnArrival()
        {
			for (int i = 0; i < 1; i = 1) yield return new WaitForEndOfFrame();
            foreach (Transform transform in transformsToUnparentChildren) transform?.DetachChildren();
            foreach (MonoBehaviour monoBehaviour in componentsToEnable) if (monoBehaviour != null) monoBehaviour.enabled = true;
			yield break;
        }
	}
}
