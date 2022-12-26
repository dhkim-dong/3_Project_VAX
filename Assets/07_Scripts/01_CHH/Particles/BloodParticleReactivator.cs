using UnityEngine;
using System.Collections;

public class BloodParticleReactivator : MonoBehaviour {

    private float deley;

    void Start()
    {
        Repeating();
    }

    // 피 파티클 한번만 재생
    public void Reactivate ()
	{
	  var childs = GetComponentsInChildren<Transform>();
	  foreach (var child in childs) {
	    child.gameObject.SetActive(false);
		child.gameObject.SetActive(true);
	  }
	}

	// 피 파티클 딜레이 시간 가진 후 계속 재생
	public void Repeating()
	{
        if (gameObject.tag == "Player")
        {
            deley = 0.8f;
        }
        else if (gameObject.tag == "Enemy")
        {
            deley = 1f;
        }

        InvokeRepeating("Reactivate", 0, deley);
    }

	// 파티클 비활성화
	public void Stop()
	{
		CancelInvoke("Reactivate");
    }
}
