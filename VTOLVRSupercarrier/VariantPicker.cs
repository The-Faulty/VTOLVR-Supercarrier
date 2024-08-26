using UnityEngine;

namespace VTOLVRSupercarrier
{
  public class VariantPicker : MonoBehaviour
  {
    public Material[] materials;

    void Start()
    {
      int num = Random.Range(0, materials.Length - 1);
      GetComponent<Renderer>().material = materials[num];

      //Set Random Height
      float scaleFactor = Random.Range(0.95f, 1.05f);
      Transform parent = GetComponentInParent<Animator>().gameObject.transform;
      parent.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
      parent.GetComponentInChildren<Light>().transform.parent.localScale = new Vector3(1 / scaleFactor, 1 / scaleFactor, 1 / scaleFactor);
    }
  }
}