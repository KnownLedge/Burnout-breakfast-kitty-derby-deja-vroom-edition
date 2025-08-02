using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SettingsManager : MonoBehaviour
{
    public static float musicVolume = 1f; //Static variable (making sure it persists across scene reloads) for the music volume.
    public static float sfxVolume = 1f; //Static variable (making sure it persists across scene reloads) for the sfx volume.

    private AudioSource backgroundMusic; //The background music's audio source component.
    private float backgroundMusicBaseVolume; //The base volume for the above background music.

    private List<AudioSource> soundEffects; //Every sound effect in the scene's audio source component.
    private float[] soundEffectsBaseVolume; //The base volume for every sound effect in the above list.

    private bool userFullscreenPreference = true; //Whether the game should be fullscreen or not.
    private Resolution[] resolutions; //An array of all resolutions the player can use.
    private int userRefreshRate; //The player's refresh rate.
    private List<Resolution> filteredResolutions; //A list of the filtered resolutions to exclude resolutions of a different refresh rate to the user's.
    private int defaultResolutionIndex; //Identifies which resolution in the index is the player's default resolution.

    //Settings UI references.
    [SerializeField] private TMP_Dropdown resolutionDropdownMenu;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    void Start()
    {
        //Find and store the audio source components and base volume values for the background music and all of the sound effects in the scene.
        backgroundMusic = GameObject.FindGameObjectWithTag("Background Music").GetComponent<AudioSource>();
        backgroundMusicBaseVolume = backgroundMusic.volume;

        soundEffects = FindObjectsByType<AudioSource>(FindObjectsSortMode.None).ToList();
        soundEffects.Remove(backgroundMusic);
        soundEffectsBaseVolume = new float[soundEffects.Count];
        for (int i = 0; i < soundEffects.Count; i++)
        {
            soundEffectsBaseVolume[i] = soundEffects[i].volume;
        }

        //Update the volume of the background music and every sound effect on Start().
        backgroundMusic.volume = backgroundMusicBaseVolume * musicVolume;

        for (int i = 0; i < soundEffects.Count; i++)
        {
            soundEffects[i].volume = soundEffectsBaseVolume[i] * sfxVolume;
        }

        //Update the setting sliders, toggles etc on Start().
        musicVolumeSlider.value = musicVolume;
        sfxVolumeSlider.value = sfxVolume;
        fullScreenToggle.isOn = Screen.fullScreen;

        resolutions = Screen.resolutions; //Fill the array with all the resolutions the player can use.
        userRefreshRate = Screen.currentResolution.refreshRate; //Get the player's current refresh rate.
        filteredResolutions = new List<Resolution>();
        resolutionDropdownMenu.ClearOptions(); //Clear the default options out of the dropdown menu.


        for (int loop = 0; loop < resolutions.Length; loop++) //Loop through the array of resolutions.
        {
            if (resolutions[loop].refreshRate == userRefreshRate) //If the given resolution's refresh rate matches the player's current refresh rate,
            {
                filteredResolutions.Add(resolutions[loop]); //Then add this resolution to the filtered list of resolutions.
            }
        }

        List<string> dropdownOptions = new List<string>(); //Declare a string list which will contain the options for the dropdown menu.
        for (int loop = 0; loop < filteredResolutions.Count; loop++)
        {
            string dropdownChoice = filteredResolutions[loop].width + " x " + filteredResolutions[loop].height; //Put the resolutions into string format for the dropdown list.
            dropdownOptions.Add(dropdownChoice); //Add it to the new list.
            if (filteredResolutions[loop].width == Screen.width && filteredResolutions[loop].height == Screen.height) //If the resolution is the same as the user's current resolution, 
            {
                defaultResolutionIndex = loop; //Then we make that the selected default in the dropdown menu.
            }
        }

        resolutionDropdownMenu.AddOptions(dropdownOptions); //Add all the options to the dropdown menu.
        resolutionDropdownMenu.value = defaultResolutionIndex; //Set the default option.
        resolutionDropdownMenu.RefreshShownValue(); //Refresh the dropdown menu.
    }

    public void ChangeResolution(int userResInput)
    {
        Resolution theTargetResolution = filteredResolutions[userResInput]; //Using the user input, select the corresponding resolution from the filtered list made in Start().
        Screen.SetResolution(theTargetResolution.width, theTargetResolution.height, userFullscreenPreference); //Set the actual resolution.
    }

    public void MakeFullscreen(bool userFullscreenInput)
    {
        userFullscreenPreference = userFullscreenInput; //Read the value into a variable which can also be used above. ^
        Screen.fullScreen = userFullscreenPreference; //Set the actual fullscreen preference.
    }

    public void BackgroundMusicVolumeSliderChanged()
    {
        musicVolume = musicVolumeSlider.value; //Read the music volume slider's value to the static music volume variable.
        backgroundMusic.volume = backgroundMusicBaseVolume * musicVolume; //Update the background music's volume.
    }

    public void SoundEffectVolumeSliderChanged()
    {
        sfxVolume = sfxVolumeSlider.value; //Read the sfx volume slider's value to the static sfx volume variable.
        for (int i = 0; i < soundEffects.Count; i++)
        {
            soundEffects[i].volume = soundEffectsBaseVolume[i] * sfxVolume; //Update all the sound effect's volumes.
        }
    }
}
