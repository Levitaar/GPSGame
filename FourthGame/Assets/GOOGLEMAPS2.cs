using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GOOGLEMAPS2 : MonoBehaviour
{
	public enum MapType
	{
		RoadMap,
		Satellite,
		Terrain,
		Hybrid
	}
	public bool loadOnStart = true;
	public bool autoLocateCenter = false;
	public GoogleMapLocation centerLocation;
	public int zoom = 13;
	public float count;
	public MapType mapType;
	public int size = 512;
	public bool doubleResolution = false;
	public GoogleMapMarker[] markers;
	public GoogleMapPath[] paths;
	 float lat, lon;

	void Start() {
		
		Input.location.Start ();
		if (loadOnStart) {
			Refresh ();
		}
	}

	void Update() {
		
		UpdateLocation ();
		InthePark ();
		count += Time.deltaTime;
		
		if (count > 2) {
				Refresh();
				count = 0;
		}
	}

	void UpdateLocation() {
		
		if (Input.location.isEnabledByUser) {
		
			lat = Input.location.lastData.latitude;
			lon = Input.location.lastData.longitude;
		
			GameObject Lat = GameObject.Find ("LatText");
			GameObject Lon = GameObject.Find ("LongText");
		
			Lat.GetComponent<Text> ().text = "Lat:" + lat;
			Lon.GetComponent<Text> ().text = "Lon:" + lon;
		}
	}

	void InthePark() {

		if (Input.location.isEnabledByUser) {
	
			GameObject infobox = GameObject.Find ("Text");
	
			if (lat > 40.800678f && 40.764278f < lat && lon > -73.958233f && -73.973015f > lon) {
					
				infobox.GetComponent<Text> ().enabled = true;
				infobox.GetComponent<Text> ().text = "Outside of the Park";
			} else {
	
				infobox.GetComponent<Text> ().enabled = false;
			}
		}
	}

	public float Remap (float value, float from1, float to1, float from2, float to2) {

		return(value - from1) / (to1 - from1) * (to2 - from2) + from2;
	}

	[ContextMenu ("Refresh")]
	public void Refresh() {
		if(autoLocateCenter && (markers.Length == 0 && paths.Length == 0)) {
			Debug.LogError("Auto Center will only work if paths or markers are used.");
		}
		StartCoroutine(_Refresh());
	}

	IEnumerator _Refresh () {

		string url = "http://maps.googleapis.com/maps/api/staticmap";
		string qs = CreateURLString ();
		//send query to google maps and yield until there's a reply
		var req = new WWW (url + "?" + qs);	
		print (qs);
		yield return req;

		if (GetComponent<CanvasRenderer> ()) {
			Sprite sprite = new Sprite ();
			float ratio = Screen.height /500;
			Vector2 screensize = new Vector2 (500/ratio,500);
			GetComponent<Image> ().sprite = Sprite.Create (req.texture, new Rect (0,0,500, 500), Vector2.zero);
		} else {
			GetComponent<Renderer> ().material.mainTexture = req.texture;
		}

	}

	string CreateURLString () {
		string qs = "";
		qs += BaseRequest ();
		qs += DeviceSensor ();
		qs += MarkersAndPaths ();
		Debug.Log (qs);
		return qs;
	}

	string BaseRequest () {
		string qs = "";
		if (!autoLocateCenter) {
			if (centerLocation.address != "")
				qs += "center=" + WWW.UnEscapeURL (centerLocation.address);
			else {
				qs += "center=" + WWW.UnEscapeURL (string.Format ("{0},{1}", centerLocation.latitude, centerLocation.longitude));
			}

			qs += "&zoom=" + zoom.ToString ();
		}

		float ratio = Screen.height /500;
		Vector2 screensize = new Vector2 (500/ratio,500);

		print (screensize);

		qs += "&size=" + WWW.UnEscapeURL (string.Format ("{0}x{1}",500, 500));
		qs += "&scale=" + (doubleResolution ? "2" : "1");
		qs += "&maptype=" + mapType.ToString ().ToLower ();
		return qs;
	}

	string DeviceSensor () {
		bool usingSensor = false;
		//		#if UNITY_IPHONE
		usingSensor = Input.location.isEnabledByUser && Input.location.status == LocationServiceStatus.Running;
		//		#endif

		//		I
		return "&sensor=" + (usingSensor ? "true" : "false");
	}

	string MarkersAndPaths () {
		string qs = "";
		foreach (GoogleMapMarker i in markers) {
			qs += "&markers=" + string.Format ("size:{0}|color:{1}|label:{2}", i.size.ToString ().ToLower (), i.color, i.label);

			foreach (GoogleMapLocation loc in i.locations) {
				if (loc.address != "")
					qs += "|" + WWW.UnEscapeURL (loc.address);
				else
					qs += "|" + WWW.UnEscapeURL (string.Format ("{0},{1}", loc.latitude, loc.longitude));
			}
		}

		foreach (GoogleMapPath i in paths) {
			qs += "&path=" + string.Format ("weight:{0}|color:{1}", i.weight, i.color);

			if(i.fill) 
				qs += "|fillcolor:" + i.fillColor;

			foreach (GoogleMapLocation loc in i.locations) {
				if (loc.address != "")
					qs += "|" + WWW.UnEscapeURL (loc.address);
				else
					qs += "|" + WWW.UnEscapeURL (string.Format ("{0},{1}", loc.latitude, loc.longitude));
			}
		}
		return qs;
	}
}

public enum GoogleMapColor
{
	black,
	brown,
	green,
	purple,
	yellow,
	blue,
	gray,
	orange,
	red,
	white
}

[System.Serializable]
public class GoogleMapLocation
{
	public string address;
	public float latitude;
	public float longitude;
}

[System.Serializable]
public class GoogleMapMarker
{
	public enum GoogleMapMarkerSize
	{
		Tiny,
		Small,
		Mid
	}
	public GoogleMapMarkerSize size;
	public GoogleMapColor color;
	public string label;
	public GoogleMapLocation[] locations;

}

[System.Serializable]
public class GoogleMapPath
{
	public int weight = 5;
	public GoogleMapColor color;
	public bool fill = false;
	public GoogleMapColor fillColor;
	public GoogleMapLocation[] locations;
}