using Microsoft.EntityFrameworkCore;

namespace EntityCore.Tools.Views
{
    public class View : Generator
    {
        private readonly Type _entityType;
        public View(Type entityType)
        {
            _entityType = entityType;
        }

        public List<(string name,string code)> Generate()
        {
            var views = new List<(string, string)>();
            var filter = new Filter(_entityType);
            views.Add(("Filter", filter.Generate()));
            var create = new Create(_entityType);
            views.Add(("Create", create.Generate()));
            var details = new Details(_entityType);
            views.Add(("Details", details.Generate()));

            return views;
        }
    }
}
