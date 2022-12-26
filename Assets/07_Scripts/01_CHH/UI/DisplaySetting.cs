using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySetting : MonoBehaviour
{
    #region Variable

    private FullScreenMode screenMode;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenBtn;
    private List<Resolution> resolutions = new List<Resolution>();
    private int resolutionNum;

    #endregion Variable

    #region Unity Method

    private void Start()
    {
        InitUI();
    }

    #endregion Unity Method

    #region Method

    private void InitUI()
    {
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if ((Screen.resolutions[i].refreshRate == 60))
            {
                resolutions.Add(Screen.resolutions[i]);
            }
        }

        resolutionDropdown.options.Clear();
        int optionNum = 0;

        foreach (Resolution resolution in resolutions)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = resolution.width + " x " + resolution.height + " " + resolution.refreshRate + "hz";
            resolutionDropdown.options.Add(option);

            if (resolution.width == Screen.width && resolution.height == Screen.height)
            {
                resolutionDropdown.value = optionNum;
            }
            optionNum++;
        }
        resolutionDropdown.RefreshShownValue();

        fullscreenBtn.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow) ? true : false;
    }

    public void DropboxOptionChange(int _resolutionNum)
    {
        resolutionNum = _resolutionNum;
    }

    public void FullScreenBtn(bool isFull)
    {
        screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    public void DisplaySettingBtnClick()
    {
        Screen.SetResolution(resolutions[resolutionNum].width, resolutions[resolutionNum].height, screenMode);
    }

    #endregion Method

}
