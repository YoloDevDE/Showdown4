using BepInEx;
using ZeepSDK.Storage;

namespace Showdown4.Domain.Entities;

public static class ModStorage
{
    public static IModStorage Storage { get; private set; }

    public static void Initialize(BaseUnityPlugin plugin)
    {
        Storage = StorageApi.CreateModStorage(plugin);
    }
}