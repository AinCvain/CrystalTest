using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Иконка заблокированной ячейки")]
    [SerializeField]
    private Sprite lockСellImage = null;

    [Header("Иконки кристалов")]
    [SerializeField]
    private List<Sprite> normalCrystalsImages = null;

    [Header("Размер стороны отображаемой карты")]
    [SerializeField]
    [Min(0)]
    private float gameFieldSize = 0;

    public const int CRYSTAL_MOVE_SPEED_PER_SECOND = 200;
    public const int NUMBER_OF_CELLS_ASIDE = 6;
    public const int LOCK_CELL_COUNT = 3;
    public const int MIN_COUNT_CRYSTAL_TYPES = 3;

    private float oneCellSideSize = 0;

    private static GameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (lockСellImage == null)
        {
            throw new System.ArgumentException("Не добавлена иконка заблокированной ячейки");
        }

        if (normalCrystalsImages.Count < MIN_COUNT_CRYSTAL_TYPES)
        {
            throw new System.ArgumentException($"Количество типов кристалов не может быть меньше {MIN_COUNT_CRYSTAL_TYPES}");
        }

        foreach (Sprite sprite in normalCrystalsImages)
        {
            if (sprite == null)
            {
                throw new System.ArgumentException($"Одно или несколько изображений типов кристалов не было добавлено!");
            }
        }

        oneCellSideSize = gameFieldSize / NUMBER_OF_CELLS_ASIDE;
    }

    public float GetOneCellSideSize()
    {
        return oneCellSideSize;
    }

    public Vector2 GetRealPositionFromMapPosition(int posX, int posY)
    {
        Vector2 startPos = new Vector2(-gameFieldSize / 2 + oneCellSideSize / 2, gameFieldSize / 2 - oneCellSideSize / 2);
        return new Vector2(startPos.x + posX * oneCellSideSize, startPos.y - posY * oneCellSideSize);
    }

    public static GameManager GetInstance()
    {
        return instance;
    }

    public int GetCrystalsTypesCount()
    {
        return normalCrystalsImages.Count;
    }

    public Sprite GetCrystalSprite(int index)
    {
        if (index == -1)
        {
            return lockСellImage;
        }

        return normalCrystalsImages[index];
    }
}
