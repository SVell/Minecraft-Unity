using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

namespace SVell.Minecraft.UI {
	public class TitleMenu : MonoBehaviour
	{
		public GameObject mainMenuObject;
		public GameObject settingsMenuObject;

		[Header("MainMenu UI Elements")] 
		public TextMeshProUGUI seedField;

		private Settings settings;

		[Header("Settingsmenu UI Elements")] 
		public Slider viewDistSlider;
		public TextMeshProUGUI viewDist;
		public Slider mouseSlider;
		public TextMeshProUGUI mouseText;
		
		private void Awake()
		{
			if (!File.Exists(Application.dataPath + "/settings.cfg"))
			{
				Debug.Log("No settings file found. Creating new one.");
				
				settings = new Settings();
				string jsonExport = JsonUtility.ToJson(settings);
				File.WriteAllText(Application.dataPath + "/settings.cfg",jsonExport);
			}
			else
			{
				Debug.Log("Settings file found. Loading Settings.");
				
				string jsonImport = File.ReadAllText(Application.dataPath + "/settings.cfg");
				settings = JsonUtility.FromJson<Settings>(jsonImport);
			}
		}

		public void StartGame()
		{
			VoxelData.seed = Mathf.Abs(seedField.text.GetHashCode() / VoxelData.WorldSizeInVoxels);
			SceneManager.LoadScene("Main", LoadSceneMode.Single);
		}

		public void EnterSettings()
		{
			viewDistSlider.value = settings.viewDistance;
			UpdateViewDistSlider();

			mouseSlider.value = settings.mouseSensitivity;
			MouseSlider();
			
			mainMenuObject.SetActive(false);
			settingsMenuObject.SetActive(true);
		}
		
		public void ExitSettings()
		{
			settings.viewDistance = (int) viewDistSlider.value;
			settings.mouseSensitivity = mouseSlider.value;
			
			string jsonExport = JsonUtility.ToJson(settings);
			File.WriteAllText(Application.dataPath + "/settings.cfg", jsonExport);

			mainMenuObject.SetActive(true);
			settingsMenuObject.SetActive(false);
		}

		public void QuitGame()
		{
			Application.Quit();
		}

		public void UpdateViewDistSlider()
		{
			viewDist.text = "View Distance: " + viewDistSlider.value;
		}

		public void MouseSlider()
		{
			mouseText.text = "Mouse Sensitivity: " + mouseSlider.value.ToString("F1");
		}
	}
}