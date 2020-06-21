using System.Reflection;
using Harmony;

[ModTitle("Recycle Your Metal")]
[ModDescription("Allow you to smelt Bolt and Hinge in the Smelter.")]
[ModAuthor("Nundir")]
[ModIconUrl("https://github.com/Nundir/raft-recycleyourmetal-mod/raw/master/Icon.png")]
[ModWallpaperUrl("https://github.com/Nundir/raft-recycleyourmetal-mod/raw/master/Banner.png")]
[ModVersionCheckUrl("https://github.com/Nundir/raft-recycleyourmetal-mod/raw/master/version.txt")]
[ModVersion("1.0.0")]
[RaftVersion("10")]
public class RecycleYourMetal : Mod
{
    private const string logPrefix = "[<color=#0000ff>Recycle Your Metal</color>] ";
    private HarmonyInstance harmonyInstance;

    public void Start()
    {
        harmonyInstance = HarmonyInstance.Create("com.nundir.recycleyourmetal");
        harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

        RConsole.Log(logPrefix + "RecycleYourMetal loaded!");
    }
    
    public void onModUnload()
    {
        RConsole.Log(logPrefix + "RecycleYourMetal unloaded!");
        Destroy(gameObject);
    }
}

[HarmonyPatch(typeof(CookingStand)), HarmonyPatch("GetCookingSlotsForItem")]
internal class GetCookingSlotsForItemSmelterPatch
{
    private static void Prefix(ref Item_Base itemToInsert)
    {
        if (itemToInsert.UniqueName == "Bolt" || itemToInsert.UniqueName == "Hinge")
        {
            itemToInsert = ItemManager.GetItemByName("MetalOre");
        }
    }
}

[HarmonyPatch(typeof(CookingStand)), HarmonyPatch("InsertItem")]
internal class InsertItemSmelterPatch
{
    private static void Prefix(ref Item_Base itemToInsert, CookingSlot[] emptySlots, bool localPlayer)
    {
        if (itemToInsert.UniqueName == "Bolt" || itemToInsert.UniqueName == "Hinge")
        {
            itemToInsert = ItemManager.GetItemByName("MetalOre");
        }
    }
}