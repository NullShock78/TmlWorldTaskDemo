using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace WorldBlockDemo
{
    class WorldBlockDemoMW : ModWorld
    {
        private static volatile bool updating = false;
        private static volatile bool startUpdate = false;
        public static bool StartSwap()
        {
            if (!updating && !startUpdate)
            {
                startUpdate = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void PostUpdate()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                
                //To make sure it is not called on a multiplayer client, we start from here
                if (!updating && startUpdate)
                {
                    startUpdate = false;
                    updating = true;
                    SwapBlocksInWorld();
                }
            }
        }

        private static bool SwapBlocksInWorld()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Task.Run(async () =>
                {
                    await SwapTask();
                });
                return true;
            }
            else
            {
                return false;
            }
        }

        private static async Task SwapTask()
        {
            try
            {
                while (WorldGen.IsGeneratingHardMode)
                {
                    await Task.Yield();
                }

                var h = Main.tile.GetLength(1);
                var w = Main.tile.GetLength(0);
                WorldGen.noLiquidCheck = true;
                WorldGen.noTileActions = true;
                WorldGen.noMapUpdate = true;
                WorldGen.saveLock = true;

                const int MARGIN = 1;
                for (int x = MARGIN; x < w - MARGIN; x++)
                {
                    for (int y = MARGIN; y < h - MARGIN; y++)
                    {
                        var tile = Framing.GetTileSafely(x, y);
                        if (tile != null && tile.active() && tile.liquid == 0)
                        {
                            //Replace with whatever, using switch for demo
                            switch (tile.type) {
                                case TileID.Stone:
                                    tile.type = TileID.ObsidianBrick;
                                    break;
                                case TileID.Dirt:
                                    tile.type = TileID.GoldBrick;
                                    break;
                                default: 
                                    continue;
                            }
                        }
                    }
                }

                //Frame tiles
                for (int i = MARGIN; i < Main.maxTilesX - MARGIN; i++)
                {
                    for (int j = MARGIN; j < Main.maxTilesY - MARGIN; j++)
                    {
                        var temp = Framing.GetTileSafely(i, j);
                        if (temp != null && temp.active())
                        {
                            WorldGen.TileFrame(i, j, false, true);
                        }
                    }
                }

                if (Main.netMode == NetmodeID.Server)
                {
                    Netplay.ResetSections();
                }

                WorldGen.noLiquidCheck = false;
                WorldGen.noTileActions = false;
                WorldGen.noMapUpdate = false;
                WorldGen.saveLock = false;

                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    Main.NewText("World Changed", 50, 255, 130, false);
                }
                else if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("World Randomized"), new Color(50, 255, 130), -1);
                }

            }
            finally
            {
                EndSwap();
            }
        }

        private static void EndSwap()
        {
            updating = false;
        }

    }
}
