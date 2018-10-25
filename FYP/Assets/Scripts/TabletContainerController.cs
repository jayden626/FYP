using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class TabletContainerController : MonoBehaviour {
    private VRTK_SnapDropZone parentZone;
    public PuzzleController puzzleController;

    public string correctText;
    public string currentText;
    public GameObject snappedTile;
    public Material correctMaterial;
    public Material incorrectMaterial;

    public GameObject previousSnapZone;
    public GameObject nextSnapZone;

    private void Start()
    {
        parentZone = this.gameObject.GetComponent<VRTK_SnapDropZone>();
        parentZone.ObjectSnappedToDropZone += ObjectSnappedToDropZone;
        parentZone.ObjectUnsnappedFromDropZone += ObjectUnsnappedFromDropZone;
    }

    public bool isCorrect()
    {
        return snappedTile != null && correctText.Equals(currentText);
    }

    private void ObjectSnappedToDropZone(object o, SnapDropZoneEventArgs e)
    {
        this.snappedTile = e.snappedObject;
        this.currentText = e.snappedObject.GetComponentInChildren<Text>().text;
        puzzleController.UpdateMonitor();
    }

    private void ObjectUnsnappedFromDropZone(object o, SnapDropZoneEventArgs e)
    {
        this.snappedTile = null;
        this.currentText = "";
        puzzleController.UpdateMonitor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.tag.Equals("SolutionTablet"))
        {
            return;
        }

        Vector3 position = gameObject.GetComponent<SphereCollider>().transform.position - other.transform.position;

        //If collision on left, push right
        if (position.x > 0)
        {
            //search left for empty
            swapTilesRight();

        }
        else if (position.x < 0)
        {
            swapTilesLeft();
        }
    }

    public void swapTilesRight()
    {
        if(nextSnapZone == null || snappedTile == null)
        {
            return;
        }
        nextSnapZone.GetComponent<TabletContainerController>().swapTilesRight();

        if (nextSnapZone.GetComponent<TabletContainerController>().snappedTile == null)
        {
            GameObject currentTile = snappedTile;
            gameObject.GetComponent<VRTK_SnapDropZone>().ForceUnsnap();
            nextSnapZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(currentTile);
        }
    }

    public void swapTilesLeft()
    {
        if (previousSnapZone == null || snappedTile == null)
        {
            return;
        }
        previousSnapZone.GetComponent<TabletContainerController>().swapTilesLeft();

        if (previousSnapZone.GetComponent<TabletContainerController>().snappedTile == null)
        {
            GameObject currentTile = snappedTile;
            gameObject.GetComponent<VRTK_SnapDropZone>().ForceUnsnap();
            previousSnapZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(currentTile);
        }
    }
}
