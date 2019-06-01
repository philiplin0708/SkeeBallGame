using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public enum GameStatus
    {
        CountDown,
        Play,
        GameOver
    };

    enum BarMovement
    {
        Idle,
        ToRight,
        ToLeft
    };

    //Game variable 
    public Camera mainCam;
    public Vector3 throwForce;
    public GameObject ballPrefab;
    private Vector3 ballPos = new Vector3(1.5f, 1.49f, -16.8f); // init ball position
    private bool ballOnStage = false;                           // is ball appear
    public int totalScore;                              //final score 
    public int timer;                                   // 30sec
    public GameStatus gameStatus;
    private GameObject currentBall;

    //Powerbar
    private float barFillAmount;
    public float barMoveSpeed = 1f;
    public Image barBG;
    public Image powerBar;
    private BarMovement barMovement;

    //UI element 
    public Text ScoreText; 
    public Text CountDownText;
    public Image CountDownImage;
    public Image PauseBG; // Pause bg to use in first countdown
    public Text TimerText;
    public Text ResultGameOver;
    public GameObject EndGameButton;


    //Pool setup
    [System.Serializable]
    public struct Pool
    {
        public string tag;        //tag name
        public GameObject prefab; //here is to hook up the ball prefab
        public int size;          // how many is going to put into the pool
    };

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    //Singleton 
    #region Singleton
    public static Game Instance;

    private void Awake()
    {
        Instance = this;
    }
    
    #endregion

    void Start()
    {
        gameStatus = GameStatus.CountDown;
        PauseBG.enabled = false;
        
        ResultGameOver.text = "";
        EndGameButton.SetActive(false);

        HidePowerBar();
        TimerText.text = timer.ToString();
        ClearScore();
        StartCoroutine(CountDown());

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        foreach(Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for(int i=0; i<pool.size;i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    void ClearScore()
    {
        totalScore = 0;
        ScoreText.text = totalScore.ToString();
    }

    IEnumerator CountDown()
    {
        CountDownText.enabled = true;
        PauseBG.enabled = true;
        {
            for (int i = 4; i > 0; i--)
            {
                CountDownText.text = (i - 1).ToString();
                if (i == 1)
                {
                    CountDownText.text = "GO!!";
                }
                yield return new WaitForSecondsRealtime(1.0f);
            }
        }
        PauseBG.enabled = false;
        CountDownText.enabled = false;

        gameStatus = GameStatus.Play;
        StartCoroutine(TimerCount());
    }

    IEnumerator TimerCount()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        timer--;

        if (timer <= 0)
        {
            timer = 0;
            GameEnd();
        }
        else
            StartCoroutine(TimerCount());

        TimerText.text = timer.ToString();
    }

    public void UpdateScore(int score)
    {
        ballOnStage = false;
        totalScore += score;
        ScoreText.text = totalScore.ToString();
    }

    void GameEnd()
    {
        PauseBG.enabled = true;
        gameStatus = GameStatus.GameOver;

        EndGameButton.SetActive(true);

        if (LeaderBoardTable.HighestScore(totalScore))
        {
            ResultGameOver.text = "You Score: " + totalScore + " Amazing! You got the highest score!";
            EndGameButton.transform.GetChild(0).GetComponent<Text>().text = "Go back";
            EndGameButton.GetComponent<Button>().onClick.AddListener(() => LoadMain());
        }
        else if (LeaderBoardTable.LowestScore(totalScore))
        {
            ResultGameOver.text = "You Score: " + totalScore + ", Your score is low. Try again";
            EndGameButton.transform.GetChild(0).GetComponent<Text>().text = "Try again";
            EndGameButton.GetComponent<Button>().onClick.AddListener(() => ResetGame());
        }
        else
        {
            ResultGameOver.text = "Try Again.";
            EndGameButton.transform.GetChild(0).GetComponent<Text>().text = "Try again";
            EndGameButton.GetComponent<Button>().onClick.AddListener(() => ResetGame());
        }

        LeaderBoardTable.Record(totalScore);
    }

    void ResetGame()
    {
        SceneManager.LoadScene("Game");
    }
    void LoadMain()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void HidePowerBar()
    {
        barBG.enabled = false;
        powerBar.enabled = false;
    }
    void ShowPowerBar()
    {
        barMovement = BarMovement.ToRight;
        barFillAmount = 0;
        powerBar.GetComponent<Image>().fillAmount = barFillAmount;
        barBG.enabled = true;
        powerBar.enabled = true;
    }
    void PrebareShoot(Vector2 pos)
    {
        if (!ballOnStage)
        {
            ShowPowerBar();
        }
    }
    public GameObject SpawnFromPool(string tag, Vector3 pos, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(tag))
            return null;
        //spawn the object from the pool by the name tag, position and rotation
        GameObject objToSpawn = poolDictionary[tag].Dequeue();
        objToSpawn.SetActive(true);
        objToSpawn.transform.position = pos;
        objToSpawn.transform.rotation = rot;

        poolDictionary[tag].Enqueue(objToSpawn);
        return objToSpawn;
    }
    void ThrowingBall(Vector2 pos)
    {      
        if (!ballOnStage)
        {
            Vector3 realWorldPos = mainCam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 0));

            //giving a direction base on the touch position and apply the direction to the force
            float touchpoint = (pos.x / Screen.width) - 0.5f;
            
            float forceZ = throwForce.z + (barFillAmount * 6);
            float forceY = throwForce.y + (barFillAmount * 6);
            float forceX = throwForce.x + (touchpoint * 10);
            
            //there are few different type of ball, when we touch the screen > it will randomly spawn different type of ball by the tag
            GameObject ball = SpawnFromPool("Ball" + Random.Range(1,4).ToString(), ballPos, Quaternion.identity) as GameObject;
            Vector3 dir = (realWorldPos - ballPos).normalized * 2.5f + new Vector3(forceX, forceY, forceZ);
            ball.GetComponent<Rigidbody>().velocity = dir;

            //Giving random drag so they fall with gravity randomly
            ball.GetComponent<Rigidbody>().drag = Random.Range(0, 1); 

            currentBall = ball;
            ballOnStage = true;
            HidePowerBar();
        }
    }

    void FixedUpdate()
    {
        if (gameStatus == GameStatus.Play)
        {
            if (ballOnStage)
                CheckBallDistance();
        }
    }

    void Update()
    {   
        // touch setup 
        if (gameStatus == GameStatus.Play)
        {
            UpdateBar();

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    PrebareShoot(touch.position);
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    ThrowingBall(touch.position);
                }
            }
        }
    }

    void UpdateBar()
    {
        if (!powerBar.enabled)
            return;

        switch (barMovement)
        {
            case BarMovement.ToLeft:
                barFillAmount -= (barMoveSpeed * Time.fixedDeltaTime);
                break;

            case BarMovement.ToRight:
                barFillAmount += (barMoveSpeed * Time.fixedDeltaTime) - (barFillAmount * barMoveSpeed / 50);
                break;
        }

        if (barFillAmount >= .9f && barMovement == BarMovement.ToRight)
        {
            barMovement = BarMovement.ToLeft;
        }
        else
            if (barFillAmount <= .1 && barMovement == BarMovement.ToLeft)
            barMovement = BarMovement.ToRight;

        powerBar.GetComponent<Image>().fillAmount = barFillAmount;
    }

    void CheckBallDistance()
    {
        if (currentBall == null)
            return;

        Vector3 headDir = new Vector3(currentBall.transform.position.x - mainCam.transform.position.x, currentBall.transform.position.y - mainCam.transform.position.y, currentBall.transform.position.z - mainCam.transform.position.z);
        //if the distance is too far, then player can throw ball again
        float distance = Vector3.Dot(headDir, mainCam.transform.forward);
        if (distance > 4f)
        {
            ballOnStage = false;
        }
    }
}