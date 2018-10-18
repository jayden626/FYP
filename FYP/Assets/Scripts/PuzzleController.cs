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
    public GameObject[] indicators;
    public string[] sentence;
    private int currentPuzzle;

    // Use this for initialization
    void Start()
    {
        currentPuzzle = 0;
        SetUpSlidingTilePuzzle();
        //SetUpPuzzlePunctuationOnOwnTile();
    }

    public void UpdateMonitor()
    {
        if(firstZone != null)
        {
            string text = "";
            GameObject currentZone = firstZone;
            while(currentZone != null)
            {
                TabletContainerController tcc = currentZone.GetComponent<TabletContainerController>();
                if (currentZone.GetComponent<TabletContainerController>().snappedTile != null)
                {
                    //Add a space before words, except after a semi colon
                    if (!currentZone.GetComponent<TabletContainerController>().currentText.Equals(";"))
                    {
                        text += " ";
                    }
                    text += currentZone.GetComponent<TabletContainerController>().currentText;
                }
                currentZone = currentZone.GetComponent<TabletContainerController>().nextSnapZone;
            }
            monitor.GetComponentInChildren<Text>().text = text;
        }
    }

    private void ObjectSnappedToDropZone(object o, SnapDropZoneEventArgs e)
    {
        if (showSolution)
        {
            isSolved();
        }
    }

    private void ObjectUnsnappedFromDropZone(object o, SnapDropZoneEventArgs e)
    {
        if (showSolution)
        {
            e.snappedObject.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    public bool isSolved()
    {
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
        if(isCorrect)
        {
            showSolution = false;
            DestroyPuzzle();
            indicators[currentPuzzle].GetComponent<Renderer>().material.SetColor( "_Color", Color.green);
            currentPuzzle += 1;
            if(currentPuzzle < sentence.Length)
            {
                SetUpSlidingTilePuzzle();
            }
        }
        else
        {
            showSolution = true;
        }

        return isCorrect;
    }

    public bool isComplete()
    {
        isSolved();
        if(currentPuzzle >= sentence.Length)
        {
            return true;
        }
        return false;
    }

    void DestroyPuzzle()
    {
        GameObject currentZone = firstZone;
        firstZone = null;
        while(currentZone != null)
        {
            GameObject nextZone = currentZone.GetComponent<TabletContainerController>().nextSnapZone;
            GameObject tile = currentZone.GetComponent<TabletContainerController>().snappedTile;

            currentZone.GetComponent<VRTK_SnapDropZone>().ForceUnsnap();
            Destroy(currentZone.GetComponent<VRTK_SnapDropZone>());
            Destroy(tile);
            currentZone = nextZone;
        }
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
        string[] words = Regex.Matches(sentence[currentPuzzle], @"[\w'.,:?$!]+|[;]").Cast<Match>().Select(p => p.Value).ToArray();
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
            newSolutionZone.GetComponent<TabletContainerController>().puzzleController = this;
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectSnappedToDropZone += ObjectSnappedToDropZone;
            newSolutionZone.GetComponent<VRTK_SnapDropZone>().ObjectUnsnappedFromDropZone += ObjectUnsnappedFromDropZone;

            if (Regex.Matches(words[i], @"[;]").Count > 0)
            {
                semicolonHit = true;
            }

            //If it isn't the last word. For the last word, we need to leave it empty (unless no semicolon was seen)
            if (i < words.Length - 1 || !semicolonHit )
            {
                GameObject newSolutionTablet = Instantiate(puzzleTile, new Vector3(upperBound.x + (tabletSize.x * xCounter) + (tileSpacing * xCounter),
                                                                                upperBound.y - (tabletSize.y * yCounter) - (tileSpacing * yCounter),
                                                                                upperBound.z), Quaternion.identity, this.transform);

                newSolutionZone.GetComponent<VRTK_SnapDropZone>().ForceSnap(newSolutionTablet);

                //Set text on both sides of tile
                //If we have seen a semicolon, we need to +1 to the word count to skip the semicolon
                Text[] textAreas = newSolutionTablet.GetComponentsInChildren<Text>();
                for (int j = 0; j < textAreas.Length; j++)
                {
                    if (semicolonHit)
                    {
                        textAreas[j].text = words[i + 1];
                    }
                    else
                    {
                        textAreas[j].text = words[i];
                    }
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
        string[] words = Regex.Matches(sentence[currentPuzzle], @"[\w']+|[.,!?;]").Cast<Match>().Select(p => p.Value).ToArray();

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
