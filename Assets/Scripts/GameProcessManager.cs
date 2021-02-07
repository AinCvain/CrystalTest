using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameProcessManager : MonoBehaviour
{
    [Header("Место, где будут располагаться кристаллы")]
    [SerializeField]
    private RectTransform gameFieldRect = null;

    [Header("UIManager")]
    [SerializeField]
    private UIManager uiManager = null;

    private int countPoints = 0;



    private static GameProcessManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (gameFieldRect == null)
        {
            throw new System.ArgumentException($"Не добавлено место, где будут располагаться кристаллы!");
        }
        if (uiManager == null)
        {
            throw new System.ArgumentException($"UIManager не был добавлен!");
        }
    }

    public static GameProcessManager GetInstance()
    {
        return instance;
    }

    private GameManager gameManager = null;


    // Состояния ячейки
    // null - пусто
    // -1 - Заблокирована
    // 0-(n-1) - кристал
    private CrystalController[,] gameFieldState = null;

    private void Start()
    {
        this.gameManager = GameManager.GetInstance();

        int numberOfCellsAside = GameManager.NUMBER_OF_CELLS_ASIDE;

        this.gameFieldState = new CrystalController[numberOfCellsAside, numberOfCellsAside];

        gameFieldRect.sizeDelta = new Vector2(gameManager.GetOneCellSideSize(), gameManager.GetOneCellSideSize());

        uiManager.UpdateCountPoints(countPoints);

        FillInGameField();
    }

    private void FillInGameField()
    {
        // Заполнение
        // Дырки
        int countLockCellNow = 0;

        int countCellsInGameField = GameManager.NUMBER_OF_CELLS_ASIDE * GameManager.NUMBER_OF_CELLS_ASIDE;

        for (int i = 0; i < countCellsInGameField; i++)
        {
            // Сделал так, для того что бы избежать бесконечного цикла который может заСтопить программу, хоть это и очень маловероятно.
            bool isLock = Random.Range(0, 14) == 1; // вероятность 1/14 = ~7.14%
            int posY = i / GameManager.NUMBER_OF_CELLS_ASIDE;
            int posX = i % GameManager.NUMBER_OF_CELLS_ASIDE;

            if (isLock)
            {
                gameFieldState[posY, posX] = CreateCrystal(posX, posY, -1);
                countLockCellNow++;
            }

            if (countLockCellNow == GameManager.LOCK_CELL_COUNT)
            {
                break;
            }
            else
            {
                int howNeedLockCell = GameManager.LOCK_CELL_COUNT - countLockCellNow;
                if (i == countCellsInGameField - howNeedLockCell)
                {
                    gameFieldState[posY, posX] = CreateCrystal(posX, posY, -1);
                    countLockCellNow++;
                }
            }
        }

        // # избежать бесконечного цикла
        /*for (int countLockCellNow = 0; countLockCellNow < GameManager.LOCK_CELL_COUNT;)
        {
            int lockCellPos = Random.Range(0, gameFieldState.Length);
            int posY = lockCellPos / numberOfCellsAside;
            int posX = lockCellPos % numberOfCellsAside;

            if (gameFieldState[posY, posX] == -1)
            {
                continue;
            }
            gameFieldState[posY, posX] = -1;
            countLockCellNow++;
        }*/

        // Кристалы
        for (int i = 0; i < countCellsInGameField; i++)
        {
            int posY = i / GameManager.NUMBER_OF_CELLS_ASIDE;
            int posX = i % GameManager.NUMBER_OF_CELLS_ASIDE;

            if (gameFieldState[posY, posX] != null && gameFieldState[posY, posX].GetCrystalType() == -1)
            {
                continue;
            }

            int currentCrystalType = Random.Range(0, gameManager.GetCrystalsTypesCount());

            bool thisTypeOfCrystalIsSuitable = true;

            if (posX - 1 >= 0 && gameFieldState[posY, posX - 1].GetCrystalType() == currentCrystalType)
            {
                if (posX - 2 >= 0 && gameFieldState[posY, posX - 2].GetCrystalType() == currentCrystalType)
                {
                    thisTypeOfCrystalIsSuitable = false;
                }
            }
            if (posY - 1 >= 0 && gameFieldState[posY - 1, posX].GetCrystalType() == currentCrystalType)
            {
                if (posY - 2 >= 0 && gameFieldState[posY - 2, posX].GetCrystalType() == currentCrystalType)
                {
                    thisTypeOfCrystalIsSuitable = false;
                }
            }

            if (thisTypeOfCrystalIsSuitable)
            {
                gameFieldState[posY, posX] = CreateCrystal(posX, posY, currentCrystalType);
            }
            else
            {
                // откатл переменную для того чтобы в следующей итерации (после инкремента) значение i было таким же как и в текущей итерации
                i--;
            }
        }
    }

    private CrystalController CreateCrystal(int posX, int posY, int type)
    {
        RectTransform thisRectTransform = GetComponent<RectTransform>();
        Vector2 rectGameMapSize = thisRectTransform.sizeDelta;

        Vector2 crystalSize = new Vector2(gameManager.GetOneCellSideSize(), gameManager.GetOneCellSideSize());

        GameObject currentCrystal = new GameObject("Crystal");
        RectTransform currentCrystalRect = currentCrystal.AddComponent<RectTransform>();
        currentCrystalRect.SetParent(gameFieldRect, false);
        currentCrystalRect.sizeDelta = crystalSize;
        currentCrystalRect.localPosition = gameManager.GetRealPositionFromMapPosition(posX, GameManager.NUMBER_OF_CELLS_ASIDE);

        CrystalController crystalController = currentCrystal.AddComponent<CrystalController>();
        crystalController.SetCrystalType(type);
        crystalController.SetCurrentPosInMap(posX, posY);
        return crystalController;
    }

    public delegate void OnSwitchInteractable(bool isInteractable);
    public OnSwitchInteractable onSwitchInteractable = null;

    private CrystalController crystal1 = null;
    private CrystalController crystal2 = null;

    private List<Vector2Int> allBurnPositions = null;

    public void OnClickToCrystal(CrystalController crystal)
    {
        if (crystal1 == null)
        {
            crystal1 = crystal;
            crystal1.SetSelectedMode(true);
            return;
        }

        if (crystal2 == null)
        {
            crystal2 = crystal;
            crystal2.SetSelectedMode(true);
            // Проверить стоят ли они рядом

            Vector2Int posOnMapCrystal1 = crystal1.GetCurrentPosInMap();
            Vector2Int posOnMapCrystal2 = crystal2.GetCurrentPosInMap();

            int deltaX = Mathf.Abs(posOnMapCrystal1.x - posOnMapCrystal2.x);
            int deltaY = Mathf.Abs(posOnMapCrystal1.y - posOnMapCrystal2.y);

            // если да то
            if (deltaX + deltaY == 1)
            {
                // Поменять их местами
                crystal1.SetSelectedMode(false);
                crystal2.SetSelectedMode(false);

                CrystalController buffer = gameFieldState[posOnMapCrystal1.y, posOnMapCrystal1.x];
                gameFieldState[posOnMapCrystal1.y, posOnMapCrystal1.x] = gameFieldState[posOnMapCrystal2.y, posOnMapCrystal2.x];
                gameFieldState[posOnMapCrystal2.y, posOnMapCrystal2.x] = buffer;

                crystal1.SetCurrentPosInMap(posOnMapCrystal2.x, posOnMapCrystal2.y);
                crystal2.SetCurrentPosInMap(posOnMapCrystal1.x, posOnMapCrystal1.y);

                allBurnPositions = new List<Vector2Int>();

                List<Vector2Int> bufferBurnPositions = GetPositionsOfCrystalsToBeBurned(crystal1);
                if (bufferBurnPositions.Count > 0)
                {
                    AddPoints(bufferBurnPositions);
                }
                allBurnPositions.AddRange(bufferBurnPositions);

                bufferBurnPositions = GetPositionsOfCrystalsToBeBurned(crystal2);
                if (bufferBurnPositions.Count > 0)
                {
                    AddPoints(bufferBurnPositions);
                }
                allBurnPositions.AddRange(bufferBurnPositions);

                if (allBurnPositions.Count == 0)
                {
                    buffer = gameFieldState[posOnMapCrystal1.y, posOnMapCrystal1.x];
                    gameFieldState[posOnMapCrystal1.y, posOnMapCrystal1.x] = gameFieldState[posOnMapCrystal2.y, posOnMapCrystal2.x];
                    gameFieldState[posOnMapCrystal2.y, posOnMapCrystal2.x] = buffer;

                    crystal1.SetCurrentPosInMap(posOnMapCrystal1.x, posOnMapCrystal1.y);
                    crystal2.SetCurrentPosInMap(posOnMapCrystal2.x, posOnMapCrystal2.y);
                }
                crystal1 = null;
                crystal2 = null;
            }
            else
            {
                crystal1.SetSelectedMode(false);
                crystal1 = crystal2;
                crystal2 = null;
            }
        }
    }

    private List<Vector2Int> GetPositionsOfCrystalsToBeBurned(CrystalController crystal)
    {
        Vector2Int crystalPos = crystal.GetCurrentPosInMap();
        int crystalType = crystal.GetCrystalType();

        List<Vector2Int> burnPositions = new List<Vector2Int>();
        burnPositions.Add(crystalPos);

        // Проверка по горизотнали
        List<Vector2Int> horizontalBurnCrystalsPos = new List<Vector2Int>();
        for (int x = crystalPos.x - 1; x >= 0; x--)
        {
            if (gameFieldState[crystalPos.y, x].GetCrystalType() == crystalType)
            {
                horizontalBurnCrystalsPos.Add(gameFieldState[crystalPos.y, x].GetCurrentPosInMap());
            }
            else
            {
                break;
            }
        }
        for (int x = crystalPos.x + 1; x < GameManager.NUMBER_OF_CELLS_ASIDE; x++)
        {
            if (gameFieldState[crystalPos.y, x].GetCrystalType() == crystalType)
            {
                horizontalBurnCrystalsPos.Add(gameFieldState[crystalPos.y, x].GetCurrentPosInMap());
            }
            else
            {
                break;
            }
        }
        if (horizontalBurnCrystalsPos.Count < 2)
        {
            horizontalBurnCrystalsPos.Clear();
        }

        // Проверка по вертикали 
        List<Vector2Int> verticalBurnCrystalsPos = new List<Vector2Int>();
        for (int y = crystalPos.y - 1; y >= 0; y--)
        {
            if (gameFieldState[y, crystalPos.x].GetCrystalType() == crystalType)
            {
                verticalBurnCrystalsPos.Add(gameFieldState[y, crystalPos.x].GetCurrentPosInMap());
            }
            else
            {
                break;
            }
        }
        for (int y = crystalPos.y + 1; y < GameManager.NUMBER_OF_CELLS_ASIDE; y++)
        {
            if (gameFieldState[y, crystalPos.x].GetCrystalType() == crystalType)
            {
                verticalBurnCrystalsPos.Add(gameFieldState[y, crystalPos.x].GetCurrentPosInMap());
            }
            else
            {
                break;
            }
        }
        if (verticalBurnCrystalsPos.Count < 2)
        {
            verticalBurnCrystalsPos.Clear();
        }

        // Объединение
        burnPositions.AddRange(horizontalBurnCrystalsPos);
        burnPositions.AddRange(verticalBurnCrystalsPos);

        if (burnPositions.Count <= 1)
        {
            burnPositions.Clear();
        }

        return burnPositions;
    }

    private void AddPoints(List<Vector2Int> burnPositions)
    {
        countPoints += 10 + (burnPositions.Count - 3) * 5; ;
        uiManager.UpdateCountPoints(countPoints);
    }

    private int countCrystalsMove = 0;
    public void OnCrystalMoveStart()
    {
        if (countCrystalsMove == 0)
        {
            onSwitchInteractable(false);
        }
        countCrystalsMove++;
    }

    public void OnCrystalMoveEnd()
    {
        countCrystalsMove--;
        if (countCrystalsMove == 0)
        {
            onSwitchInteractable(true);

            if (allBurnPositions == null)
            {
                allBurnPositions = new List<Vector2Int>();

                for (int y = 0; y < GameManager.NUMBER_OF_CELLS_ASIDE; y++)
                {
                    for (int x = 0; x < GameManager.NUMBER_OF_CELLS_ASIDE; x++)
                    {
                        if (gameFieldState[y, x] != null && gameFieldState[y, x].GetCrystalType() != -1 && !allBurnPositions.Contains(gameFieldState[y, x].GetCurrentPosInMap()))
                        {
                            List<Vector2Int> bufferBurnPositions = GetPositionsOfCrystalsToBeBurned(gameFieldState[y, x]);
                            if (bufferBurnPositions.Count > 0)
                            {
                                AddPoints(bufferBurnPositions);
                            }
                            allBurnPositions.AddRange(bufferBurnPositions);
                        }
                    }
                }
            }

            foreach (Vector2Int c2i in allBurnPositions)
            {
                if (gameFieldState[c2i.y, c2i.x] != null)
                {
                    Destroy(gameFieldState[c2i.y, c2i.x].gameObject);
                    gameFieldState[c2i.y, c2i.x] = null;
                }
            }
            MoveTheCrystalsUp();
            allBurnPositions = null;
            FillInEmptyCells();

            bool isPossibilityOfMakingASeries = CheckingForThePossibilityOfMakingASeries();

            if (!isPossibilityOfMakingASeries)
            {
                uiManager.ShowNotificationNoWayToComposeARow();
                StirCrystals();
            }
        }
    }

    private void StirCrystals()
    {
        List<int> freePositions = new List<int>();

        for (int i = 0; i < gameFieldState.Length; i++)
        {
            int posY = i / GameManager.NUMBER_OF_CELLS_ASIDE;
            int posX = i % GameManager.NUMBER_OF_CELLS_ASIDE;
            if (gameFieldState[posY, posX].GetCrystalType() != -1)
            {
                freePositions.Add(i);
            }
        }

        for (int i = 0; i < freePositions.Count; i++)
        {
            int posYFirstCrystal = freePositions[i] / GameManager.NUMBER_OF_CELLS_ASIDE;
            int posXFirstCrystal = freePositions[i] % GameManager.NUMBER_OF_CELLS_ASIDE;
            freePositions.Remove(freePositions[i]);

            int randomSecondCrystalPos = freePositions[Random.Range(0, freePositions.Count)];
            freePositions.Remove(randomSecondCrystalPos);
            int posYSecondCrystal = randomSecondCrystalPos / GameManager.NUMBER_OF_CELLS_ASIDE;
            int posXSecondCrystal = randomSecondCrystalPos % GameManager.NUMBER_OF_CELLS_ASIDE;
            gameFieldState[posYFirstCrystal, posXFirstCrystal].SetCurrentPosInMap(posXSecondCrystal, posYSecondCrystal);
            gameFieldState[posYSecondCrystal, posXSecondCrystal].SetCurrentPosInMap(posXFirstCrystal, posYFirstCrystal);

            CrystalController buffer = gameFieldState[posYFirstCrystal, posXFirstCrystal];
            gameFieldState[posYFirstCrystal, posXFirstCrystal] = gameFieldState[posYSecondCrystal, posXSecondCrystal];
            gameFieldState[posYSecondCrystal, posXSecondCrystal] = buffer;
        }
    }

    private int[,] checkedCodes = new int[16,3] {{ 2,  3,  1 }, 
                                                 { 12, 13, 11 }, 
                                                 { 8,  9,  7 }, 
                                                 { 7,  9,  8 }, 
                                                 { 7,  3,  8 }, 
                                                 { 7,  13, 8 }, 
                                                 { 2,  8,  7 },
                                                 { 12, 8,  7 },
                                                 { 12, 17, 7 },
                                                 { 10, 15, 5 },
                                                 { 16, 21, 11 },
                                                 { 11, 21, 16 },
                                                 { 12, 16, 11 },
                                                 { 10, 16, 11 },
                                                 { 11, 17, 16 },
                                                 { 11, 15, 16 },
    };

    private bool CheckingForThePossibilityOfMakingASeries()
    {
        for (int y = 0; y < GameManager.NUMBER_OF_CELLS_ASIDE; y++)
        {
            for (int x = 0; x < GameManager.NUMBER_OF_CELLS_ASIDE; x++)
            {
                if (gameFieldState[y, x] != null && gameFieldState[y, x].GetCrystalType() != -1)
                {
                    int currentType = gameFieldState[y, x].GetCrystalType();
                   
                    bool success = true;
                    for (int i = 0; i < 16; i++)
                    {
                        success = true;
                        for (int j = 0; j < 3; j++)
                        {
                            int iXPosOnMap = (x-1) + ((checkedCodes[i, j]) % 5);
                            int iYPosOnMap = (y-1) + ((checkedCodes[i, j]) / 5);
                            if (iYPosOnMap >= GameManager.NUMBER_OF_CELLS_ASIDE || iYPosOnMap < 0)
                            {
                                success = false;
                                break;
                            }
                            if (iXPosOnMap >= GameManager.NUMBER_OF_CELLS_ASIDE || iXPosOnMap < 0)
                            {
                                success = false;
                                break;
                            }
                            if (gameFieldState[iYPosOnMap, iXPosOnMap] == null)
                            {
                                success = false;
                                break;
                            }

                            if (j != 2)
                            {
                                if (gameFieldState[iYPosOnMap, iXPosOnMap].GetCrystalType() != currentType)
                                {
                                    success = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (gameFieldState[iYPosOnMap, iXPosOnMap].GetCrystalType() == -1)
                                {
                                    success = false;
                                }
                            }

                        }
                        if (success)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void MoveTheCrystalsUp()
    {
        for (int x = 0; x < GameManager.NUMBER_OF_CELLS_ASIDE; x++)
        {
            for (int y = 0; y < GameManager.NUMBER_OF_CELLS_ASIDE; y++)
            {
                if (gameFieldState[y, x] == null)
                {
                    for (int yi = y + 1; yi < GameManager.NUMBER_OF_CELLS_ASIDE; yi++)
                    {
                        if (gameFieldState[yi, x] != null && gameFieldState[yi, x].GetCrystalType() != -1)
                        {
                            gameFieldState[y, x] = gameFieldState[yi, x];
                            gameFieldState[yi, x] = null;
                            gameFieldState[y, x].SetCurrentPosInMap(x, y);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void FillInEmptyCells()
    {
        for (int x = 0; x < GameManager.NUMBER_OF_CELLS_ASIDE; x++)
        {
            for (int y = 0; y < GameManager.NUMBER_OF_CELLS_ASIDE; y++)
            {
                if (gameFieldState[y, x] == null)
                {
                    gameFieldState[y, x] = CreateCrystal(x, y, Random.Range(0, gameManager.GetCrystalsTypesCount()));
                }
            }
        }
    }
}
