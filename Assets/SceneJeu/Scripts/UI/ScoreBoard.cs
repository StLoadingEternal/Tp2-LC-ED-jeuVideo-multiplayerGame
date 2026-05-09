using UnityEngine;
using TMPro;

public class ScoreBoard : MonoBehaviour
{
    public TextMeshProUGUI[] playerScoreTexts;

    void Update()
    {
        UpdateScoreBoard();
    }

    void UpdateScoreBoard()
    {
        CarHealth[] cars = FindObjectsByType<CarHealth>(FindObjectsSortMode.None);

        foreach (var text in playerScoreTexts)
            text.text = "";

        for (int i = 0; i < cars.Length && i < playerScoreTexts.Length; i++)
        {
            CarHealth car = cars[i];
            CarColor carColor = car.GetComponent<CarColor>();

            string playerName = "Joueur " + (car.OwnerClientId + 1);
            int score = car.GetPoints();

            playerScoreTexts[i].text = playerName + " : " + score;

            if (carColor != null)
                playerScoreTexts[i].color = carColor.GetColor();
        }
    }
}