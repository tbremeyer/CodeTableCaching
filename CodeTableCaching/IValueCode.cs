namespace CodeTableCaching
{
    namespace Core.Interfaces.Entities
    {
        public interface IValueCode
        {
            string Id { get; set; }

            string Value { get; set; }

            string InvalidValue { get; set; }
        }
    }

}
