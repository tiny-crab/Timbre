using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EthreadMenu : MonoBehaviour {

    private AllyInfo selectedAlly;
    public List<ThreadButtonGroup> threadButtonGroups = new List<ThreadButtonGroup>();

    public class ThreadButtonGroup {
        public string prefabName;
        public GameObject threadPrefab;

        public Button plus;
        public Button minus;

        public int quantity = 3;
        public Text quantityText;

        public ThreadButtonGroup (Transform parentTransform, string prefabName) {
            this.prefabName = prefabName;
            var threadButtons = parentTransform.Find(prefabName).gameObject;
            plus = (Button) threadButtons.transform.Find("Plus").gameObject.GetComponent<Button>();
            minus = (Button) threadButtons.transform.Find("Minus").gameObject.GetComponent<Button>();
            quantityText = (Text) threadButtons.transform.Find("Quantity").gameObject.GetComponent<Text>();

            threadPrefab = Resources.Load<GameObject>("Prefabs/UI/ThreadIndicators/" + prefabName);
        }
    }

    private class AllyInfo {
        public GameObject group;

        public Image image;
        public Image selectionBox;
        public Button selectionButton;

        public int numSlots = 2;
        public int maxNumSlots = 5;
        public Transform threadCapacityGroup;
        public List<GameObject> capacitySlots = new List<GameObject>();
        public List<GameObject> capacity = new List<GameObject>();

        public AllyInfo(GameObject group) {
            this.group = group;
            image = group.transform.Find("Image").GetComponent<Image>();
            selectionBox = group.transform.Find("SelectionBox").GetComponent<Image>();
            selectionButton = group.transform.Find("SelectionButton").GetComponent<Button>();
            threadCapacityGroup = group.transform.Find("ThreadCapacity");
            for (var i = 0; i < maxNumSlots; i++) {
                capacitySlots.Add(threadCapacityGroup.Find("Slot" + i.ToString()).gameObject);
            }
            capacitySlots.Skip(numSlots).ToList().ForEach(slot => slot.SetActive(false));

            capacitySlots = capacitySlots.Take(numSlots).ToList();
        }
    }

    private Dictionary<AllyInfo, GameObject> partyInfoDict = new Dictionary<AllyInfo, GameObject>();

    public void Awake () {
        threadButtonGroups = new List<ThreadButtonGroup> {
            new ThreadButtonGroup(this.transform, "RedThread"),
            new ThreadButtonGroup(this.transform, "BlueThread"),
            new ThreadButtonGroup(this.transform, "GreenThread"),
            new ThreadButtonGroup(this.transform, "PurpleThread"),
            new ThreadButtonGroup(this.transform, "YellowThread"),
            new ThreadButtonGroup(this.transform, "PinkThread"),
        };

        threadButtonGroups.ForEach(group => {
            group.plus.onClick.AddListener(delegate{OnPlusClick(group);});
            group.minus.onClick.AddListener(delegate{OnMinusClick(group);});
        });
    }

    public void PopulateParty(List<GameObject> partyPrefabs) {
        if (!partyPrefabs.All(prefab => partyInfoDict.Values.Contains(prefab))) {
            for (var i = 0; i < partyPrefabs.Count; i++) {
                var group = transform.Find("PartyMember" + i.ToString()).gameObject;
                var allyInfo = new AllyInfo(group);
                allyInfo.image.sprite = partyPrefabs[i].GetComponent<SpriteRenderer>().sprite;
                allyInfo.selectionButton.onClick.AddListener(delegate{OnSelectionClick(allyInfo);});
                partyInfoDict.Add(allyInfo, partyPrefabs[i]);
            }
            selectedAlly = partyInfoDict.Keys.First();
            selectedAlly.selectionBox.enabled = true;
        }
    }

    public void Update () {
        threadButtonGroups.ForEach(group => group.quantityText.text = "x" + group.quantity.ToString());
        partyInfoDict.Keys.ToList().ForEach(allyInfo => allyInfo.selectionBox.enabled = selectedAlly == allyInfo);
    }

    private void OnPlusClick (ThreadButtonGroup group) {
        if (selectedAlly.capacity.Count < selectedAlly.numSlots && group.quantity > 0) {
            AddThreadToSlots(selectedAlly, group);
        }
    }

    private void OnMinusClick (ThreadButtonGroup group) {
        var ethreadToRemove = selectedAlly.capacity.Find(thread => group.prefabName == thread.GetComponent<Ethread>().effectName);
        if (selectedAlly.capacity.Count > 0 && ethreadToRemove != null) {
            var slotIndex = selectedAlly.capacity.IndexOf(ethreadToRemove);
            RemoveThreadFromSlot(slotIndex, selectedAlly, group);
        }
    }

    private void OnSlotClick (AllyInfo parent, int index, ThreadButtonGroup group) {
        selectedAlly = parent;
        if (index + 1 <= parent.capacity.Count) {
            RemoveThreadFromSlot(index, selectedAlly, group);
        }
    }

    private void OnSelectionClick(AllyInfo parent) {
        selectedAlly = parent;
    }

    private void AddThreadToSlots(AllyInfo parent, ThreadButtonGroup group) {
        var newEthread = Instantiate(group.threadPrefab, new Vector2(0,0), Quaternion.identity);
        newEthread.transform.SetParent(parent.threadCapacityGroup);
        parent.capacity.Add(newEthread);

        RefreshSlots(parent);

        partyInfoDict[parent].GetComponent<GridEntity>().equippedThreads.Add(newEthread.GetComponent<Ethread>());

        group.quantity--;
    }

    private void RemoveThreadFromSlot(int index, AllyInfo parent, ThreadButtonGroup group) {
        var ethreadToRemove = parent.capacity[index];
        parent.capacity.Remove(ethreadToRemove);

        RefreshSlots(parent);

        partyInfoDict[parent].GetComponent<GridEntity>().equippedThreads.Remove(ethreadToRemove.GetComponent<Ethread>());
        Destroy(ethreadToRemove);

        group.quantity++;
    }

    private void RefreshSlots(AllyInfo parent) {
        var slotButtons = parent.capacitySlots.Select(slot => slot.GetComponent<Button>()).ToList();
        slotButtons.ForEach(button => button.onClick.RemoveAllListeners());

        parent.capacity.Each((thread, i) => {
            var slot = parent.capacitySlots[i];
            thread.transform.position = slot.transform.position;
            var newThreadGroup = threadButtonGroups.Find(x => x.prefabName == thread.GetComponent<Ethread>().effectName);
            slotButtons[i].onClick.AddListener(delegate{OnSlotClick(parent, i, newThreadGroup);});
        });
    }
}