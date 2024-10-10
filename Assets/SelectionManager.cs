using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelectionManager : MonoBehaviour
{
    public LayerMask unitLayer;
    public LayerMask groundLayer;
    public RectTransform selectionBox; // UI上の選択ボックス
    private Vector2 startPosition;
    private Vector2 endPosition;
    private Camera mainCamera;
    private List<GameObject> selectedUnits = new List<GameObject>();

    void Start()
    {
        mainCamera = Camera.main;
        selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            startPosition = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);
        }
        if (Input.GetMouseButton(0))
        {
            endPosition = Input.mousePosition;
            UpdateSelectionBox();
        }        
        if (Input.GetMouseButtonUp(0))
        {
            selectionBox.gameObject.SetActive(false);
            SelectUnitsInBox();
        }
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            MoveSelectedUnits();
        }
    }
    void UpdateSelectionBox()
    {
        Vector2 boxStart = startPosition;
        Vector2 boxEnd = endPosition;
        Vector2 boxCenter = (boxStart + boxEnd) / 2;
        Vector2 boxSize = new Vector2(Mathf.Abs(boxStart.x - boxEnd.x), Mathf.Abs(boxStart.y - boxEnd.y));

        selectionBox.position = boxCenter;
        selectionBox.sizeDelta = boxSize;
    }

    void SelectUnitsInBox()
    {
        selectedUnits.Clear();
        Rect selectionRect = new Rect(startPosition.x, startPosition.y, endPosition.x - startPosition.x, endPosition.y - startPosition.y);

        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
            if (selectionRect.Contains(screenPos, true))
            {
                selectedUnits.Add(unit);
                Debug.Log("Unit selected: " + unit.name);
            }
        }
    }
    void MoveSelectedUnits()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            Vector3 targetPosition = hit.point;
            Vector3 centerOffset = targetPosition - GetUnitsCenter();

            foreach (GameObject unit in selectedUnits)
            {
                NavMeshAgent navMeshAgent = unit.GetComponent<NavMeshAgent>();
                Vector3 moveToPosition = unit.transform.position + centerOffset;
                navMeshAgent.SetDestination(moveToPosition);
            }
        }
    }

    Vector3 GetUnitsCenter()
    {
        if (selectedUnits.Count == 0) return Vector3.zero;

        Vector3 sumPosition = Vector3.zero;
        foreach (GameObject unit in selectedUnits)
        {
            sumPosition += unit.transform.position;
        }
        return sumPosition / selectedUnits.Count;
    }
}
//赤いImageをドラッグしたところに作る
//半透明（デスクトップのドラッグした時に出る青い枠に近い）
//Image内に入ったオブジェクトを選択したことにし、距離を保ち移動
//左クリックで選択、右クリックで実際の移動をする