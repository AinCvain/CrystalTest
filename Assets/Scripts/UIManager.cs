using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI текст для отображения очков")]
    [SerializeField]
    private Text uiTextCountPoints = null;

    [Header("UI animator - уведомление о том, что кристаллы будут изменены")]
    [SerializeField]
    private Animator uiAnimatorNotificationNoWayToComposeARow = null;

    private void Awake()
    {
        if (uiTextCountPoints == null)
        {
            throw new System.ArgumentException($"Не добавлен UI текст для отображения очков!");
        }
        if (uiAnimatorNotificationNoWayToComposeARow == null)
        {
            throw new System.ArgumentException($"Не добавлен UI animator - уведомление о том, что кристаллы будут изменены");
        }
    }


    public void UpdateCountPoints(int nowCount)
    {
        uiTextCountPoints.text = nowCount.ToString();
    }

    public void ShowNotificationNoWayToComposeARow()
    {
        uiAnimatorNotificationNoWayToComposeARow.SetTrigger("ShowNotification");
    }
}
