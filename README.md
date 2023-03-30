# MetadataChecker [![](https://img.shields.io/github/downloads/realtrollman2319/MetadataChecker/total.svg)](https://github.com/realtrollman2319/MetadataChecker/releases)
Special thanks to Ster for helping me make this module!

# About
This module is for uScript2 for Unturned.
Its purpose is to add a extension that allows you to retrieve or edit the metadata of a gun, gas can, night vision or a canteen.
Useful for storing the durability of a suppressor inside a gun, save files and more.
```
item [Class]:
	+metadata            [get]      : array
	  
player [Class]:
	+giveItemMetadata(uInt16 id, array metadataArray, [byte quality], [byte amount])
    
inventory [Class]:
	+addItemMetadata(uInt16 id, array metadataArray, [byte quality], [byte amount])
    
MetadataEditor [Class]:
	+applyMetadata(object item, string playerId, array metadataArray)
	+toByteArray(uInt16 number)     : array
```

# Note:
It now supports arrays. Make sure it's the same count as the item's metadata you're editing.


# Additional information
Here's additional information about item metadatas.
https://steamcommunity.com/sharedfiles/filedetails/?id=2184421464

If you're having problem with this module, just let me know via discord.
735211881953886238
Or join the uScript discord.
https://discord.gg/86Zfvsjkdt

# Donations

If you feel grateful to donate, you can do so by clicking [here.](https://cgproductions-store.tebex.io/package/5222683)
