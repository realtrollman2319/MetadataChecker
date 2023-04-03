using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using uScript.API.Attributes;
using uScript.Core;
using uScript.Module.Main.Classes;

namespace MetadataChecker
{
    [ScriptTypeExtension(typeof(InventoryClass))]
    public class MetadataItemSpawner2
    {
        [ScriptFunction("addItemMetadata")]
        public static void addItemMetadata([ScriptInstance] ExpressionValue instance, ushort idGiven, ExpressionValue arrayGiven, byte qualityGiven = 100, byte amountGiven = 1) // For uScript module developers, you can set optional arguments by setting a default variable like qualityGiven, and amountGiven.
        {
            if (!(instance.Data is InventoryClass iclass)) return;

            Asset findAsset = Assets.find(EAssetType.ITEM, idGiven); // Find item

            if (findAsset != null) // Yes it exists
            {
                List<byte> bList = new List<byte>(); // Create list

                foreach (var arrayElement in arrayGiven.AsList) // Loop through
                {
                    byte parsedByte;
                    bool hasParsedArrayElementByte = Byte.TryParse(arrayElement, out parsedByte);
                    if (hasParsedArrayElementByte == true)
                    {
                        bList.Add(parsedByte); // Yay, valid array element! Let's add it.
                    }
                    else
                    {
                        Console.WriteLine("MetadataChecker module from uScript => One of the array elements failed to parse! Numbers must only be 0 to 255 and not a string or an object. Check the array elements and try again.", Console.ForegroundColor = ConsoleColor.Red); // Asshole put "255" lmao
                        Console.ResetColor();
                        return;
                    }
                }

                Item newItem = new Item(idGiven, 1, qualityGiven, bList.ToArray()); // Create an item
                ItemClass newUItemClass = new ItemClass(idGiven);
                int uItemLength = newUItemClass.Item.item.metadata.Length;

                if (uItemLength == 0) // Item dosen't have metadata (like a potato)
                {
                    Console.WriteLine("MetadataChecker module from uScript => Item's metadata dosen't contain an array! Spawn another item.", Console.ForegroundColor = ConsoleColor.Red); // metadata dosen't exist
                    Console.ResetColor();
                    return;
                }
                else if (uItemLength < bList.Count) // Item metadata is shorter than array given
                {
                    Console.WriteLine($"MetadataChecker module from uScript => Given array exceeds the given item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: {uItemLength} | Array count: {bList.Count}", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else if (uItemLength > bList.Count) // Item metadata is larger than array given
                {
                    Console.WriteLine($"MetadataChecker module from uScript => Given array is smaller than the item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: {uItemLength} | Array count: {bList.Count}", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else
                {
                    for (int i = 0; i < amountGiven; i++)
                    {
                        iclass.Inventory.tryAddItem(newItem, true);
                    }
                }
            }
            else // No it dosen't again lmao
            {
                Console.WriteLine("MetadataChecker module from uScript => Item not found.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }
        }
    }

    [ScriptTypeExtension(typeof(ItemClass))]
    public class MetadataChecker
    {
        [ScriptFunction("get_metadata")]
        public static ExpressionValue getMetadata([ScriptInstance] ExpressionValue instance)
        {
            if (!(instance.Data is ItemClass item)) return ExpressionValue.Null;
            var metadata = item.Item.item.metadata.Select(b => (ExpressionValue)b); // Converts each byte in the byte array to an ExpressionValue - Ster
            return ExpressionValue.Array(metadata);
        }
    }

    [ScriptModule("MetadataEditor")]
    public class MetadataEditorClass
    {
        [ScriptFunction("applyMetadata")]
        public static void applyMetadata(ItemClass itemGiven, string playerIdGiven, ExpressionValue arrayGiven)
        {
            List<byte> bList = new List<byte>(); // Create a list

            foreach (var arrayElement in arrayGiven.AsList) // Loop through
            {
                byte parsedByte;
                bool hasParsedArrayElementByte = Byte.TryParse(arrayElement, out parsedByte);
                if (hasParsedArrayElementByte == true) // It is a byte
                {
                    bList.Add(parsedByte); // Add it to the array
                }
                else // No it isn't again
                {
                    Console.WriteLine("MetadataChecker module from uScript => One of the array elements failed to parse! Numbers must only be 0 to 255 and not a string or an object. Check the array elements and try again.", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
            }

            ulong parsedPlayerID;
            bool hasParsedPlayerID = UInt64.TryParse(playerIdGiven, out parsedPlayerID); // Parse player ID
            if (hasParsedPlayerID != true)
            {
                Console.WriteLine("MetadataChecker module from uScript => The player ID failed to parse! Check the player ID argument and see if it's a string with numbers only included.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }

            Player playerGiven = PlayerTool.getPlayer(new CSteamID(parsedPlayerID)); // Get player from ID
            if (playerGiven == null)
            {
                Console.WriteLine("MetadataChecker module from uScript => Player dosen't exist on the game! Check if you've entered the correct player ID and the player is currently in-game.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }

            byte itemPage = 0;
            bool hasFoundItem = false;

            Items[] items = playerGiven.inventory.items;
            foreach (Items itemsElement in items) // Loop through player inventory
            {
                if (itemsElement.containsItem(itemGiven.Item)) // Does the item contain the item?
                {
                    itemPage = itemsElement.page; // Set page
                    hasFoundItem = true;
                    break;
                }
            }

            if (hasFoundItem == true)
            {
                int iLength = itemGiven.Item.item.metadata.Length;
                if (iLength == 0) // Item dosen't have metadata (like a potato)
                {
                    Console.WriteLine("MetadataChecker module from uScript => Item's metadata dosen't contain an array! Edit another item.", Console.ForegroundColor = ConsoleColor.Red); // metadata dosen't exist
                    Console.ResetColor();
                    return;
                }
                else if (iLength < bList.Count) // Item metadata is shorter than array given
                {
                    Console.WriteLine($"MetadataChecker module from uScript => Given array exceeds the given item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: {iLength} | Array count: {bList.Count}", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else if (iLength > bList.Count) // Item metadata is larger than array given
                {
                    Console.WriteLine($"MetadataChecker module from uScript => Given array is smaller than the item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: {iLength} | Array count: {bList.Count}", Console.ForegroundColor = ConsoleColor.Red);
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
            else
            {
                Console.WriteLine("MetadataChecker module from uScript => Item not found. Please report issue on GitHub.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }
        }

        [ScriptFunction("toByteArray")]
        public static ExpressionValue toByteArray(ushort ushortGiven) // Turn uInt16 to 2 bytes
        {
            byte[] bArray = BitConverter.GetBytes(ushortGiven);
            var byteArray = bArray.Select(b => (ExpressionValue)b); // Converts each byte in the byte array to an ExpressionValue - Ster
            return ExpressionValue.Array(byteArray);
        }

        [ScriptFunction("toUInt16")]
        public static ExpressionValue toUInt16(byte byte1given, byte byte2given) // Turn 2 bytes to uInt16
        {
            byte[] bytes = new byte[2] { byte1given, byte2given };
            return BitConverter.ToUInt16(bytes, 0);
        }
    }

    [ScriptTypeExtension(typeof(PlayerClass))]
    public class MetadataItemSpawner
    {
        [ScriptFunction("giveItemMetadata")]
        public static void giveItemMetadata([ScriptInstance] ExpressionValue instance, ushort idGiven, ExpressionValue arrayGiven, byte qualityGiven = 100, byte amountGiven = 1)
        {
            if (!(instance.Data is PlayerClass playerClass)) return;

            Asset findAsset = Assets.find(EAssetType.ITEM, idGiven); // Find the asset

            if (findAsset != null) // If it's not null (It exists)
            {
                List<byte> bList = new List<byte>(); // Create a byte list

                foreach (var arrayElement in arrayGiven.AsList)
                {
                    byte parsedByte;
                    bool hasParsedArrayElementByte = Byte.TryParse(arrayElement, out parsedByte); // Try to parse

                    if (hasParsedArrayElementByte == true) // Yes it exists
                    {
                        bList.Add(parsedByte);
                    }
                    else // No it dosen't lmao, now we tell player
                    {
                        Console.WriteLine("MetadataChecker module from uScript => One of the array elements failed to parse! Numbers must only be 0 to 255 and not a string or an object. Check the array elements and try again.", Console.ForegroundColor = ConsoleColor.Red);
                        Console.ResetColor();
                        return;
                    }
                }

                Item newItem = new Item(idGiven, 1, qualityGiven, bList.ToArray()); // Create an item
                ItemClass newUItemClass = new ItemClass(idGiven);
                int uItemLength = newUItemClass.Item.item.metadata.Length;

                if (uItemLength == 0) // Item dosen't have metadata (like a potato)
                {
                    Console.WriteLine("MetadataChecker module from uScript => Item's metadata dosen't contain an array! Spawn another item.", Console.ForegroundColor = ConsoleColor.Red); // Metadata dosen't exist
                    Console.ResetColor();
                    return;
                }
                else if (uItemLength < bList.Count) // Item metadata is shorter than array given
                {
                    Console.WriteLine($"MetadataChecker module from uScript => Given array exceeds the given item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: {uItemLength} | Array count: {bList.Count}", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else if (uItemLength > bList.Count) // Item metadata is larger than array given
                {
                    Console.WriteLine($"MetadataChecker module from uScript => Given array is smaller than the item's metadata! Edit your array to match the metadata you're editing. Item's metadata count: {uItemLength} | Array count: {bList.Count}", Console.ForegroundColor = ConsoleColor.Red);
                    Console.ResetColor();
                    return;
                }
                else
                {
                    for (int i = 0; i < amountGiven; i++)
                    {
                        playerClass.Player.inventory.forceAddItem(newItem, true); // Add the item to player's inventory | TBH I should've removed this method and replace it to inventory.addItemMetadata, but that would fuck up scripts
                    }
                }
            }
            else
            {
                Console.WriteLine("MetadataChecker module from uScript => Item not found.", Console.ForegroundColor = ConsoleColor.Red);
                Console.ResetColor();
                return;
            }
        }
    }
}