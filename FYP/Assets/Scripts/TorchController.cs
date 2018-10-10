using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchController : MonoBehaviour {
    public GameObject pointLight;
    public GameObject flame;

	public void EnableTorch()
    {
        pointLight.SetActive(true);
        flame.SetActive(true);
    }
}
