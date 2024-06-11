namespace Core.View
{
    public interface IViewController
    {
        public bool IsActive { get; }
        public void ActivateGameObject();
        public void DeactivateGameObject();
    }
}