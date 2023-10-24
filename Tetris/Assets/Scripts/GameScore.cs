using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameScore : MonoBehaviour
{
    TextMeshProUGUI scoreText;
    int score;

    [SerializeField] TextMeshProUGUI highscore;

    public int Score
    {
        get
        {
            return score;
        }
        set
        {
            score = value;
            UpdateScoreTextUI();
            UpdateHighscoreTextUI();
        }

    }

    private void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();

        highscore.text = PlayerPrefs.GetInt("Highscore", 0).ToString();
    }

    private void UpdateScoreTextUI()
    {
        string scoreStr = string.Format("{0:0000000}", score);
        scoreText.text = scoreStr;

    }

    public void UpdateHighscoreTextUI()
    {
        if (score > PlayerPrefs.GetInt("Highscore", 0))
        {
            PlayerPrefs.SetInt("Highscore", score);
            highscore.text = score.ToString();
        }
    }
}
