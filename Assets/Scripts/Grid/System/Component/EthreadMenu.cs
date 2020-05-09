using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EthreadMenu : MonoBehaviour {

    private AllyInfo selectedAlly;
    public List<ThreadButtonGroup> threadButtonGroups = new List<ThreadButtonGroup>();
    private SelectedAllyDisplay selectedAllyDisplay;

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

        public GridEntity entity;

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

    private class SelectedAllyDisplay {

        private Transform title;
        private Text nameText;
        private Text subnameText;

        private Image damageIcon;
        private List<Image> damageChips;
        private Image moveRangeIcon;
        private List<Image> moveRangeChips;
        private Image atkRangeIcon;
        private List<Image> atkRangeChips;

        private List<Transform> skillElements;
        private List<Text> skillNameTexts;
        private List<Text> skillDescTexts;

        public SelectedAllyDisplay(GameObject ethreadMenu) {
            title = ethreadMenu.transform.Find("Title");
            nameText = (Text) title.transform.Find("Name").GetComponent<Text>();
            subnameText = (Text) title.transform.Find("Subname").GetComponent<Text>();
            damageIcon = (Image) title.transform.Find("Damage").GetComponent<Image>();
            damageChips = GetChipsForSkillMenu(damageIcon.transform);
            moveRangeIcon = (Image) title.transform.Find("MoveRange").GetComponent<Image>();
            moveRangeChips = GetChipsForSkillMenu(moveRangeIcon.transform);
            atkRangeIcon = (Image) title.transform.Find("AtkRange").GetComponent<Image>();
            atkRangeChips = GetChipsForSkillMenu(atkRangeIcon.transform);

            skillElements = new List<Transform> {
                ethreadMenu.transform.Find("Skill1"),
                ethreadMenu.transform.Find("Skill2"),
                ethreadMenu.transform.Find("Skill3")
            };
        }

        public void UpdateDisplay(AllyInfo selectedAlly) {
            var threadDamageBonus = selectedAlly.capacity.Where(thread => thread.GetComponent<Ethread>().effectName == "RedThread").Count();
            var threadMoveBonus = selectedAlly.capacity.Where(thread => thread.GetComponent<Ethread>().effectName == "BlueThread").Count();

            nameText.text = selectedAlly.entity.entityName;
            subnameText.text = selectedAlly.entity.entitySubname;
            SetChips(damageChips, selectedAlly.entity.damage + threadDamageBonus);
            SetChips(moveRangeChips, selectedAlly.entity.maxMoves + threadMoveBonus);
            SetChips(atkRangeChips, selectedAlly.entity.range);

            var skills = SkillUtils.PopulateSkills(selectedAlly.entity.skillNames, new List<int> {1, 1, 1});

            skillNameTexts = skillElements.Select(element => {
                return element.Find("SkillName").GetComponent<Text>();
            }).ToList();

            skillDescTexts = skillElements.Select(element => {
                return element.Find("SkillDesc").GetComponent<Text>();
            }).ToList();

            skills.ForEach(skill => {
                var skillNameText = skillNameTexts[0];
                skillNameTexts.Remove(skillNameText);

                skillNameText.text = skill.name ?? "";

                var skillDescText = skillDescTexts[0];
                skillDescTexts.Remove(skillDescText);

                skillDescText.text = skill.desc ?? "";
            });

            var enablePrimarySkillText = selectedAlly.capacity.Any(thread => thread.GetComponent<Ethread>().effectName == "PurpleThread");
            var enableSecondarySkillText = selectedAlly.capacity.Any(thread => thread.GetComponent<Ethread>().effectName == "YellowThread");
            var enableTertiarySkillText = selectedAlly.capacity.Any(thread => thread.GetComponent<Ethread>().effectName == "PinkThread");

            ToggleSkillText(skillElements[0], enablePrimarySkillText);
            ToggleSkillText(skillElements[1], enableSecondarySkillText);
            ToggleSkillText(skillElements[2], enableTertiarySkillText);

            // resets any skill section that was previously populated, that the newly selected ally does not have
            // i.e swapping from ally with 3 skills to one with 2 will empty the third slot even though
            skillNameTexts.ForEach(text => text.text = "");
            skillDescTexts.ForEach(text => text.text = "");
        }

        private void ToggleSkillText(Transform skillElement, bool enabled) {
            var skillNameText = skillElement.Find("SkillName").GetComponent<Text>();
            var skillDescText = skillElement.Find("SkillDesc").GetComponent<Text>();
            var threadImage = skillElement.Find("ThreadImage").GetComponent<Image>();

            if (enabled) {
                threadImage.color = new Color(threadImage.color.r, threadImage.color.g, threadImage.color.b, 1f);
                skillNameText.color = Color.white;
                skillDescText.color = Color.white;
            } else {
                threadImage.color = new Color(threadImage.color.r, threadImage.color.g, threadImage.color.b, 0.7f);
                skillNameText.color = Color.grey;
                skillDescText.color = Color.grey;
            }
        }

        private List<Image> GetChipsForSkillMenu(Transform transform) {
            return Enumerable.Range(0,8).Select(index => {
                return (Image) transform.Find(
                    String.Format("Chip{0}", index)
                ).GetComponent<Image>();
            }).ToList();
        }

        private void SetChips(IEnumerable<Image> chips, int number) {
            chips.Take(number).ToList().ForEach(chip => chip.gameObject.SetActive(true));
            chips.Skip(number).ToList().ForEach(chip => chip.gameObject.SetActive(false));
        }
    }

    private Dictionary<AllyInfo, GameObject> partyInfoDict = new Dictionary<AllyInfo, GameObject>();

    public void Awake () {
        threadButtonGroups = new List<ThreadButtonGroup> {
            new ThreadButtonGroup(this.transform, "RedThread"),
            new ThreadButtonGroup(this.transform, "BlueThread"),
            // new ThreadButtonGroup(this.transform, "GreenThread"),
            new ThreadButtonGroup(this.transform, "PurpleThread"),
            new ThreadButtonGroup(this.transform, "YellowThread"),
            new ThreadButtonGroup(this.transform, "PinkThread"),
        };

        threadButtonGroups.ForEach(group => {
            group.plus.onClick.AddListener(delegate{OnPlusClick(group);});
            group.minus.onClick.AddListener(delegate{OnMinusClick(group);});
        });

        selectedAllyDisplay = new SelectedAllyDisplay(this.gameObject);
    }

    public void PopulateParty(List<GameObject> partyPrefabs) {
        if (!partyPrefabs.All(prefab => partyInfoDict.Values.Contains(prefab))) {
            for (var i = 0; i < partyPrefabs.Count; i++) {
                var group = transform.Find("PartyMember" + i.ToString()).gameObject;
                var allyInfo = new AllyInfo(group);
                allyInfo.image.sprite = partyPrefabs[i].GetComponent<SpriteRenderer>().sprite;
                allyInfo.entity = partyPrefabs[i].GetComponent<GridEntity>();
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
        selectedAllyDisplay.UpdateDisplay(selectedAlly);
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