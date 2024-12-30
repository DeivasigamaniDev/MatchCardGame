using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;
    public AudioSource audiosource, bgaudiosource;
    public AudioClip[] audioclip;
    public static int gameSize = 2, Levelcount = 2;
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private GameObject cardList;
    [SerializeField]
    private Sprite cardBack;
    [SerializeField]
    private Sprite[] sprites;
    private CardController[] cards;
    int score = 0, hiscore = 0;
    public Text ScoreText, HighScoretText, LevelcountText;
    [SerializeField]
    private GameObject GamePlayArea, InGameUI;
    private int spriteSelected;
    private int cardSelected;
    private int cardLeft;
    private bool gameStart;
    public Sprite[] soundIcons;
    public Image soundButton;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if (PlayerPrefs.HasKey("HighScoretText"))
        {
            hiscore = PlayerPrefs.GetInt("HighScoretText", hiscore);
            HighScoretText.text = "HighScore: " + hiscore.ToString();
        }

        {
            Levelcount = PlayerPrefs.GetInt("LevelcountText", Levelcount);
            gameSize = Levelcount;
            LevelcountText.text = (Levelcount + "X" + Levelcount).ToString();
        }
        gameStart = false;
        GamePlayArea.SetActive(false);
        InGameUI.SetActive(false);
    }
    public void StartCardGame()
    {
        PlayAudiobyIndex(4);
        if (gameStart) return;
        gameStart = true;
        GamePlayArea.SetActive(true);
        InGameUI.SetActive(true);
        SetGamePanel();
        cardSelected = spriteSelected = -1;
        cardLeft = cards.Length;
        SpriteCardAllocation();
        StartCoroutine(HideFace());
    }
    private void SetGamePanel()
    {
        int isOdd = gameSize % 2;

        cards = new CardController[gameSize * gameSize - isOdd];
        foreach (Transform child in cardList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        RectTransform panelsize = GamePlayArea.transform.GetComponent(typeof(RectTransform)) as RectTransform;
        float row_size = panelsize.sizeDelta.x;
        float col_size = panelsize.sizeDelta.y;
        float scale = 1.0f / gameSize;
        float xInc = row_size / gameSize;
        float yInc = col_size / gameSize;
        float curX = -xInc * (float)(gameSize / 2);
        float curY = -yInc * (float)(gameSize / 2);

        if (isOdd == 0)
        {
            curX += xInc / 2;
            curY += yInc / 2;
        }
        float initialX = curX;
        for (int i = 0; i < gameSize; i++)
        {
            curX = initialX;
            for (int j = 0; j < gameSize; j++)
            {
                GameObject c;
                if (isOdd == 1 && i == (gameSize - 1) && j == (gameSize - 1))
                {
                    int index = gameSize / 2 * gameSize + gameSize / 2;
                    c = cards[index].gameObject;
                }
                else
                {
                    c = Instantiate(prefab);
                    c.transform.parent = cardList.transform;

                    int index = i * gameSize + j;
                    cards[index] = c.GetComponent<CardController>();
                    cards[index].ID = index;
                    c.transform.localScale = new Vector3(scale, scale);
                }
                c.transform.localPosition = new Vector3(curX, curY, 0);
                curX += xInc;

            }
            curY += yInc;
        }

    }
    IEnumerator HideFace()
    {
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < cards.Length; i++)
            cards[i].Flip();
        yield return new WaitForSeconds(0.5f);
    }
    private void SpriteCardAllocation()
    {
        int i, j;
        int[] selectedID = new int[cards.Length / 2];
        for (i = 0; i < cards.Length / 2; i++)
        {
            int value = Random.Range(0, sprites.Length - 1);
            for (j = i; j > 0; j--)
            {
                if (selectedID[j - 1] == value)
                    value = (value + 1) % sprites.Length;
            }
            selectedID[i] = value;
        }

        for (i = 0; i < cards.Length; i++)
        {
            cards[i].Active();
            cards[i].SpriteID = -1;
            cards[i].ResetRotation();
        }
        for (i = 0; i < cards.Length / 2; i++)
            for (j = 0; j < 2; j++)
            {
                int value = Random.Range(0, cards.Length - 1);
                while (cards[value].SpriteID != -1)
                    value = (value + 1) % cards.Length;

                cards[value].SpriteID = selectedID[i];
            }

    }
    public Sprite GetSprite(int spriteId)
    {
        return sprites[spriteId];
    }
    public Sprite CardBack()
    {
        return cardBack;
    }
    public bool canClick()
    {
        if (!gameStart)
            return false;
        return true;
    }
    public void cardClicked(int spriteId, int cardId)
    {
        if (spriteSelected == -1)
        {
            spriteSelected = spriteId;
            cardSelected = cardId;
            PlayAudiobyIndex(3);
        }
        else
        {
            if (spriteSelected == spriteId)
            {
                cards[cardSelected].Inactive();
                cards[cardId].Inactive();
                cardLeft -= 2;
                CheckGameWin();
                score += 5;
                ScoreText.text = "Score: " + score.ToString();
                if (score > hiscore)
                {
                    HighScoretText.text = "HighScore: " + score.ToString();
                    PlayerPrefs.SetInt("HighScoretText", score);
                }
                PlayAudiobyIndex(2);
            }
            else
            {
                cards[cardSelected].Flip();
                cards[cardId].Flip();
                PlayAudiobyIndex(1);
            }
            cardSelected = spriteSelected = -1;
        }
    }
    private void CheckGameWin()
    {
        if (cardLeft == 0)
        {
            Levelcount += 1;
            EndGame();
            gameSize = Levelcount;
            PlayAudiobyIndex(0);
        }
    }
    private void EndGame()
    {
        gameStart = false;
        GamePlayArea.SetActive(false);
        InGameUI.SetActive(false);
        LevelcountText.text = (Levelcount + "X" + Levelcount).ToString();
        PlayerPrefs.SetInt("LevelcountText", Levelcount);
    }
    public void Reset()
    {
        EndGame();
        PlayAudiobyIndex(4);
    }
    public void DisplayResult()
    {
        InGameUI.SetActive(true);
    }
    int i = 1;
    public void SoundController()
    {
        i++;
        if (i % 2 == 0)
        {
            soundButton.sprite = soundIcons[0];
            audiosource.volume = 0;
            bgaudiosource.volume = 0;
        }
        else
        {
            soundButton.sprite = soundIcons[1];
            audiosource.volume = 1;
            bgaudiosource.volume = 0.5f;
        }
    }
    void PlayAudiobyIndex(int index)
    {
        audiosource.clip = audioclip[index];
        audiosource.Play();
    }
    public void Help(int index)
    {
        audiosource.clip = audioclip[index];
        audiosource.Play();
    }
}
