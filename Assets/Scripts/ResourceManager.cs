using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    #region Singleton

    public static ResourceManager instance;

    private void Awake()
    {
        if (instance)
        {
            Debug.LogWarning("Instance already exists!");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    #endregion

    public LayerMask dropPlacementObstacleMask;
  

    public Entity holePrefab;
    public Entity logsEntity;

    [Header("------ Inventory Items -----")]
    public InventoryItem moneyItem;

    //[Header("------ UI Prefabs -----")]
    //public WorldUIText worldUITextPrefab;
    //public EquipmentWheel equipmentWheelPrefab;
    //public EquipmentWheelItem equipmentWheelItemPrefab;
    //public ItemUI itemUIPrefab;
    //public QuestItemUI questItemUIPrefab;
}
