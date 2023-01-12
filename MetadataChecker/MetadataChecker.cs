using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using uScript.API.Attributes;
using uScript.Core;
using uScript.Module.Main.Classes;

namespace MetadataChecker
{
    [ScriptTypeExtension(typeof(ItemClass))]
    public class MetadataChecker
    {
        [ScriptFunction("get_metadata")]
        public static ExpressionValue metadata([ScriptInstance] ExpressionValue instance)
        {
            if (!(instance.Data is ItemClass item)) return 0;
            var metadata = item.Item.item.metadata.Select(b => (ExpressionValue)b); // Converts each byte in the byte array to an ExpressionValue
            return ExpressionValue.Array(metadata);
        }
    }

    [ScriptTypeExtension(typeof(PlayerClass))]
    public class MetadataItemSpawner
    {
        [ScriptFunction]
        public static void giveItemMetadata([ScriptInstance] ExpressionValue instance, ushort itemId, string metadataString)
        {
            if (!(instance.Data is PlayerClass playerClass)) return;
            if (string.IsNullOrEmpty(metadataString)) return;

            string[] bytes = metadataString.Split(',');
            List<byte> bList = new List<byte>();

            foreach (string b in bytes)
            {
                bList.Add(Byte.Parse(b));
            }

            Item newItem = new Item(itemId, 1, 100);
            newItem.metadata = bList.ToArray();

            playerClass.Player.inventory.forceAddItem(newItem, true);
        }
    }
}