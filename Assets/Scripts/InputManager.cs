using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private Transform currentMoveableObject;

    private LayerMask moveableObjects;
    private Camera mainCamera;
    private Ray mouseRay;
    private RaycastHit mouseHitInfo;
    private float mouseRayLenght = 100f;

    private void Start()
    {
        moveableObjects = LayerMask.GetMask("MoveableObject");
        mainCamera = Camera.main;
    }

    private void Update()
    {
        mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            

            if (Physics.Raycast(mouseRay, out mouseHitInfo, mouseRayLenght))
            {
                currentMoveableObject = mouseHitInfo.collider.transform;
               
            }
        }

        if (Input.GetMouseButton(0))
        {
            currentMoveableObject.position = mouseHitInfo.point;
        }

        if (Input.GetMouseButtonUp(0))
        {

        }
    }
}
