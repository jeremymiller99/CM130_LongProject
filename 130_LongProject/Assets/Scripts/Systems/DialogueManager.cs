using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    private static DialogueManager instance;

    void Awake()
    {
        instance = this;
        dialoguePanel.SetActive(false);
    }

    public static void Show(string message)
    {
        if (instance != null)
        {
            instance.dialogueText.text = message;
            instance.dialoguePanel.SetActive(true);
        }
    }

    public static void Hide()
    {
        if (instance != null)
        {
            instance.dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        // Hide dialogue on player input (Enter or MouseClick)
        if (dialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            Hide();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Hide();
        }
    }
}
