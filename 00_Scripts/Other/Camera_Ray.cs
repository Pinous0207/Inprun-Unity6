using UnityEngine;

public class Camera_Ray : MonoBehaviour
{
    Camera cam;
    Hero_Holder holder = null;
    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            
            if(holder != null)
                holder.ReturnRange();

            if (hit.collider != null)
            {
                holder = hit.collider.GetComponent<Hero_Holder>();
                if(holder != null)
                {
                    holder.GetRange();
                }
            }
        }
    }
}
