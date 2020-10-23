using UnityEngine;
using UnityEngine.UI;

public class RadialProgress : MonoBehaviour
{
	public Image LoadingBar;
	public float progress;

	// Use this for initialization
	void Start()
	{
		LoadingBar.fillAmount = 0.0f;
	}

	// Update is called once per frame
	void Update()
	{
		LoadingBar.fillAmount = progress;
	}
}