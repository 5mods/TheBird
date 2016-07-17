using GTA;
using GTA.Native;

namespace GTA5M
{
    // Provided by InfamousSabre (http://gtaforums.com/topic/804802-identifying-cops-from-other-peds-cops-groupid/#entry1067688621)
    public enum PedType
    {
        Player = 1,
        Male = 4,
        Female = 5,
        Cop = 6,
        SWAT = 27,
        Animal = 28,
        Army = 29
    }

    public static class PedExtension
    {
        public static PedType Type(this Ped ped)
        {
           return (PedType)Function.Call<int>(Hash.GET_PED_TYPE, ped);
        }

        public static void Destroy(this Ped ped)
        {
            ped.MarkAsNoLongerNeeded();
            ped.IsPersistent = false;
            ped.AlwaysKeepTask = false;
        }
    }
}