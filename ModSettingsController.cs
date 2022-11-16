using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TrombSettings
{
    class TrombButton : MonoBehaviour
    {
        public Button Button;
        public RectTransform RectTransform;
        public Text Text;
        public string CategoryID;

        public void Move(Vector2 change)
        {
            RectTransform.anchoredPosition = new Vector2(change.x + RectTransform.anchoredPosition.x, change.y + RectTransform.anchoredPosition.y);
        }
    }

    class TrombCheckbox : MonoBehaviour
    {
        public Toggle Toggle;
        public Text Label;
        public RectTransform RectTransform;
    }

    public class TrombDropDown
    {
        public List<string> Options = new List<string>();
    }

    public class ModSettingsController : MonoBehaviour
    {
        public HomeController controller;
        public GameObject SettingsPanel;

        private static TrombButton buttonPrefab;
        private static TrombCheckbox checkboxPrefab;
        private static Dropdown dropdownPrefab;
        private static Text textPrefab;
        private static Slider sliderPrefab;

        private bool hasInitted;

        private int categoryCount = 0;

        private Dictionary<string, GameObject> panelDict = new Dictionary<string, GameObject>();

        private int yOffset = 0;

        public void Init()
        {
            if (!hasInitted)
            {
                SettingsPanel = transform.Find("Settings").gameObject;

                GetButtonPrefab();
                GetCheckboxPrefab();
                GetDropdownPrefab();
                GetTextPrefab();
                GetSliderPrefab();

                hasInitted = true;
            }

            foreach (var t in TrombConfig.TrombSettings)
            {
                yOffset = 0;
                AddCategory(t.Key);
                if (t.Key != "Settings")
                {
                    foreach (BaseConfig config in t.Value)
                    {
                        if (config.GetType() == typeof(BaseConfig))
                        {
                            switch (config.entry.BoxedValue)
                            {
                                case bool boolValue:
                                {
                                    AddCheckbox(config.entry, t.Key, config.entry.Definition.Key);

                                    break;
                                }
                                case Enum enumValue:
                                {
                                    AddDropdown(config.entry, t.Key, config.entry.Definition.Key);

                                    break;
                                }
                                case null:
                                    break;
                            }
                        }
                        else
                        {
                            if (config is StepSliderConfig sliderConfig)
                            {
                                AddSlider(sliderConfig, t.Key, sliderConfig.entry.Definition.Key);
                            }
                        }

                    }

                    panelDict[t.Key].SetActive(false);
                }
            }
        }


        public static Text CreateText(Transform canvasTransform, Vector2 position, float width, float height, string text, int fontSize = 28, Color? textColor = null, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            Text textLabel = UnityEngine.Object.Instantiate(textPrefab, canvasTransform);
            textLabel.gameObject.SetActive(true);

            textLabel.text = text;
            textLabel.fontSize = textLabel.fontSize;
            textLabel.color = textColor ?? Color.gray;
            RectTransform t = textLabel.GetComponent<RectTransform>();

            t.anchoredPosition = position;
            t.sizeDelta = new Vector2(width, height);

            textLabel.resizeTextForBestFit = true;

            textLabel.alignment = alignment;

            return textLabel;
        }

        public static Dropdown CreateDropdown(Transform canvasTransform, Vector2 position, float width, float height, List<string> options, int value, Action<int> onValueChanged) 
        {
            Dropdown dropdown = UnityEngine.Object.Instantiate(dropdownPrefab, canvasTransform);
            dropdown.gameObject.SetActive(true);

            RectTransform t = dropdown.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(width, height);

            t.anchoredPosition = position;

            dropdown.AddOptions(options);
            dropdown.value = value;

            dropdown.onValueChanged.AddListener((int index) =>
            {
                onValueChanged.Invoke(index);
            });

            return dropdown;
        }

        public static Toggle CreateToggle(Transform canvasTransform, Vector2 position, float width, float height, string label, bool value, Action<bool> onValueChanged)
        {
            TrombCheckbox checkbox = UnityEngine.Object.Instantiate(checkboxPrefab, canvasTransform);
            checkbox.gameObject.SetActive(true);
            checkbox.Label.text = label;

            RectTransform t = checkbox.RectTransform;
            t.sizeDelta = new Vector2(width, height);

            t.anchoredPosition = position + new Vector2(width, 0);

            checkbox.Toggle.isOn = value;

            checkbox.Toggle.onValueChanged.AddListener((bool _checked) =>
            {
                onValueChanged.Invoke(_checked);
            });

            return checkbox.Toggle;
        }

        public static Button CreateButton(Transform canvasTransform, Vector2 position, float width, float height, string label, Action onClick)
        {
            TrombButton categoryButton = UnityEngine.Object.Instantiate(buttonPrefab, canvasTransform);
            categoryButton.gameObject.SetActive(true);
            categoryButton.Text.text = label;

            RectTransform t = categoryButton.RectTransform;
            t.sizeDelta = new Vector2(width, height);
            t.anchoredPosition = position;

            categoryButton.Button.onClick.AddListener(() =>
            {
                onClick.Invoke();
            });

            return categoryButton.Button;
        }

        public static Slider CreateSlider(Transform canvasTransform, Vector2 position, float width, float height, float minValue, float maxValue, float value, float increment, bool integerOnly, Action<float> onValueChange)
        {
            Slider slider = UnityEngine.Object.Instantiate(sliderPrefab, canvasTransform);
            slider.gameObject.SetActive(true);

            RectTransform t = slider.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(width, height);

            t.anchoredPosition = position;

            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = value;
            
            int decimals = 0;
            if (!integerOnly)
            {
                if (increment.ToString().Contains("."))
                    decimals = increment.ToString().Split('.')[1].Length;
                else
                    decimals = 2;
            }
             

            CreateText(slider.transform, new Vector2(-width, -45), 100, height, minValue.ToString());
            CreateText(slider.transform, new Vector2(-100, -45), 100, height, maxValue.ToString(), alignment:TextAnchor.MiddleRight);

            Text valueText = CreateText(slider.handleRect.transform, new Vector2(-50, 0), 100, height, string.Format(new NumberFormatInfo() { NumberDecimalDigits = decimals }, "{0:F}", value), alignment: TextAnchor.MiddleCenter);
            RectTransform vRect = valueText.GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.5f, 0f);
            vRect.anchorMax = new Vector2(0.5f, 0f);

            float oldValue = (int)value;
            slider.onValueChanged.AddListener((float _value) =>
            {
                float newValue = 0;

                if (_value - oldValue < increment / 2)
                    newValue = Mathf.Round(_value / increment) * increment;
                else if (_value - oldValue > increment / 2)
                    newValue = Mathf.Round(_value / increment) * increment;
                else
                    return;

                oldValue = newValue;

                slider.value = newValue;

                if (integerOnly)
                    valueText.text = newValue.ToString("n0");
                else
                    valueText.text = string.Format(new NumberFormatInfo() { NumberDecimalDigits = decimals}, "{0:F}", newValue);
                
                    

                onValueChange.Invoke(newValue);
            });

            return slider;
        }

        private void AddSlider(StepSliderConfig sliderConf, string category, string label)
        {
            GameObject panel = panelDict[category];
            Debug.Log(sliderConf.entry.SettingType.ToString());
            if (sliderConf.integerOnly)
            {
                CreateSlider(panel.transform, new Vector2(-640, -100 + yOffset - 40), 400, 20, sliderConf.min, sliderConf.max, (int)sliderConf.entry.BoxedValue, sliderConf.increment, sliderConf.integerOnly, (float val) =>
                {
                    sliderConf.entry.BoxedValue = (int)val;
                });
            }
            else
            {
                CreateSlider(panel.transform, new Vector2(-640, -100 + yOffset - 40), 400, 20, sliderConf.min, sliderConf.max, (float)sliderConf.entry.BoxedValue, sliderConf.increment, sliderConf.integerOnly, (float val) =>
                {
                    sliderConf.entry.BoxedValue = val;
                });
            }

            CreateText(panel.transform, new Vector2(-640, -100 + yOffset - 10), 400, 28, label);

            yOffset -= 130;
        }

        private void AddDropdown(ConfigEntryBase entry, string category, string label)
        {
            GameObject panel = panelDict[category];

            CreateDropdown(panel.transform, new Vector2(-640, -100 + yOffset - 40), 400, 70, Enum.GetNames(entry.BoxedValue.GetType()).ToList(), (int)entry.BoxedValue, (int val) =>
            {
                entry.BoxedValue = val;
            });

            CreateText(panel.transform, new Vector2(-640, -100 + yOffset - 10), 400, 28, label);

            yOffset -= 130;
        }

        private void AddCheckbox(ConfigEntryBase entry, string category, string label)
        {
            GameObject panel = panelDict[category];

            CreateToggle(panel.transform, new Vector2(-640, -100 + yOffset), 400, 60, label, (bool)entry.BoxedValue, (bool _checked) =>
            {
                entry.BoxedValue = _checked;
            });

            yOffset -= 80;
        }

        void GetSliderPrefab()
        {
            GameObject slider = SettingsPanel.transform.Find("AUDIO/master_volume/SET_sld_volume").gameObject;
            GameObject _s = UnityEngine.Object.Instantiate(slider);

            RectTransform rect = _s.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;

            var _slider = _s.GetComponent<Slider>();

            var c = _slider.colors;
            var fr = _slider.fillRect;
            var hr = _slider.handleRect;

            UnityEngine.Object.DestroyImmediate(_slider);

            var newSlider = _s.AddComponent<Slider>();
            newSlider.colors = c;
            newSlider.fillRect = fr;
            newSlider.handleRect = hr;

            sliderPrefab = newSlider;
            _s.SetActive(false);
            DontDestroyOnLoad(_s);
        }

        void GetTextPrefab()
        {
            GameObject text = SettingsPanel.transform.Find("ALLEGIANCE/lbl_baboon_qty").gameObject;
            GameObject _t = UnityEngine.Object.Instantiate(text);

            RectTransform rect = _t.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;

            textPrefab = _t.GetComponent<Text>();
            textPrefab.text = "";
            _t.SetActive(false);
            DontDestroyOnLoad(_t);
        }

        void GetDropdownPrefab()
        {
            GameObject dropdown = SettingsPanel.transform.Find("ALLEGIANCE/SET_drp_baboon_qty").gameObject;
            GameObject _dd = UnityEngine.Object.Instantiate(dropdown);

            RectTransform rect = _dd.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;

            dropdownPrefab = _dd.GetComponent<Dropdown>();
            dropdownPrefab.onValueChanged.RemoveAllListeners();

            dropdownPrefab.ClearOptions();
            _dd.SetActive(false);
            DontDestroyOnLoad(_dd);
        }

        void GetButtonPrefab()
        {
            GameObject backBtn = transform.Find("BackButton").gameObject;
            GameObject graphicButton = SettingsPanel.transform.Find("GRAPHICS/btn_opengraphicspanel").gameObject;

            GameObject _btn = UnityEngine.Object.Instantiate(graphicButton);
            //_btn.transform.localScale = Vector3.one * 2;

            var pls = _btn.GetComponent<Button>();
            var c = pls.colors;

            UnityEngine.Object.DestroyImmediate(pls);

            var newBtn = _btn.AddComponent<Button>();
            newBtn.colors = c;

            buttonPrefab = _btn.AddComponent<TrombButton>();

            buttonPrefab.Button = _btn.GetComponent<Button>();
            buttonPrefab.RectTransform = _btn.GetComponent<RectTransform>();
            buttonPrefab.Text = _btn.GetComponentInChildren<Text>();

            buttonPrefab.Button.onClick.RemoveAllListeners();

            buttonPrefab.RectTransform.anchoredPosition = backBtn.GetComponent<RectTransform>().anchoredPosition;
            _btn.SetActive(false);
            DontDestroyOnLoad(_btn);
        }

        void GetCheckboxPrefab()
        {
            GameObject checkBox = transform.Find("accessibility_panel/all_settiings/acc_jumpscares/toggle_jump").gameObject;
            GameObject _check = UnityEngine.Object.Instantiate(checkBox);

            checkboxPrefab = _check.AddComponent<TrombCheckbox>();

            checkboxPrefab.Toggle = _check.GetComponent<Toggle>();
            checkboxPrefab.RectTransform = _check.GetComponent<RectTransform>();
            checkboxPrefab.Label = _check.GetComponentInChildren<Text>();
            _check.SetActive(false);
            DontDestroyOnLoad(_check);
        }

        public void AddCategory(string _modName)
        {
            categoryCount++;

            var b = CreateButton(transform, new Vector2(340, -categoryCount * 60 - 170), 200, 40, _modName, () =>
            {
                SwitchPanel(_modName);
            });

            var t = b.GetComponent<RectTransform>();
            t.anchorMax = new Vector2(0, 1f);
            t.anchorMin = new Vector2(0, 1f);

            /*
            TrombButton categoryButton = UnityEngine.Object.Instantiate(buttonPrefab, transform);
            categoryButton.gameObject.SetActive(true);
            categoryButton.Text.text = _modName;
            categoryButton.Move(new Vector2(130, -categoryCount * 70 - 50));
            categoryButton.name = _modName + "_Button";
            categoryButton.CategoryID = _modName;
            categoryButton.Button.onClick.AddListener(() =>
            {
                SwitchPanel(categoryButton.CategoryID);
            });
            */

            GameObject copy = Instantiate(SettingsPanel, SettingsPanel.transform.position, SettingsPanel.transform.rotation, transform);
            copy.name = _modName + "_Panel";
            foreach (Transform child in copy.transform)
            {
                Destroy(child.gameObject);
            }

            panelDict.Add(_modName, copy);
        }

        public void SwitchPanel(string panelID)
        {
            transform.Find("header-Settings").GetComponentsInChildren<Text>().ToList().ForEach(t =>
            {
                t.text = panelID;
                t.GetComponent<RectTransform>().offsetMax = new Vector2(1000,0);
            });

            if (panelID != "Settings")
            {
                foreach (var panel in panelDict)
                {
                    if (panel.Key == panelID)
                        panel.Value.SetActive(true);
                    else
                        panel.Value.SetActive(false);
                }

                SettingsPanel.SetActive(false);
            }
            else
            {
                foreach (var panel in panelDict)
                {
                    panel.Value.SetActive(false);
                }

                SettingsPanel.SetActive(true);
            }


        }
    }
}
