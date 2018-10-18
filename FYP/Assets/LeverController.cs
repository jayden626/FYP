using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.Controllables.ArtificialBased;

public class LeverController : MonoBehaviour {

    public VRTK_InteractableObject linkedObject;
    public VRTK_ArtificialRotator lever;

    protected virtual void OnEnable()
    {
        linkedObject = (linkedObject == null ? GetComponent<VRTK_InteractableObject>() : linkedObject);

        if (linkedObject != null)
        {
            linkedObject.InteractableObjectUsed += InteractableObjectUsed;
            linkedObject.InteractableObjectUnused += InteractableObjectUnused;
        }
        lever = this.GetComponentInChildren<VRTK_ArtificialRotator>();

    }

    protected virtual void OnDisable()
    {
        if (linkedObject != null)
        {
            linkedObject.InteractableObjectUsed -= InteractableObjectUsed;
            linkedObject.InteractableObjectUnused -= InteractableObjectUnused;
        }
    }

    protected virtual void InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        print("Ding used");
        lever.SetAngleTarget(90.0f, 0.1f);
    }

    protected virtual void InteractableObjectUnused(object sender, InteractableObjectEventArgs e)
    {
        print("Dong unused");
        lever.SetAngleTarget(0.0f, 0.1f);
    }
}