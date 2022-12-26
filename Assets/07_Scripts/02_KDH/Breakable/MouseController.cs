using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour                    // 큐브 폭파 테스트용 마우스컨트롤 구현
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit t_hit;
            if(Physics.Raycast(t_ray, out t_hit, 100f))
            {
                if (!t_hit.transform.GetComponent<Cube>())
                    return;

                t_hit.transform.GetComponent<Cube>().Explosion();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            Ray t_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit t_hit;
            if (Physics.Raycast(t_ray, out t_hit, 100f))
            {
                if (!t_hit.transform.GetComponent<BigDoorOpen>())
                    return;

                t_hit.transform.GetComponent<BigDoorOpen>().TryHandle();
            }
        }
    }
}
