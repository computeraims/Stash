using Nito.AsyncEx;
using SDG.Unturned;
using Stash.Database;
using Stash.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Stash.Commands
{
    public class CommandStash : Command
    {
        protected override async void execute(CSteamID executor, string parameter)
        {
            Player ply = PlayerTool.getPlayer(executor);

            if (Main.Config.safeZoneOnly && !ply.movement.isSafe)
            {
                UnityThread.executeInUpdate(() =>
                {
                    ChatManager.say(executor, $"Stash can only be accessed inside a safezone", Color.cyan);
                });
                return;
            }

            Items stash = new Items(7);
            stash.resize((byte)Main.Config.width, (byte)Main.Config.height);

            //List<ItemJar> items = AsyncContext.Run(async () => await Task.Run(() => StashDB.GetItemsAsync(executor.ToString())));
            List<ItemJar> items = await Task.Run(() => StashDB.GetItemsAsync(executor.ToString()));
            foreach (ItemJar item in items)
            {
                ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, item.item.id);

                try
                {
                    stash.addItem(item.x, item.y, item.rot, new Item(item.item.id, item.item.amount, item.item.quality, item.item.metadata));
                }
                catch (Exception ex)
                {
                    UnityThread.executeInUpdate(() =>
                    {
                        ChatManager.say(executor, $"Failed to get {itemAsset.itemName}, not enough free space", Color.cyan);
                    });
                }
            }

            stash.onItemAdded = async (byte page, byte index, ItemJar item) => {
                await Task.Run(() => StashDB.AddItemAsync(executor.ToString(), item));
                //AsyncContext.Run(async () => await Task.Run(() => StashDB.AddItemAsync(executor.ToString(), item)));
            };

            stash.onItemRemoved = async (byte page, byte index, ItemJar item) =>
            {
                await Task.Run(() => StashDB.DeleteItemAsync(executor.ToString(), item));
                //AsyncContext.Run(async () => await Task.Run(() => StashDB.DeleteItemAsync(executor.ToString(), item)));
            };

            stash.onItemUpdated = async (byte page, byte index, ItemJar item) =>
            {
                await Task.Run(() => StashDB.DeleteItemAsync(executor.ToString(), item));
                //AsyncContext.Run(async () => await Task.Run(() => StashDB.UpdateItemAsync(executor.ToString(), item)));
            };

            ply.inventory.updateItems(7, stash);
            ply.inventory.sendStorage();
        }

        public CommandStash()
        {
            this.localization = new Local();
            this._command = "stash";
            this._info = "stash";
            this._help = "View your private stash";
        }
    }
}
