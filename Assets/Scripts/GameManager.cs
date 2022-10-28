using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { set; get; }
    private PlayerController player;

    public bool isDead { set; get; }
    private bool isGameStarted = false;

    private const int COIN_SCORE_AMOUNT = 5;
    private int LAST_SCORE;

    // UI and the UI fileds
    public TextMeshProUGUI scoreText, coinText, modifierText;
    private float score, coinScore, modifierScore;

    private void Awake()
    {
        Instance = this;
        modifierScore = 1;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        modifierText.text = "Modifier Score: x" + modifierScore.ToString("0.0");
        coinText.text = coinScore.ToString("0");
        scoreText.text = scoreText.text = score.ToString("0");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (MobileInput.Instance.Tap && !isGameStarted)
        {
            isGameStarted = true;
            player.StartRunning();
        }

        if (isGameStarted && !isDead)
        {
            // Bump up the score
            score += (Time.deltaTime * modifierScore);
            if (LAST_SCORE != (int)score)
            {
                LAST_SCORE = (int)score;
                Debug.Log(LAST_SCORE);
                scoreText.text = "Score: " + score.ToString("0");

            } 
        }
    }

    public void GetCoin()
    {
        coinScore += COIN_SCORE_AMOUNT;
        scoreText.text = scoreText.text = score.ToString("0");
    }

    public void UpdateModifier(float modifierAmount)
    {
        modifierScore = 1.0f + modifierAmount;
        modifierText.text = "Modifier Score: " + modifierScore.ToString("0.0");
    }
}
