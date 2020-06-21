using System.Reflection;
using Harmony;
using System;
using System.Collections.Generic;
using FMODUnity;
using Steamworks;
using UnityEngine;

[ModTitle("BetterCropPlots")]
[ModDescription("Add the option to harvest all plant with one click in a Crop Plot.")]
[ModAuthor("Nundir")]
[ModIconUrl("https://github.com/Nundir/raft-bettercropplot-mod/raw/master/Icon.png")]
[ModWallpaperUrl("https://github.com/Nundir/raft-bettercropplot-mod/raw/master/Banner.png")]
[ModVersionCheckUrl("https://github.com/Nundir/raft-bettercropplot-mod/raw/master/version.txt")]
[ModVersion("1.0.0")]
[RaftVersion("11")]
public class BetterCropPlot : Mod
{
    private const string logPrefix = "[<color=#7c5295>BetterCropPlots</color>] ";
    private HarmonyInstance harmonyInstance;
    private Network_Player playerNetwork = ComponentManager<Network_Player>.Value;

    public void Start()
    {
        ComponentManager<BetterCropPlot>.Value = this;

        harmonyInstance = HarmonyInstance.Create("com.nundir.bettercropplots");
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        RConsole.Log(logPrefix + " Loaded !");
    }

    public void OnModUnload()
    {
        RConsole.Log(logPrefix + " Unloaded !");
        Destroy(gameObject);
    }

    public void HarvestAll(PlantManager instance, Plant plant)
    {
        if (this.playerNetwork == null)
        {
            this.playerNetwork = ComponentManager<Network_Player>.Value;
        }

        foreach (PlantationSlot slot in plant.cropplot.plantationSlots)
        {
            if (slot.plant.FullyGrown() && slot.plant.harvestable && slot.plant.playerCanHarvest && plant.plantationSlotIndex != slot.plant.plantationSlotIndex)
            {
                Message_HarvestPlant message = new Message_HarvestPlant(Messages.PlantManager_HarvestPlant, instance, slot.plant, true);
                if (Semih_Network.IsHost)
                {
                    this.playerNetwork.Network.RPC(message, Target.Other, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
                    slot.plant.PullRoots();
                    this.playerNetwork.PickupScript.PickupItem(slot.plant.pickupComponent, true, true);
                    continue;
                }
                this.playerNetwork.SendP2P(message, EP2PSend.k_EP2PSendReliable, NetworkChannel.Channel_Game);
            }
        }
    }

    public void PlantAll(Cropplot cropplot, Plant plantPrefab, uint plantObjectIndex, bool waterCropAswell, bool treatAsPlayerPlanted)
    {
        if (this.playerNetwork == null)
        {
            this.playerNetwork = ComponentManager<Network_Player>.Value;
        }

        int itemCount = this.playerNetwork.Inventory.GetItemCount(plantPrefab.item);
        int slotCount = cropplot.plantationSlots.Count - 1;

        int count = itemCount > slotCount ? slotCount : itemCount;

        if (cropplot == null || plantPrefab == null) return;

        for (int i = 0; i < count; i++)
        {
            Plant plant = cropplot.PlantSeed(plantPrefab, plantObjectIndex, waterCropAswell, -1);
        }

        if (treatAsPlayerPlanted && this.playerNetwork.IsLocalPlayer)
        {
            this.playerNetwork.Inventory.RemoveItem(plantPrefab.item.UniqueName, count);
        }
    }
}

[HarmonyPatch(typeof(PlantManager)), HarmonyPatch("Harvest")]
internal class HarvestPlantManagerPatch
{
    private static void Prefix(PlantManager __instance, Plant plant)
    {
        if (MyInput.GetButton("Sprint"))
        {
            RConsole.Log("Test");
            ComponentManager<BetterCropPlot>.Value.HarvestAll(__instance, plant);
        }
    }
}

[HarmonyPatch(typeof(PlantManager)), HarmonyPatch("PlantSeed")]
internal class PlantSeedPlantManagerPatch
{
    private static void Prefix(Cropplot cropplot, Plant plantPrefab, uint plantObjectIndex, bool waterCropAswell, bool treatAsPlayerPlanted)
    {
        if (MyInput.GetButton("Sprint"))
        {
            ComponentManager<BetterCropPlot>.Value.PlantAll(cropplot, plantPrefab, plantObjectIndex, waterCropAswell, treatAsPlayerPlanted);
        }
    }
}

[HarmonyPatch(typeof(DisplayTextManager))]
[HarmonyPatch("ShowText")]
[HarmonyPatch(new Type[] { typeof(string), typeof(KeyCode), typeof(int), typeof(int), typeof(bool) })]
internal class ShowTextDisplayTextManagerPatch
{
    private static void Prefix(ref string text, KeyCode key, int displayIndex = 0, int priority = 0, bool clearAllTexts = true)
    {
        if (MyInput.GetButton("Sprint"))
        {
            if (text == Helper.GetTerm("Game/PlantItemX", true))
                text = "Plant all";
            else if (text == Helper.GetTerm("Game/Harvest", false))
                text = "Harvest all";
        }
        
    }
}