using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class SyncLocationInteraction : MonoBehaviour
{
	int _locationId;

	public Button _syncButton;
	public Text _syncLocationText;
	public Image _syncLocationImage;

	public Sprite _syncPointIcon;
	public Sprite _conferenceRoomIcon;
	public Sprite _phoneRoomIcon;

	public event Action<int> OnSyncLocationInteraction = delegate { };

	void Awake()
	{
		_syncButton.onClick.AddListener(SyncLocation);
	}


	public static Color hexToColor(string hex)
	{
		hex = hex.Replace("0x", "");//in case the string is formatted 0xFFFFFF
		hex = hex.Replace("#", "");//in case the string is formatted #FFFFFF
		byte a = 255;//assume fully visible unless specified in hex
		byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
		//Only use alpha if the string has enough characters
		if (hex.Length == 8)
		{
			a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
		}
		return new Color32(r, g, b, a);
	}
	public void Register(int location, string label, Action<int> callback, string type = null)
	{
		//_syncButton.onClick.AddListener(SyncLocation);
		_locationId = location;

		OnSyncLocationInteraction += callback;

		_syncLocationText.text = string.IsNullOrEmpty(label) ? location.ToString() : label;

		if (type != null)
		{
			Debug.Log("Setting types");


			//if (type == "conference-room")
			//{
			//	_syncLocationImage.sprite = _conferenceRoomIcon;
			//}
			if (type == "phone-room")
			{
				_syncLocationImage.sprite = _phoneRoomIcon;
			}
			else
			{
				var iconColor = hexToColor(type);
				_syncLocationImage.color = iconColor;
			}
		}
	}

	private void SyncLocation()
	{
		Debug.Log("Button OnClick");
		OnSyncLocationInteraction(_locationId);
	}
}
