using GTA;
using GTA.Native;

namespace GTA5M
{
    public static class EntityExtension
    {
        public static bool IsPed(this Entity entity)
        {
            return Function.Call<bool>(Hash.IS_ENTITY_A_PED, entity);
        }
    }
}