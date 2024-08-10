using Auxilaryfunction.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Auxilaryfunction.Models
{
    public class StationTip : MonoBehaviour
    {
        public RectTransform RectTransform;
        public Transform[] Icons;
        public RectTransform[] IconRectTransforms;
        public Transform[] IconLocals;
        public Transform[] IconRemotes;
        public Transform[] countTexts;

        public Transform[] Icontexts;
        public Text[] CountText;
        public Text[] CountText2;

        public GameObject info_text;

        void Start()
        {
        }

        public void Init()
        {
            RectTransform = GetComponent<RectTransform>();
            Icons = new Transform[13];
            IconLocals = new Transform[13];
            IconRemotes = new Transform[13];
            countTexts = new Transform[13];
            Icontexts = new Transform[3];


            CountText = new Text[3];
            CountText2 = new Text[3];

            IconRectTransforms = new RectTransform[13];
            info_text = transform.Find("info-text").gameObject;
            for (int i = 0; i < 3; i++)
            {
                Icontexts[i] = transform.Find("icontext" + i);
                CountText[i] = Icontexts[i].Find("countText").GetComponent<Text>();
                if (i != 2)
                {
                    CountText2[i] = Icontexts[i].Find("countText2").GetComponent<Text>();
                }
            }
            for (int i = 0; i < 13; i++)
            {
                Icons[i] = transform.Find("icon" + i);
                IconLocals[i] = transform.Find("iconLocal" + i);
                IconRemotes[i] = transform.Find("iconremote" + i);
                countTexts[i] = transform.Find("countText" + i);
                IconRectTransforms[i] = Icons[i].GetComponent<RectTransform>();

                IconLocals[i].gameObject.SetActive(false);
                IconRemotes[i].gameObject.SetActive(false);
            }
            info_text.SetActive(false);
        }

        /// <summary>
        /// 设置物流站名称
        /// </summary>
        /// <param name="stationName"></param>
        /// <param name="index"></param>
        public void SetStationName(string stationName, int index)
        {
            var lasticon = Icons[index];
            var lastcountText = countTexts[index];
            lasticon.gameObject.SetActive(false);
            for (int i = index; i < 13; ++i)
            {
                IconLocals[i].gameObject.SetActive(false);
                IconRemotes[i].gameObject.SetActive(false);
                Icons[i].gameObject.SetActive(false);
                countTexts[i].gameObject.SetActive(false);
            }
            if (!string.IsNullOrEmpty(stationName))
            {
                var lastcountTextPosition = lastcountText.GetComponent<RectTransform>().anchoredPosition3D;
                lastcountText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(90, lastcountTextPosition.y, 0);
                lastcountText.GetComponent<Text>().fontSize = 18;
                lastcountText.GetComponent<Text>().text = stationName;
                lastcountText.GetComponent<Text>().color = Color.white;
                lastcountText.gameObject.SetActive(true);
            }
            else
            {
                lasticon.gameObject.SetActive(false);
                lastcountText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 设置对应格子物体
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemCount"></param>
        /// <param name="i"></param>
        /// <param name="localLogic"></param>
        /// <param name="remoteLogic"></param>
        /// <param name="ShowStationInfoMode"></param>
        /// <param name="IsStellarorCollector"></param>
        public void SetItem(int itemId, int itemCount, int i, ELogisticStorage localLogic, ELogisticStorage remoteLogic, bool ShowStationInfoMode, bool IsStellarorCollector)
        {
            var icon = Icons[i];
            var iconposition = IconRectTransforms[i].anchoredPosition3D;
            var iconLocal = IconLocals[i];
            var iconremote = IconRemotes[i];
            var iconlocalimage = iconLocal.GetComponent<Image>();
            var iconremoteimage = iconremote.GetComponent<Image>();
            var countText = countTexts[i];
            var countTextUitext = countText.GetComponent<Text>();
            if (itemId > 0)
            {
                switch (localLogic)
                {
                    case ELogisticStorage.Supply:
                        iconlocalimage.sprite = GUIDraw.rightsprite;
                        iconlocalimage.color = GUIDraw.BlueColor;
                        countTextUitext.color = GUIDraw.BlueColor;
                        break;
                    case ELogisticStorage.Demand:
                        iconlocalimage.sprite = GUIDraw.leftsprite;
                        iconlocalimage.color = GUIDraw.OrangeColor;
                        countTextUitext.color = GUIDraw.OrangeColor;
                        break;
                    case ELogisticStorage.None:
                        iconlocalimage.sprite = GUIDraw.flatsprite;
                        iconlocalimage.color = Color.gray;
                        countTextUitext.color = Color.gray;
                        break;
                }
                if (IsStellarorCollector)
                {
                    switch (remoteLogic)
                    {
                        case ELogisticStorage.Supply:
                            iconremoteimage.sprite = GUIDraw.rightsprite;
                            iconremoteimage.color = GUIDraw.BlueColor;
                            break;
                        case ELogisticStorage.Demand:
                            iconremoteimage.sprite = GUIDraw.leftsprite;
                            iconremoteimage.color = GUIDraw.OrangeColor;
                            break;
                        case ELogisticStorage.None:
                            iconremoteimage.sprite = GUIDraw.flatsprite;
                            iconremoteimage.color = Color.gray;
                            break;
                    }
                    iconremote.gameObject.SetActive(true);
                }
                if (ShowStationInfoMode)
                {
                    iconLocal.gameObject.SetActive(false);
                    if (IsStellarorCollector)
                    {
                        iconremote.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                        iconremote.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(100, iconposition.y, 0);
                        countText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, iconposition.y, 0);
                    }
                    else
                    {
                        iconremote.gameObject.SetActive(false);
                        countText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(90, iconposition.y, 0);
                    }
                }
                else
                {
                    iconLocal.gameObject.SetActive(true);
                    countText.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(70, iconposition.y, 0);
                    if (IsStellarorCollector)
                    {
                        iconLocal.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
                        iconremote.GetComponent<RectTransform>().sizeDelta = new Vector2(21, 21);
                        iconLocal.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(105, iconposition.y, 0);
                        iconremote.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(105, iconposition.y - 15, 0);
                    }
                    else
                    {
                        iconremote.gameObject.SetActive(false);
                        iconLocal.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(100, iconposition.y, 0);
                        iconLocal.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
                    }
                }
                icon.GetComponent<Image>().sprite = LDB.items.Select(itemId)?.iconSprite;
                icon.gameObject.SetActive(true);
                countTextUitext.text = itemCount.ToString("#,##0");
            }
            else
            {
                iconLocal.gameObject.SetActive(false);
                iconremote.gameObject.SetActive(false);
                icon.gameObject.SetActive(false);
                countTextUitext.color = Color.white;
                countTextUitext.text = "无";
                int anchoredX = 70;
                if (ShowStationInfoMode)
                {
                    if (IsStellarorCollector)
                    {
                        anchoredX = 70;
                    }
                    else
                    {
                        anchoredX = 90;
                    }
                }
                else
                {
                    anchoredX = 70;
                }
                countTextUitext.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(anchoredX, iconposition.y, 0);
            }
            countText.gameObject.SetActive(true);
        }

        /// <summary>
        /// 设置飞机飞船翘曲
        /// </summary>
        public void SetDroneShipWarp(int index, int itemId, int totalCount, int CurrentCount, int lastLine)
        {
            if (itemId == 0)
            {
                Icontexts[index].gameObject.SetActive(false);
                return;
            }
            if (index < 2)
            {
                CountText2[index].color = Color.white;
                CountText2[index].text = CurrentCount.ToString();
            }
            Icontexts[index].gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(index * 30, -30 - 30 * lastLine, 0);
            Icontexts[index].GetComponent<Image>().sprite = LDB.items.Select(itemId).iconSprite;
            CountText[index].color = Color.white;
            CountText[index].text = totalCount.ToString();
            Icontexts[index].gameObject.SetActive(true);
        }
    }
}
