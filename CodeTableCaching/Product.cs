namespace CodeTableCaching
{
    public class Product : ProjectCode
    {

        public Product(string id, bool isActive, string description, string comment, int version)
            : base(id, isActive, comment, version)
        {
            _description = description;
        }

        public override string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
    }
}
