namespace EntityCore.Tools.Views.Components
{
    public partial class PaginationComponent
    {
        public string Style()
        {
            return @".page-link {
    border-radius: 5px;
    border: none;
    font-weight: bold;
}

.page-link:hover {
    border: 1px solid gray;
    border-radius: 5px;
}

.page-item {
    margin: 1px;
}";
        }
    }
}
