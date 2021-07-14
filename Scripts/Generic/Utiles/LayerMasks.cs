using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.MonoBehaviours;
using Game.MonoBehaviours.Combat;
using Game.MonoBehaviours.Inventory;
using Game.MonoBehaviours.Interactable;

namespace Game.Generic.Utiles
{
    public static class LayerMasks
    {
        // Layer lookup names
        static Dictionary<System.Type, string> layerMasksNames = new Dictionary<System.Type, string>()
        {
            {typeof(Player), "Player"},
            {typeof(Enemy), "Enemies"},
            {typeof(DestroyableEntity), "Destroyable Entities"},
            {typeof(EnvironmentEntity), "Environment Entities"},
            {typeof(FracturatedObject), "Fractured Object"},
            {typeof(Item), "Items"},
            {typeof(JumpableWall), "Interactables"},
         };
        
        public static readonly string TERRAIN_LAYER_MASK = "Terrain";
        public static readonly string WORLD_BOUNDS_MASK = "WorldBounds";
        public static readonly string PLAYER_MASK = "Player";
        public static readonly string ENEMIES_MASK = "Enemies";

        public static LayerMask PathToTargetMask()
        {
            // Layers where collision is not detected
            return ~LayerMask.GetMask(PLAYER_MASK, layerMasksNames[typeof(Enemy)], layerMasksNames[typeof(FracturatedObject)], layerMasksNames[typeof(Item)], TERRAIN_LAYER_MASK);
        }

        public static LayerMask FieldOfViewMask()
        {
            // Layers where collision is not detected
            return ~LayerMask.GetMask(PLAYER_MASK, layerMasksNames[typeof(Enemy)], layerMasksNames[typeof(FracturatedObject)], layerMasksNames[typeof(Item)]);
        }

        public static LayerMask ClickablesMask()
        {
            // Layers where collision is detected
            return LayerMask.GetMask(layerMasksNames[typeof(Enemy)], layerMasksNames[typeof(DestroyableEntity)], layerMasksNames[typeof(Item)], "Interactables", TERRAIN_LAYER_MASK);
        }

        /// <summary>
        ///  Returns 0 "Default" if layer value is not found
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetLayerMaskValueFromType(System.Type type)
        {
            int layerValue = 0;
            string lookAtLayerName;
            if (layerMasksNames.TryGetValue(type, out lookAtLayerName))
            {
                layerValue = LayerMask.NameToLayer(lookAtLayerName);
                if (layerValue == -1)
                    layerValue = 0;
            }

            return layerValue;
        }
    }
}
