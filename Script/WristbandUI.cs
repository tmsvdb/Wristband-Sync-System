using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wristband;

public class WristbandUI : MonoBehaviour {

	[Header("Control panel components")]
	public Button connectButton;
	public Button disconnectButton;
	public Button getStepsButton;
	public Button clearButton;

	[Header("Write settings panel components")]
	public Toggle isKids;
	public InputField stepTargetField;
	public InputField distancePerStepField;
	public InputField weightField;
	public InputField heightField;
	public InputField genderField;
	public InputField dateOfBirthField;
	public Button writeButton;
	
	[Header("Status panel components")]
	public Toggle initToggle;
	public Toggle connectedToggle;
	public Toggle hasDeviceToggle;
	public Toggle profileToggle;
	public Toggle historyToggle;
	public Toggle busyToggle;

	[Header("Availeble devices components")]
	public Text devicesText;

	[Header("Events components")]
	public Text eventsText;
	

	// Use this for initialization
	void Start () {
		// set Control panel button handlers
		connectButton.onClick.AddListener(OnConnectButtonClick);
		disconnectButton.onClick.AddListener(OnDisconnectButtonClick);
		getStepsButton.onClick.AddListener(OnGetStepsButtonClick);
		clearButton.onClick.AddListener(OnClearButtonClick);
		// write wristband settings -> test data
		isKids.isOn = true;
		stepTargetField.text = "10000";
		distancePerStepField.text = "500000";
		weightField.text = "70.0";
		heightField.text = "180.0";
		genderField.text = "m";
		dateOfBirthField.text = "1980-07-01";
		writeButton.onClick.AddListener(OnWriteButtonClick);
	}
	
	// Update is called once per frame
	void Update () {
		bool[] states = Manager.WristbandStates();
		initToggle.isOn = states[0];
		connectedToggle.isOn = states[1];
		hasDeviceToggle.isOn = states[2];
		profileToggle.isOn = states[3];
		historyToggle.isOn = states[4];
		busyToggle.isOn = states[5];

		if(genderField.text != "m" && genderField.text != "f") 
			genderField.text = "o";

		devicesText.text = "";
		foreach (string device in Manager.WristbandDeviceList())
			devicesText.text += "- " + device + "\n";

		eventsText.text = "";
		List<WristbandHistoryEvent> h = Manager.EventHistory;
		int startpos = h.Count > 9 ? h.Count-10 : 0;
		for(int i = h.Count -1; i > startpos; i--) {
			eventsText.text += (h[i].isError?"ERROR: ":"msg: ") + h[i].msg + "\n"; 
		}
	}

	// Control panel handlers
	// ======================
	void OnConnectButtonClick () {
		Manager.ConnectToWristband();
	}

	void OnDisconnectButtonClick () {
		Manager.AbortWristband();
	}

	void OnGetStepsButtonClick () {
		Manager.GetStepsFromWristband();
	}

	void OnClearButtonClick () {
		Manager.RemoveHistoryFormWristband();
	}

	// Write panel handlers
	// ======================
	void OnWriteButtonClick () {

		bool is_kids = isKids.isOn;
		int steps_target = CheckStepsTarget ();
		int distance_per_step = CheckDistancePerStep();
		float weight = CheckWeight();
		float height = CheckHeight();
		string gender = genderField.text;
		string date_of_birth = CheckDateOfBirth();

		Manager.WriteWristbandSettings(
			Manager.CreateNewWristbandSettings (
				is_kids,
				steps_target,
				distance_per_step,
				weight,
				height,
				gender,
				date_of_birth
			)
		);
	}

	int CheckStepsTarget () {
		int steps_target = 0;
		if (int.TryParse(stepTargetField.text, out steps_target))
			return steps_target;
		else {
			Debug.LogWarning("Could not parse stepTargetField, returned 10000");
			stepTargetField.text = "10000";
			return 10000;
		}	
	}

	int CheckDistancePerStep() {
		int distance = 0;
		if (int.TryParse(distancePerStepField.text, out distance))
			return distance;
		else {
			Debug.LogWarning("Could not parse distancePerStepField, returned 500000");
			distancePerStepField.text = "500000";
			return 500000;
		}	
	}

	float CheckWeight() {
		float weight = 0;
		if (float.TryParse(weightField.text, out weight))
			return weight;
		else {
			Debug.LogWarning("Could not parse weightField, returned 70.0");
			weightField.text = "70.0";
			return 70.0f;
		}	
	}

	float CheckHeight() {
		float height = 0;
		if (float.TryParse(heightField.text, out height))
			return height;
		else {
			Debug.LogWarning("Could not parse heightField, returned 180.0");
			heightField.text = "180.0";
			return 180.0f;
		}	
	}

	string CheckDateOfBirth () {
		string[] d = dateOfBirthField.text.Split('-');
		if (d.Length == 3 && d[0].Length == 4 && d[1].Length == 2 && d[2].Length == 2)
		{
			int year, month, day;
			if(
				int.TryParse(d[0], out year) &&
				int.TryParse(d[1], out month) &&
				int.TryParse(d[2], out day)
			) {
				// everything is in order
			}
			else {
				Debug.LogWarning("Could not parse heightField, returned '1980-07-01'");
				dateOfBirthField.text = "1980-07-01";
				
			}	
		} else {
			Debug.LogWarning("Could not parse heightField, returned '1980-07-01'");
			dateOfBirthField.text = "1980-07-01";
		}

		return dateOfBirthField.text;
	}

	// Manager instance
	// ======================
	WristbandManager Manager { get { return WristbandManager.Instance;} }
}
