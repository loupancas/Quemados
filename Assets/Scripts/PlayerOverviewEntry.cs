using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerOverviewEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;

    public void UpdateEntry(PlayerDataNetworked playerData)
    {
        playerNameText.text = playerData.NickName;
        scoreText.text = playerData.Score.ToString();
        livesText.text = playerData.Lives.ToString();
    }
}
