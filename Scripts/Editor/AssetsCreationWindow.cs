using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Game.ScriptableObjects.Core;
using Game.ScriptableObjects.Inventory;

namespace Game.Editor
{
    public abstract class AssetsCreationWindow<T> : EditorWindow
        where T : GameSO
    {
        protected T[] allAssets;
        protected ListView assetsListView;
        protected VisualElement assetPropertiesSpace;
        protected VisualElement contentSpace;

        protected virtual void OnEnable()
        {
            LoadAllAssets();
        }

        public virtual void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/VisualTreeAssets/ItemsCreationWindow.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            CreateAssetsListView();
            CreateAssetCreationView();
        }

        protected void LoadAllAssets() => allAssets = GameSO.GetAllAssetsOfType<T>();


        protected virtual void CreateAssetsListView()
        {
            contentSpace = rootVisualElement.Q<VisualElement>("content-space");
            assetsListView = contentSpace.Q<ListView>("assets-list");
            assetsListView.Clear();
            assetsListView.makeItem = () => new Label();
            assetsListView.bindItem = (element, i) => (element as Label).text = allAssets[i].name;

            assetsListView.itemsSource = allAssets;
            assetsListView.itemHeight = 16;
            assetsListView.selectionType = SelectionType.Single;

            assetsListView.onSelectionChange += (enumerable) =>
            {
                foreach (Object obj in enumerable)
                {
                    assetPropertiesSpace = contentSpace.Q<VisualElement>("asset-properties");
                    assetPropertiesSpace.Clear();

                    T asset = obj as T;

                    SerializedObject assetSO = new SerializedObject(asset);
                    SerializedProperty assetProperty = assetSO.GetIterator();
                    assetProperty.Next(true);

                    while (assetProperty.NextVisible(false))
                    {
                        PropertyField prop = new PropertyField(assetProperty);

                        prop.SetEnabled(assetProperty.name != "m_Script");
                        prop.Bind(assetSO);
                        assetPropertiesSpace.Add(prop);
                    }
                }
            };

            assetsListView.Refresh();
        }
        protected virtual void CreateAssetCreationView()
        {
            VisualElement creationSpace = rootVisualElement.Q<VisualElement>("creation-space");
            TextField creationText = rootVisualElement.Q<TextField>("new-asset-text");
            Button creationButton = creationSpace.Q<Button>("new-asset-button");
            creationButton.RegisterCallback<ClickEvent>(ev =>
            {
                GameSO.CreateSO(typeof(T), creationText.text);
                LoadAllAssets();
                CreateAssetsListView();
            });
        }
    }
}
