using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CartoonFX
{
	public class VFX_Manager : MonoBehaviour
	{
		[System.NonSerialized] public GameObject currentEffect;
		GameObject[] effectsList;

		void Awake()
		{
			var list = new List<GameObject>();
			for (int i = 0; i < this.transform.childCount; i++)
			{
				var effect = this.transform.GetChild(i).gameObject;
				list.Add(effect);

				var cfxrEffect= effect.GetComponent<CFXR_Effect>();
				if (cfxrEffect != null) cfxrEffect.clearBehavior = CFXR_Effect.ClearBehavior.Disable;
			}
			effectsList = list.ToArray();
		}

        public void PlayAtIndex(int index, Vector3 position)
		{
			
			int indexWraped = WrapIndex(index);
			currentEffect = effectsList[indexWraped];
            currentEffect.transform.position = position;
            currentEffect.SetActive(true);
		}

		private int WrapIndex(int index)
		{
			if (index < 0) index = effectsList.Length - 1;
			if (index >= effectsList.Length) index = 0;
			return index;
		}
	}
}