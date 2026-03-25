namespace CarlosReturn
{
    public class CarlosItem : Item
    {
        public Notebook notebook;

        public SoundObject crunch;

        public override bool Use(PlayerManager pm)
        {
            CoreGameManager.Instance.audMan.PlaySingle(crunch);
            CoreGameManager.Instance.sceneObject.manager.CollectNotebook(notebook);
            CoreGameManager.Instance.audMan.PlaySingle(notebook.audPickup);
            return true;
        }
    }
}