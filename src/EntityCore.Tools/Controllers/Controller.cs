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
    /// Entity -> Controller for CRUD operations for an Entity
    /// Service -> Controller for any service
    /// </summary>
    public enum ControllerType
    {
        Entity,
        Service
    }
}
