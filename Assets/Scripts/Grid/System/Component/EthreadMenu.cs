using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EthreadMenu : MonoBehaviour {

    private GameObject redThread;
    private GameObject redThreadPrefab;
    private Button plus;
    private Button minus;
    private int quantity = 3;
    private Text quantityText;

    private AllyInfo selectedAlly;

    private class AllyInfo {
        public GameObject group;

        public Image image;
        public Image selectionBox;
        public Button selectionButton;

        public int maxCapacity = 5;
        public Transform threadCapacityGroup;
        public List<GameObject> capacitySlots = new List<GameObject>();
        public List<GameObject> capacity = new List<GameObject>();

        public AllyInfo(GameObject group) {
            this.group = group;
            image = group.transform.Find("Image").GetComponent<Image>();
            selectionBox = group.transform.Find("SelectionBox").GetComponent<Image>();
            selectionButton = group.transform.Find("SelectionButton").GetComponent<Button>();
            threadCapacityGroup = group.transform.Find("ThreadCapacity");
            for (var i = 0; i < maxCapacity; i++) {
                capacitySlots.Add(threadCapacityGroup.Find("Slot" + i.ToString()).gameObject);
            }
        }
    }

    private Dictionary<AllyInfo, GameObject> partyInfoDict = new Dictionary<AllyInfo, GameObject>();

    public void Awake () {
        redThread = this.transform.Find("RedThread").gameObject;
        plus = (Button) redThread.transform.Find("Plus").gameObject.GetComponent<Button>();
        minus = (Button) redThread.transform.Find("Minus").gameObject.GetComponent<Button>();
        quantityText = (Text) redThread.transform.Find("Quantity").gameObject.GetComponent<Text>();

        plus.onClick.AddListener(OnPlusClick);
        minus.onClick.AddListener(OnMinusClick);

        redThreadPrefab = Resources.Load<GameObject>("Prefabs/UI/RedThread");
    }

    public void PopulateParty(List<GameObject> partyPrefabs) {
        if (!partyPrefabs.All(prefab => partyInfoDict.Values.Contains(prefab))) {
            for (var i = 0; i < partyPrefabs.Count; i++) {
                var group = transform.Find("PartyMember" + i.ToString()).gameObject;
                var allyInfo = new AllyInfo(group);
                allyInfo.image.sprite = partyPrefabs[i].GetComponent<SpriteRenderer>().sprite;
                allyInfo.selectionButton.onClick.AddListener(delegate{OnSelectionClick(allyInfo);});
                allyInfo.capacitySlots.Each((slot, index) => {
                    slot.GetComponent<Button>().onClick.AddListener(delegate{OnSlotClick(allyInfo, index);});
                });
                partyInfoDict.Add(allyInfo, partyPrefabs[i]);
            }
            selectedAlly = partyInfoDict.Keys.First();
            selectedAlly.selectionBox.enabled = true;
        }
    }

    public void Update () {
        quantityText.text = "x" + quantity.ToString();
        partyInfoDict.Keys.ToList().ForEach(allyInfo => allyInfo.selectionBox.enabled = selectedAlly == allyInfo);
    }

    private void OnPlusClick () {
        if (selectedAlly.capacity.Count < selectedAlly.maxCapacity && quantity > 0) {
            quantity--;
            var newEthread = Instantiate(redThreadPrefab, new Vector2(0,0), Quaternion.identity);
            newEthread.transform.SetParent(selectedAlly.threadCapacityGroup);
            selectedAlly.capacity.Add(newEthread);
            newEthread.transform.position = selectedAlly.capacitySlots[selectedAlly.capacity.Count - 1].transform.position;
            partyInfoDict[selectedAlly].GetComponent<GridEntity>().equippedThreads.Add(newEthread.GetComponent<Ethread>());
        }

    }

    private void OnMinusClick () {
        if (selectedAlly.capacity.Count > 0) {
            quantity++;
            var ethreadToRemove = selectedAlly.capacity.Last();
            selectedAlly.capacity.Remove(ethreadToRemove);
            partyInfoDict[selectedAlly].GetComponent<GridEntity>().equippedThreads.Remove(ethreadToRemove.GetComponent<Ethread>());
            Destroy(ethreadToRemove);
        }
    }

    private void OnSlotClick (AllyInfo parent, int index) {
        selectedAlly = parent;
        if (index + 1 <= parent.capacity.Count) {
            quantity++;
            var ethreadToRemove = selectedAlly.capacity[index];
            selectedAlly.capacity.Remove(ethreadToRemove);

            selectedAlly.capacity.Each((ethread, i) => {
                ethread.transform.position = selectedAlly.capacitySlots[i].transform.position;
            });

            partyInfoDict[selectedAlly].GetComponent<GridEntity>().equippedThreads.Remove(ethreadToRemove.GetComponent<Ethread>());
            Destroy(ethreadToRemove);
        }
    }

    private void OnSelectionClick(AllyInfo parent) {
        selectedAlly = parent;
    }
}