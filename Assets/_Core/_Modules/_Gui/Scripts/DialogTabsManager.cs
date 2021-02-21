using System.Collections.Generic;
using System.Linq;
using Core.Gui.Data;
using Core.Gui.Controllers.Elements;
using UnityEngine;

namespace Core.Gui
{
    //[RequireComponent(typeof(AudioData))]
    public class DialogTabsManager : MonoBehaviour
    {
        [SerializeField] private List<TabCallbackEntry> _tabCallbacks;

        //private AudioData _audioData;

        private int _selectedTabIndx = -1;

        public void Awake()
        {
            foreach (var entry in _tabCallbacks)
                entry.Awake();

            //_audioData = GetComponent<AudioData>();
        }

        public void Init(int tabIndx = 0)
        {
            _selectedTabIndx = -1;
            if (_tabCallbacks == null || _tabCallbacks[tabIndx].Tab == null)
                return;
            OnTabSelected(_tabCallbacks[tabIndx].Tab.gameObject);
        }
        
        public int SelectedTab
        {
            get { return _selectedTabIndx; }
        }

        public DialogTab[] Tabs
        {
            get { return _tabCallbacks.Select(entry => entry.Tab).ToArray(); }
        }

        /// <summary>
        /// Установка вкладки
        /// </summary>
        /// <param name="selectedTab">GameObject - вкладка. Аргумент должен иметь тип UnityEngine.Object, чтобы корректно передаваться при нажатии кнопки</param>
        private void ArrangeTabs(GameObject selectedTab)
        {
            if (selectedTab == null)
                return;

            // сортировка вкладок по уровням.
            // сверху выбранная вкладка
            // за ней граница отображаемого контента
            // еще ниже прилежащие к выбранной вкладки
            // еще ниже все остальные
            for (int i = 0; i < _tabCallbacks.Count; i++)
            {
                var tab = _tabCallbacks[i].Tab;
                if (tab.gameObject == selectedTab)
                {
                    tab.Selected = true;
                    //tab.transform.SetAsLastSibling();
                }
                else
                {
                    tab.Selected = false;
                    //tab.transform.SetSiblingIndex(_tabCallbacks.Count - 1 - i);
                }
            }

            //selectedTab.transform.SetAsLastSibling();
        }

        public void OnTabSelected(Object selectedTabObj)
        {
            GameObject selectedTab = selectedTabObj as GameObject;
            int indx = -1;
            for (int i = 0; i < _tabCallbacks.Count; i++)
            {
                if (_tabCallbacks[i].Tab.gameObject == selectedTab)
                {
                    _tabCallbacks[i].Tab.Button.interactable = false;
                    indx = i;
                }
                else
                    _tabCallbacks[i].Tab.Button.interactable = _tabCallbacks[i].Tab.Enabled;
            }
            if (indx == _selectedTabIndx) 
                return;
            ArrangeTabs(selectedTab);
            _selectedTabIndx = indx;
            //_audioData.Play(AudioLibrary.TabSwitched);
            _tabCallbacks[indx].Callback();
        }
    }
}
