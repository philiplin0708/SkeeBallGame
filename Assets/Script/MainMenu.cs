using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainCanvas;
    public GameObject leaderboardCanvas;
    public Text[] scoresText;

    private void Start()
    {
        //when the app start, go to main menu
        ToMain();
    }
    
    public void ToMain()
    {
        //canvas switch
        mainCanvas.SetActive(true);
        leaderboardCanvas.SetActive(false);
    }
    public void PlayGame()
    {
        mainCanvas.SetActive(false);
        leaderboardCanvas.SetActive(false);
        //load the game
        SceneManager.LoadScene("Game");
    }
    public void LeaderBoard()
    {
        leaderboardCanvas.SetActive(true);
        mainCanvas.SetActive(false);
        for (int i = 0; i < LeaderBoardTable.ENTRYCOUNT; ++i)
        {
            //display the ranking with score
            var entry = LeaderBoardTable.GetEntry(i);
            scoresText[i].text = "No." + (i+1).ToString() + "  Score: " + entry.score;
        }
    }


}
