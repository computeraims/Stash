using SDG.Unturned;
using Stash.Database;
using Stash.Models;
using Stash.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stash
{
    class StashManager : MonoBehaviour
    {
        private StashDB StashDB;
        public HyperCommand StashCommand;
        public void Awake()
        {
            Console.WriteLine("StashManager loaded");
            StashDB = new StashDB();
            //CommandManager.RegisterCommand(new CommandStash());
            ChatManager.onChatted += OnChatted;
            StashCommand = new CommandStash();
        }

        private void OnChatted(SteamPlayer player, EChatMode Mode, ref Color Color, ref bool isRich, string text, ref bool isVisible)
        {
            if (text[0] == '/')
            {
                isVisible = false;
                string[] InputArray = text.Split(' ');
                InputArray[0] = InputArray[0].Substring(1);
                string[] args = InputArray.Skip(1).ToArray();

                if (InputArray[0].ToLower() == "stash")
                {
                    StashCommand.execute(player.playerID.steamID, args);
                }
            }
        }

        public class CommandStash : HyperCommand
        {
            public CommandStash()
            {
                Name = "stash";
                Description = "Open your stash";
                Usage = "";
            }

            public override void execute(CSteamID executor, string[] args)
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
                stash.resize((byte) Main.Config.width, (byte) Main.Config.height);

                List<ItemJar> items = StashDB.GetItems(executor.ToString());
                //Console.WriteLine(items.Count);

                foreach (ItemJar item in items)
                {
                    ItemAsset itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, item.item.id);

                    try
                    {
                        if ((itemAsset.size_y + item.y - 1) > (byte) Main.Config.height)
                        {
                            byte x;
                            byte y;
                            byte rot;

                            if (stash.tryFindSpace(item.x, item.y, out x, out y, out rot))
                            {
                                stash.addItem(x, y, rot, item.item);
                            }
                        }
                        else
                        {
                            stash.addItem(item.x, item.y, item.rot, item.item);
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityThread.executeInUpdate(() =>
                        {
                            ChatManager.say(executor, $"Failed to get {itemAsset.itemName}, not enough free space", Color.cyan);
                        });
                    }

                    //Console.WriteLine($"Found Item: {item.item.id} {item.x} {item.y} {item.rot} {item.item.durability} {item.item.amount} {item.item.metadata}");
                }

                stash.onItemAdded = (byte page, byte index, ItemJar item) => {
                    //item.item.metadata = Encoding.UTF8.GetBytes("This is a test metadata");
                    //Console.WriteLine($"Item Added: {page} {index} {item.item.id} {item.x} {item.y}");
                    StashDB.AddItem(executor.ToString(), item);
                };

                stash.onItemRemoved = (byte page, byte index, ItemJar item) =>
                {
                    StashDB.DeleteItem(executor.ToString(), item);
                };

                stash.onItemUpdated = (byte page, byte index, ItemJar item) =>
                {
                    StashDB.UpdateItem(executor.ToString(), item);
                };

                ply.inventory.updateItems(7, stash);
                ply.inventory.sendStorage();
            }
        }
    }
}
