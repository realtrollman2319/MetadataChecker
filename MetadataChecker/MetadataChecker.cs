using SDG.Unturned;
using Steamworks;
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
        public static ExpressionValue getMetadata([ScriptInstance] ExpressionValue instance)
        {
            if (!(instance.Data is ItemClass item)) return ExpressionValue.Null;
            var metadata = item.Item.item.metadata.Select(b => (ExpressionValue)b); // Converts each byte in the byte array to an ExpressionValue
            return ExpressionValue.Array(metadata);
        }
    }

    [ScriptTypeExtension(typeof(PlayerClass))]
    public class MetadataItemSpawner
    {
        [ScriptFunction]
        public static void giveItemMetadata([ScriptInstance] ExpressionValue instance, ushort itemId, ExpressionValue arrayGiven)
        {
            if (!(instance.Data is PlayerClass playerClass)) return;

            List<byte> bList = new List<byte>();

            foreach (var arrayElement in arrayGiven.AsList)
            {
                byte parsedByte;
                bool hasParsedArrayElementByte = Byte.TryParse(arrayElement, out parsedByte);

                if (hasParsedArrayElementByte == true)
                {
                    bList.Add(parsedByte);
                }
                else
                {
                    Console.WriteLine("MetadataChecker module from uScript => One of the array elements failed to parse! Numbers must only be 0 to 255 and not a string or an object. Check the array elements and try again.", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
            }

            Item newItem = new Item(itemId, 1, 100);
            newItem.metadata = bList.ToArray();

            playerClass.Player.inventory.forceAddItem(newItem, true);
        }
    }

    [ScriptModule("MetadataEditor")]
    public class MetadataEditorClass
    {
        [ScriptFunction]
        public static void applyMetadata(ItemClass itemGiven, string playerIdGiven, ExpressionValue arrayGiven)
        {
            List<byte> bList = new List<byte>();

            foreach (var arrayElement in arrayGiven.AsList)
            {
                byte parsedByte;
                bool hasParsedArrayElementByte = Byte.TryParse(arrayElement, out parsedByte);
                if (hasParsedArrayElementByte == true)
                {
                    bList.Add(parsedByte);
                }
                else
                {
                    Console.WriteLine("MetadataChecker module from uScript => One of the array elements failed to parse! Numbers must only be 0 to 255 and not a string or an object. Check the array elements and try again.", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
            }

            ulong parsedPlayerID;
            bool hasParsedPlayerID = UInt64.TryParse(playerIdGiven, out parsedPlayerID);
            if (hasParsedPlayerID != true)
            {
                Console.WriteLine("MetadataChecker module from uScript => The player ID failed to parse! Check the player ID argument and see if it's a string with numbers only included.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }

            Player playerGiven = PlayerTool.getPlayer(new CSteamID(parsedPlayerID));
            if (playerGiven == null)
            {
                Console.WriteLine("MetadataChecker module from uScript => Player dosen't exist on the game! Check if you've entered the correct player ID and the player is currently in-game.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }

            byte itemPage = 0;
            bool hasFoundItem = false;

            Items[] items = playerGiven.inventory.items;
            foreach (Items itemsElement in items)
            {
                if (itemsElement.containsItem(itemGiven.Item))
                {
                    itemPage = itemsElement.page;
                    hasFoundItem = true;
                    break;
                }
            }

            if (hasFoundItem != true)
            {
                Console.WriteLine("MetadataChecker module from uScript => Item not found. This should probably not happen unless the module creator made a error making this module.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }
            else
            {
                if (itemGiven.Item.item.metadata.Length == 0)
                {
                    Console.WriteLine("MetadataChecker module from uScript => Item's metadata dosen't contain an array! Edit another item.", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else if (itemGiven.Item.item.metadata.Length < bList.Count)
                {
                    Console.WriteLine("MetadataChecker module from uScript => Given array exceeds the given item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: " + itemGiven.Item.item.metadata.Length, Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else if (itemGiven.Item.item.metadata.Length > bList.Count)
                {
                    Console.WriteLine("MetadataChecker module from uScript => Given array is smaller than the item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: " + itemGiven.Item.item.metadata.Length, Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else
                {
                    itemGiven.Item.item.metadata = bList.ToArray();

                    playerGiven.equipment.state = bList.ToArray();
                    playerGiven.equipment.sendUpdateState();
                    playerGiven.inventory.sendUpdateInvState(itemPage, itemGiven.Item.x, itemGiven.Item.y, bList.ToArray());
                }
            }
        }
        
        [ScriptFunction]
        public static ExpressionValue toByteArray(int intGiven)
        {
            byte[] split = new byte[2];
            Buffer.BlockCopy(BitConverter.GetBytes(intGiven), 0, split, 0, 2);

            var byteArray = split.Select(b => (ExpressionValue)b); // Converts each byte in the byte array to an ExpressionValue - Ster
            return ExpressionValue.Array(byteArray);
        }
    }
}