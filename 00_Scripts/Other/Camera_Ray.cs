using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class Camera_Ray : NetworkBehaviour
{
    Camera cam;
    Hero_Holder holder = null;
    Hero_Holder Move_Holder = null;
    string HostAndClIent = "";
    private void Start()
    {
        cam = Camera.main;
        HostAndClIent = Net_Utils.LocalID() == 0 ? "HOST" : "CLIENT";
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            MouseButtonDown();
        }
        if(Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            MouseButton();
        }

        if(Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            MouseButtonUp();
        }
    }

    private void MouseButtonDown()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (holder != null)
        {
            holder.ReturnRange();
            holder = null;
        }
        if (hit.collider != null)
        {
            holder = hit.collider.GetComponent<Hero_Holder>();
            if(holder.Holder_Name == "" || holder.Holder_Name == null)
            {
                holder = null;
                return;
            }
            bool CanGet = false;
            int value = (int)NetworkManager.Singleton.LocalClientId;
            if (value == 0) CanGet = holder.Holder_Part_Name.Contains("HOST");
            else if (value == 1) CanGet = holder.Holder_Part_Name.Contains("CLIENT");

            if (!CanGet) holder = null;
        }
    }

    // 마우스가 눌리고 있는 동안
    private void MouseButton()
    {
        if(holder != null)
        {
            holder.G_GetClick(true);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if(hit.collider != null && hit.collider.transform != holder.transform)
            {
                if (hit.collider.GetComponent<Hero_Holder>() == null) return;
                if(hit.collider.GetComponent<Hero_Holder>().Holder_Part_Name.Contains(HostAndClIent) == false)
                {
                    return;
                }
                if(Move_Holder != null)
                {
                    Move_Holder.S_SetClick(false);
                }
                Move_Holder = hit.collider.GetComponent<Hero_Holder>();
                Move_Holder.S_SetClick(true);
            }
        }
    }
    private void MouseButtonUp()
    {
        if (holder == null) return;

        if (Move_Holder == null)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                if (holder.transform == hit.collider.transform)
                {
                    holder.GetRange();
                }
            }
        }
        else
        {
            Move_Holder.S_SetClick(false);

            Spawner.instance.Holder_Position_Set(
                holder.Holder_Part_Name, Move_Holder.Holder_Part_Name);
        }

        if (holder != null)
            holder.G_GetClick(false);

        Move_Holder = null;
    }
}
