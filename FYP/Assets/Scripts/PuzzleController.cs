using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;
using VRTK;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour {

    public GameObject plane;
    public GameObject tablet;
    public GameObject puzzleTile;
    public GameObject solutionSnapDropZone;
    public GameObject monitor;
    private GameObject firstZone;

    //public GameObject solutionTileSpawnLocation;

    public float leftMargin = 0.0f;
    public float topMargin = 0.0f;
    public float tileSpacing = 0.0f;

    public bool showSolution;

    public string sentence;

    // Use this for initialization
    void Start()
    {
        SetUpSlidingTilePuzzle();
        //SetUpPuzzlePunctuationOnOwnTile();
    }

    private void UpdateMonitor()
    {
        if(firstZone != null)
        {
            string text = "";
            GameObject currentZone = firstZone;
            while(currentZone != null)
            {
                if(currentZone.GetComponent<TabletContainerController>().snappedTile != null && !currentZone.GetComponent<TabletContainerController>().currentText.Equals(";"))
                {
                    text += " ";
                }
                text += currentZone.GetComponent<TabletContainerController>().currentText;
                currentZone = currentZone.GetComponent<TabletContainerController>().nextSnapZone;
            }
            monitor.GetComponentInChildren<Text>().text = text;
            print(text);
        }
    }

    private void ObjectSnappedToDropZone(object o, SnapDropZoneEventArgs e)
    {
        if (showSolution)
        {
            isSolved();
        }
        UpdateMonitor();
    }

    private void ObjectUnsnappedFromDropZone(object o, SnapDropZoneEventArgs e)
    {
        if (showSolution)
        {
            e.snappedObject.GetComponent<MeshRenderer>().material.color = Color.white;
        }
        UpdateMonitor();
    }

    public bool isSolved()
    {
        showSolution = true;
        bool isCorrect = true;
        VRTK_SnapDropZone[] solutionZones = this.GetComponentsInChildren<VRTK_SnapDropZone>();
        for(int i=0; i<solutionZones.Length; i++)
        {
            TabletContainerController tabletContainerController = solutionZones[i].GetComponent<TabletContainerController>();
            if( tabletContainerController.snappedTile == null)
            {
                isCorrect = false;
            }
            else if (tabletContainerController.isCorrect())
            {
                tabletContainerController.snappedTile.GetComponent<MeshRenderer>().material.color = Color.green;
            }
            else
            {
                isCorrect = false;
                tabletContainerController.snappedTile.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
        return isCorrect;
    }

    void SetUpSlidingTilePuzzle()
    {
        //Get puzzle dimensions
        Vector3 boxCentre = plane.transform.position;
        Vector3 boxSize = plane.GetComponent<Renderer>().bounds.size;
        Vector3 tabletSize = tablet.GetComponent<Renderer>().bounds.size;
        Vector3 upperBound = new Vector3(boxCentre.x - (boxSize.x / 2) + (tabletSize.x / 2) + leftMargin,
                                         boxCentre.y + (boxSize.y / 2) - (tabletSize.y / 2) - topMargin,
                                         boxCentre.z);
        Transform transform = plane.transform;

        //Split sentence and place on board
        string[] words = Regex.Matches(sentence, @"[\w'.,:?]+|[;]").Cast<Match>().Select(p => p.Value).ToArray();
        GameObject previousSnapZone = null;
        bool semicolonHit = false;
        int xCounter = 0;
        int yCounter = 0;
        for (int i = 0; i < words.Length; i++)
        {
            print("Word:" + words[i]);
            print("Tab pos:" + (upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter)) + "Box:" + plane.GetComponent<Renderer>().bounds.max.x);
            //If tile will be off the edge of the tile zone, reset the x counter and drop the tile down in the y direction
            if ((upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter) + (tabletSize.x / 2)) > plane.GetComponent<Renderer>().bounds.max.x)
            {
                xCounter = 0;
                yCounter++;
            }

            GameObject newSolutionZone = Instantiate(solutionSnapDropZone, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                                       upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                                       upperBound.z), Quaternion.identity, this.transform);
            if(i==0)
            {
                firstZone = newSolutionZone;
            }

            newSolutionZone.GetComponent<TabletContainerController>().correctText = words[i];
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectSnappedToDropZone += ObjectSnappedToDropZone;
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectUnsnappedFromDropZone += ObjectUnsnappedFromDropZone;

            if (Regex.Matches(words[i], @"[;]").Count > 0)
            {
                semicolonHit = true;
            }
            else
            {
                GameObject newSolutionTablet = Instantiate(puzzleTile, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                               upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                               upperBound.z), Quaternion.identity, this.transform);
/*                if(semicolonHit && previousSnapZone != null)
                {
                    previousSnapZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(newSolutionTablet);
                }
                else
                {
                }*/
                newSolutionZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(newSolutionTablet);
                //newSolutionZone.GetComponent<TabletContainerController>().snappedTile = newSolutionTablet;

                //Set text on both sides of tile
                TextMeshPro[] textAreas = newSolutionTablet.GetComponentsInChildren<TextMeshPro>();
                for (int j = 0; j < textAreas.Length; j++)
                {
                    textAreas[j].text = words[i];
                }
            }

            if (previousSnapZone != null)
            {
                previousSnapZone.GetComponent<TabletContainerController>().nextSnapZone = newSolutionZone;
            }
            newSolutionZone.GetComponent<TabletContainerController>().previousSnapZone = previousSnapZone;
            previousSnapZone = newSolutionZone;

            xCounter++;
        }
        //Push tiles to the left, leaving last tile as empty
        previousSnapZone.GetComponent<TabletContainerController>().swapTilesLeft();

        /*
        //If is a punctuation tile, spawn an interactable tile in a SnapDropZone
        if (Regex.Matches(words[i], @"[;]").Count>0)
        {
            GameObject newSolutionZone = Instantiate(solutionSnapDropZone, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                         upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                         upperBound.z), Quaternion.identity, this.transform);
            GameObject newSolutionTablet = Instantiate(puzzleTile, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                         upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                         upperBound.z), Quaternion.identity, this.transform);
            TextMeshPro[] textAreas = newSolutionTablet.GetComponentsInChildren<TextMeshPro>();
            string tileText = words[i];
            if( words[i].Equals(";"))
            {
                tileText = ".";
            }
            for( int j=0; j<textAreas.Length; j++ )
            {
                textAreas[j].text = tileText;
            }
            newSolutionZone.GetComponent<TabletContainerController>().correctText = words[i];
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectSnappedToDropZone += ObjectSnappedToDropZone;
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectUnsnappedFromDropZone += ObjectUnsnappedFromDropZone;
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(newSolutionTablet);
        }
        else
        {
            GameObject newTablet = Instantiate(tablet, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                   upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                   upperBound.z), Quaternion.identity, this.transform);
            newTablet.GetComponentInChildren<TextMeshPro>().text = words[i];
        }

        xCounter++;
    }*/
    }

    //An old version of the puzzle. Each tile is a word or a punctuation, with the punctuation being interactable and their own tile
    void SetUpPuzzlePunctuationOnOwnTile()
    {
        //Get puzzle dimensions
        Vector3 boxCentre = plane.transform.position;
        Vector3 boxSize = plane.GetComponent<Renderer>().bounds.size;
        Vector3 tabletSize = tablet.GetComponent<Renderer>().bounds.size;
        Vector3 upperBound = new Vector3(boxCentre.x - (boxSize.x / 2) + (tabletSize.x / 2) + leftMargin,
                                         boxCentre.y + (boxSize.y / 2) - (tabletSize.y / 2) - topMargin,
                                         boxCentre.z);
        Transform transform = plane.transform;

        //Split sentence and place on board
        string[] words = Regex.Matches(sentence, @"[\w']+|[.,!?;]").Cast<Match>().Select(p => p.Value).ToArray();

        int xCounter = 0;
        int yCounter = 0;
        for (int i = 0; i < words.Length; i++)
        {
            print("Word:" + words[i]);
            print("Tab pos:" + (upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter)) + "Box:" + plane.GetComponent<Renderer>().bounds.max.x);
            //If tile will be off the edge of the tile zone, reset the x counter and drop the tile down in the y direction
            if ((upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter) + (tabletSize.x / 2)) > plane.GetComponent<Renderer>().bounds.max.x)
            {
                xCounter = 0;
                yCounter++;
            }

            //If is a punctuation tile, spawn an interactable tile in a SnapDropZone
            if (Regex.Matches(words[i], @"[.!?;,\\-]").Count > 0)
            {
                GameObject newSolutionZone = Instantiate(solutionSnapDropZone, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                             upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                             upperBound.z), Quaternion.identity, this.transform);
                GameObject newSolutionTablet = Instantiate(puzzleTile, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                             upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                             upperBound.z), Quaternion.identity, this.transform);
                TextMeshPro[] textAreas = newSolutionTablet.GetComponentsInChildren<TextMeshPro>();
                string tileText = words[i];
                if (words[i].Equals(";"))
                {
                    tileText = ".";
                }
                for (int j = 0; j < textAreas.Length; j++)
                {
                    textAreas[j].text = tileText;
                }
                newSolutionZone.GetComponent<TabletContainerController>().correctText = words[i];
                newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectSnappedToDropZone += ObjectSnappedToDropZone;
                newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectUnsnappedFromDropZone += ObjectUnsnappedFromDropZone;
                newSolutionZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(newSolutionTablet);
            }
            else
            {
                GameObject newTablet = Instantiate(tablet, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                       upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                       upperBound.z), Quaternion.identity, this.transform);
                newTablet.GetComponentInChildren<TextMeshPro>().text = words[i];
            }

            xCounter++;
        }
    }
}
