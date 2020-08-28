using SDG.Unturned;
using Stash.Commands;
using Stash.Database;
using Stash.Utils;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Stash
{
    class StashManager : MonoBehaviour
    {
        public void Awake()
        {
            Console.WriteLine("StashManager loaded");

            Commander.register(new CommandStash());
            ChatManager.onCheckPermissions += OnCheckedPermissions;
        }

        private void OnCheckedPermissions(SteamPlayer player, string text, ref bool shouldExecuteCommand, ref bool shouldList)
        {
            if (text.StartsWith("/stash"))
            {
                if (Main.Config.safeZoneOnly && !player.player.movement.isSafe)
                {
                    UnityThread.executeInUpdate(() =>
                    {
                        ChatManager.say(player.playerID.steamID, $"Must be in a safezone to view your stash", Color.cyan);
                    });

                    shouldExecuteCommand = false;
                    return;
                }
                shouldExecuteCommand = true;
            }
        }
    }
}
