using UnityEngine;
using System.Collections;
using System.Linq;

public class Randomizer : MonoBehaviour {
	public GameObject[] Muzzles;
	int MuzzleIndex;
	public GameObject[] Host;
	int HostIndex;
	public GameObject[] Founds;
	int FoundIndex;
	public GameObject[] Rounds;
	int RoundIndex;

	//Clean previous result
	public void Clean () {
		var tempList = transform.Cast<Transform>().ToList();
		foreach(var child in tempList)
		{
			DestroyImmediate(child.gameObject);
		}
	}

	//Build the new set

	//Build Muzzle
	public void MuzzleBuild () {
		MuzzleIndex = Random.Range (0, Muzzles.Length);
		GameObject RandomMuzzle = Muzzles [MuzzleIndex];
		GameObject NewMuzzle = Instantiate (RandomMuzzle) as GameObject;
		NewMuzzle.transform.parent = gameObject.transform;
	}

	//Build Top Part
	public void HostBuild () {
		HostIndex = Random.Range (0, Host.Length);
		GameObject RandomHostPart = Host [HostIndex];
		GameObject NewHostPart = Instantiate (RandomHostPart) as GameObject;
		NewHostPart.transform.parent = gameObject.transform;
	}

	//Build Middle Part
	public void FoundBuild () {
		FoundIndex = Random.Range (0, Founds.Length);
		GameObject RandomFoundPart = Founds [FoundIndex];
		GameObject NewFoundPart = Instantiate (RandomFoundPart) as GameObject;
		NewFoundPart.transform.parent = gameObject.transform;
	}

	//Build Back Part
	public void RoundBuild () {
		RoundIndex = Random.Range (0, Rounds.Length);
		GameObject RandomRoundPart = Rounds [RoundIndex];
		GameObject NewRoundPart = Instantiate (RandomRoundPart) as GameObject;
		NewRoundPart.transform.parent = gameObject.transform;
	}
}
