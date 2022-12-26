using UnityEngine;
using System.Collections;

public class BloodParticleReactivator : MonoBehaviour {

    private float deley;

    void Start()
    {
        Repeating();
    }

    // �� ��ƼŬ �ѹ��� ���
    public void Reactivate ()
	{
	  var childs = GetComponentsInChildren<Transform>();
	  foreach (var child in childs) {
	    child.gameObject.SetActive(false);
		child.gameObject.SetActive(true);
	  }
	}

	// �� ��ƼŬ ������ �ð� ���� �� ��� ���
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

	// ��ƼŬ ��Ȱ��ȭ
	public void Stop()
	{
		CancelInvoke("Reactivate");
    }
}
