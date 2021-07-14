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
    public class ItemsCreationWindow : AssetsCreationWindow<ItemDefinition>
    {
        IMGUIContainer imageContainer;

        [MenuItem("Window/ItemsCreationWindow")]
        static void Init()
        {
            ItemsCreationWindow wnd = GetWindow<ItemsCreationWindow>();
            wnd.titleContent = new GUIContent("ItemsCreation");
        }

        protected override void CreateAssetsListView()
        {
            base.CreateAssetsListView();

            assetsListView.onSelectionChange += (enumerable) =>
            {
                 foreach(Object obj in enumerable)
                 {
                    imageContainer = contentSpace.Q<IMGUIContainer>("asset-icon");
                    imageContainer.Clear();

                    ItemDefinition asset = obj as ItemDefinition;

                    SerializedObject assetSO = new SerializedObject(asset);
                    SerializedProperty assetProperty = assetSO.GetIterator();
                    assetProperty.Next(true);

                    int count = 0;
                    while (assetProperty.NextVisible(false))
                    {                        
                        if (assetProperty.name == "ItemIcone")
                        {
                            assetPropertiesSpace.ElementAt(count).RegisterCallback<ChangeEvent<UnityEngine.Object>>((ev) =>
                            {
                                if (asset.ItemIcone != null)
                                    imageContainer.style.backgroundImage = asset.ItemIcone.texture;
                            });
                        }
                        count++;
                    }

                    if (asset.ItemIcone != null)
                        imageContainer.style.backgroundImage = asset.ItemIcone.texture;

                }
             };
            
            assetsListView.Refresh();
        }
    }
}
