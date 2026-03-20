using Core.Model.ModelSystems;
using Core.Systems;

namespace Core.Model
{
    public class Ability : Entity, IHierarchyEntity
    {
        public EntId OwnerID { get; }

        public Ability(EntId ownerID)
        {
            OwnerID = ownerID;
            var hierarchySystem = GetSystem<IEntityHierarchySystem>();
            hierarchySystem.AddChild(ownerID, ID);
        }
    }
}
