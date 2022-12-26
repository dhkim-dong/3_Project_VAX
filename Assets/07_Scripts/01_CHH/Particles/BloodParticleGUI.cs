using UnityEngine;
using System.Collections;

public class BloodParticleGUI : MonoBehaviour {

	[SerializeField] private Transform bloodPos;	// 파티클 위치 휴먼로이드 상의 쪽
	public GameObject[] bloodParticle;					// 파티클 이미지
	private int currentNomber;						// 파티클 첫 이미지 부터 시작
	public GameObject blood;						// Start 매서드에서 피 생성됨
	private GUIStyle guiStyleHeader = new GUIStyle();
	private float deley;
	
	// 컴포넌트 초기화
	void Start () {
        // guiStyleHeader.fontSize = 15;
        // guiStyleHeader.normal.textColor = new Color(0.15f,0.15f,0.15f);
        currentNomber = Random.Range(0, bloodParticle.Length);
        blood = Instantiate(bloodParticle[currentNomber], transform.position, new Quaternion()) as GameObject;
        blood.SetActive(false);
        blood.transform.SetParent(bloodPos.transform, true);
		blood.name = "Blood";
		var reactivator = blood.AddComponent<BloodParticleReactivator>();
		reactivator.tag = gameObject.tag;
		//blood.SetActive(true);
	}

	private void Update ()
	{
        blood.transform.position = bloodPos.position;

		if(tag == "Player")
		{
            blood.transform.rotation = transform.rotation;
        }
		else if(tag == "Enemy")
		{
            float randX = Random.Range(0f, 180f);
            float randY = Random.Range(0f, 180f);
            float randZ = Random.Range(0f, 180f);
            blood.transform.Rotate(new Vector3(randX, randY, randZ) * 3 * Time.deltaTime); ;
        }
        blood.transform.rotation = transform.rotation;
    }

	// 피 파티클 그리기
	private void OnGUI()
	{
/*		if (GUI.Button(new Rect(10, 15, 105, 30), "Previous Effect")) {
			ChangeCurrent(-1);
		}
		if (GUI.Button(new Rect(130, 15, 105, 30), "Next Effect"))
		{
			ChangeCurrent(+1);
		}*/
		//GUI.Label(new Rect(300, 15, 100, 20), "Prefab name is \"" + Prefabs[currentNomber].name + "\"  \r\nHold any mouse button that would move the camera", guiStyleHeader);
	}
	
	// 파티클 이미지 교체
	void ChangeCurrent(int delta) {
		currentNomber+=delta;
		if (currentNomber> bloodParticle.Length - 1)
			currentNomber = 0;
		else if (currentNomber < 0)
			currentNomber = bloodParticle.Length - 1;
		if(blood!=null) Destroy(blood);
		blood = Instantiate(bloodParticle[currentNomber], transform.position, new Quaternion()) as GameObject;
		var reactivator = blood.AddComponent<BloodParticleReactivator>();

		deley = Random.Range(0.2f, 1.5f);
	}
}
