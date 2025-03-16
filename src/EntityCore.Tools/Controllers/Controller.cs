namespace EntityCore.Tools.Controllers
{
    /// <summary>
    /// Controller Generator
    /// </summary>
    public partial class Controller : Generator
    {
    }

    /// <summary>
    /// Controller Type : Entity or Service
    /// Entity -> Entityni CRUD operatsiyalari uchun controller
    /// Service -> Istalgan service uchun controller
    /// </summary>
    public enum ControllerType
    {
        Entity,
        Service
    }
}
