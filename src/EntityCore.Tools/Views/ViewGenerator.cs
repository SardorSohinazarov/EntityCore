namespace EntityCore.Tools.Views
{
    public class ViewGenerator : Generator
    {
        protected HashSet<string> _usings;
        public ViewGenerator(Type entityType)
        {
            _usings = new HashSet<string>();
        }
    }
}
