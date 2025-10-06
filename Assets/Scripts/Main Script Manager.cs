using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class MainScriptManager : MonoBehaviour
{
    [Header("Big UI Canvases")]
    [SerializeField] GameObject MainUI; 
    [SerializeField] GameObject EmptyUI;

    [Header("Pop Up Canvas")]
    [SerializeField] GameObject PopUpCanvas;
    [SerializeField] GameObject InputFieldForNewEpisodeDirectory;
    [SerializeField] GameObject ErrorUI;
    [SerializeField] GameObject LoadingUI;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set Pop Up To Default
        PopUpCanvas.SetActive(false);
        InputFieldForNewEpisodeDirectory.SetActive(false);
        ErrorUI.SetActive(false);
        LoadingUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        RotateLoadingImage();
    }

    ////////////BIG UI Properties////////////////
    [Header("Big UI Properties")]
    [SerializeField] GameObject ListTemplate;
    [SerializeField] GameObject ListParent;
    [SerializeField] TextMeshProUGUI NumberOfFiles;
    [SerializeField] TMP_InputField EpisodeNaming;
    [SerializeField] TextMeshProUGUI CurrentDirectoryText;

    FileInfo[] FIleList;
    string CurrentDirectory;
    //StartFixingButton
    public void OnStartFixingButtonPress() {
        PopUpCanvas.SetActive(true);
        LoadingUI.SetActive(true);

        int counter = 1;
        foreach (FileInfo f in FIleList)
        {
            string newName = $"{EpisodeNaming.text} {counter}{f.Extension}";
            string newPath = Path.Combine(f.DirectoryName, newName);
            Debug.Log("File: " + newName);
            File.Move(f.FullName, newPath);
            counter++;
        }

        for (int a = 0; a < ListParent.transform.childCount; a++)
        {
            Destroy(ListParent.transform.GetChild(a).gameObject);
        }

        DirectoryInfo dir = new DirectoryInfo(CurrentDirectory);
        FileInfo[] info = dir.GetFiles("*.*").OrderBy(f => f.Name, new NaturalComparer()).ToArray();
        FIleList = info;
        foreach (FileInfo f in info)
        {
            Debug.Log("File: " + f.Name);
            GameObject CurrentItem = Instantiate(ListTemplate, ListParent.transform);
            CurrentItem.SetActive(true);
            CurrentItem.transform.Find("File Name").gameObject.GetComponent<TextMeshProUGUI>().text = f.Name;
            CurrentItem.transform.Find("File Directory").gameObject.GetComponent<TextMeshProUGUI>().text = f.Directory.FullName;
        }

        PopUpCanvas.SetActive(false);
        LoadingUI.SetActive(false);
    }

    ////////////Pop Up UI Buttons////////////////
    [Header("InputField UI Properties")]
    [SerializeField] TMP_InputField DirectoryInputField;

    [Header("Error UI Properties")]
    [SerializeField] TextMeshProUGUI ErrorText;

    [Header("Loading UI Properties")]
    [SerializeField] GameObject LoadingImage;

    //Loading Animation
    void RotateLoadingImage() {
        LoadingImage.transform.Rotate(0f, 0f, -200f * Time.deltaTime);
    }

    //Error UI Buttons
    public void OnErrorOkPress() {
        ErrorText.text = "";
        ErrorUI.SetActive(false);
        PopUpCanvas.SetActive(false);
    }

    //InputFieldUI Buttons
    public void OnInputFieldUIOkPress() {
        if (Directory.Exists(DirectoryInputField.text))
        {
            MainUI.SetActive(true);
            EmptyUI.SetActive(false);
            PopUpCanvas.SetActive(false);
            EpisodeNaming.text = "Episode";

            for (int a = 0; a < ListParent.transform.childCount; a++) {
                Destroy(ListParent.transform.GetChild(a).gameObject);
            }

            DirectoryInfo dir = new DirectoryInfo(DirectoryInputField.text);
            FileInfo[] info = dir.GetFiles("*.*").OrderBy(f => f.Name, new NaturalComparer()).ToArray();
            FIleList = info;
            NumberOfFiles.text = "Number Of Files: "+info.Length.ToString();
            CurrentDirectory = DirectoryInputField.text;
            CurrentDirectoryText.text = DirectoryInputField.text;
            foreach (FileInfo f in info)
            {
                Debug.Log("File: " + f.Name);
                GameObject CurrentItem=Instantiate(ListTemplate,ListParent.transform);
                CurrentItem.SetActive(true);
                CurrentItem.transform.Find("File Name").gameObject.GetComponent<TextMeshProUGUI>().text = f.Name;
                CurrentItem.transform.Find("File Directory").gameObject.GetComponent<TextMeshProUGUI>().text = f.Directory.FullName;
            }

            InputFieldForNewEpisodeDirectory.SetActive(false);
            DirectoryInputField.text = string.Empty;
        }
        else
        {
            Debug.LogError("Directory does not exist: " + DirectoryInputField.text);
            ErrorText.text = "Directory Does Not Exist";
            InputFieldForNewEpisodeDirectory.SetActive(false);
            ErrorUI.SetActive(true);
        }
    }

    public void OnInputFieldUICancelPress() {
        //Set Pop Up To Default
        PopUpCanvas.SetActive(false);
        InputFieldForNewEpisodeDirectory.SetActive(false);
        DirectoryInputField.text = string.Empty;
    }



    ///////////Side Panel Buttons/////////////////
    public void OnNewEpisodeButtonPress() {
        //Show Pop Up
        PopUpCanvas.SetActive(true);

        //Show InputField
        InputFieldForNewEpisodeDirectory.SetActive(true);
    }



    ////////////////////Tools//////////////////////////
    public class NaturalComparer : IComparer<string>
    {
        private static readonly Regex _re = new Regex(@"(\d+)", RegexOptions.Compiled);

        public int Compare(string x, string y)
        {
            var xParts = _re.Split(x);
            var yParts = _re.Split(y);

            for (int i = 0; i < Math.Min(xParts.Length, yParts.Length); i++)
            {
                if (int.TryParse(xParts[i], out int xNum) && int.TryParse(yParts[i], out int yNum))
                {
                    int cmp = xNum.CompareTo(yNum);
                    if (cmp != 0) return cmp;
                }
                else
                {
                    int cmp = string.Compare(xParts[i], yParts[i], StringComparison.OrdinalIgnoreCase);
                    if (cmp != 0) return cmp;
                }
            }
            return xParts.Length.CompareTo(yParts.Length);
        }
    }
}
