using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrystalController : MonoBehaviour, IPointerClickHandler
{
    private Image image = null;
    private RectTransform thisRectTransform = null;

    private int type = -1;

    private Vector2Int positionOnMap = Vector2Int.zero;

    private bool isLock = false;
    private bool isInteractable = false;

    private void Awake()
    {
        image = gameObject.AddComponent<Image>();
        thisRectTransform = GetComponent<RectTransform>();

        GameProcessManager.GetInstance().onSwitchInteractable += SetInteractable;
    }

    private void OnDestroy()
    {
        GameProcessManager.GetInstance().onSwitchInteractable -= SetInteractable;
    }

    public int GetCrystalType()
    {
        return type;
    }

    public void SetCrystalType(int type)
    {
        this.type = type;
        image.sprite = GameManager.GetInstance().GetCrystalSprite(this.type);
        if (this.type == -1)
        {
            isLock = true;
            image.color = Color.white;
        }
        else
        {
            isInteractable = true;
            image.color = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    public Vector2Int GetCurrentPosInMap()
    {
        return positionOnMap;
    }

    private List<Vector3> animationMoveTargetsStack;

    public void SetCurrentPosInMap(int toPosX, int toPosY)
    {
        GameProcessManager.GetInstance().OnCrystalMoveStart();

        positionOnMap = new Vector2Int(toPosX, toPosY);

        Vector3 targetPosition = GameManager.GetInstance().GetRealPositionFromMapPosition(toPosX, toPosY);

        if (animationMoveTargetsStack == null)
        {
            animationMoveTargetsStack = new List<Vector3>();
        }
        animationMoveTargetsStack.Add(targetPosition);

        if (!this.nowMove)
        {
            this.nowMove = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isLock && isInteractable)
        {
            image.color = Color.white;
            GameProcessManager.GetInstance().OnClickToCrystal(this);
        }
    }

    public void SetSelectedMode(bool isSelected)
    {
        if (type != -1)
        {
            isInteractable = !isSelected;
            image.color = (isSelected) ? new Color(1f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f);
        }
    }

    public void SetInteractable(bool isInteractable)
    {
        this.isInteractable = isInteractable;
    }

    private bool nowMove = false;
    
    private void Update()
    {
        if (nowMove)
        {
            if (Vector2.Distance(animationMoveTargetsStack[0], thisRectTransform.localPosition) > GameManager.GetInstance().GetOneCellSideSize() / 6)
            {
                Vector3 direction = animationMoveTargetsStack[0] - thisRectTransform.localPosition;
                thisRectTransform.localPosition += direction.normalized * GameManager.CRYSTAL_MOVE_SPEED_PER_SECOND * Time.deltaTime;
            }
            else
            {
                nowMove = false;
                thisRectTransform.localPosition = animationMoveTargetsStack[0];
                animationMoveTargetsStack.RemoveAt(0);

                if (animationMoveTargetsStack.Count > 0)
                {
                    nowMove = true;
                }

                GameProcessManager.GetInstance().OnCrystalMoveEnd();
            }
        }
    }
}