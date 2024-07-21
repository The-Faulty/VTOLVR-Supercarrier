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
    }
  }
}