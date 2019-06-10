namespace CodeTableCaching
{
    /// <summary>
    /// Provides a method to overwrite this instance with the values (all properties) from the source object
    /// </summary>
    public interface IOverwritable
    {
        void Overwrite(object source);
    }
}
